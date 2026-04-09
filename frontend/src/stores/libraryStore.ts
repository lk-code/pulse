import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '../api'
import type { Library, ScanStatus } from '../types'

export const useLibraryStore = defineStore('library', () => {
  const libraries = ref<Library[]>([])
  const scanStatuses = ref<Map<number, ScanStatus>>(new Map())
  const pollIntervals = ref<Map<number, ReturnType<typeof setInterval>>>(new Map())

  async function fetchLibraries() {
    libraries.value = await api.libraries.list()
  }

  async function createLibrary(name: string, rootPath: string) {
    const lib = await api.libraries.create(name, rootPath)
    libraries.value.push(lib)
    return lib
  }

  async function deleteLibrary(id: number) {
    await api.libraries.delete(id)
    libraries.value = libraries.value.filter(l => l.id !== id)
    stopPolling(id)
  }

  async function startScan(id: number) {
    await api.libraries.scan(id)
    startPolling(id)
  }

  function startPolling(id: number) {
    if (pollIntervals.value.has(id)) return

    const interval = setInterval(async () => {
      const status = await api.libraries.scanStatus(id).catch(() => null)
      if (!status) return

      scanStatuses.value.set(id, status)

      if (!status.isScanning) {
        stopPolling(id)
        await fetchLibraries()
      }
    }, 1000)

    pollIntervals.value.set(id, interval)
  }

  function stopPolling(id: number) {
    const interval = pollIntervals.value.get(id)
    if (interval) {
      clearInterval(interval)
      pollIntervals.value.delete(id)
    }
  }

  async function fetchScanStatus(id: number) {
    const status = await api.libraries.scanStatus(id)
    scanStatuses.value.set(id, status)
    if (status.isScanning) startPolling(id)
    return status
  }

  return {
    libraries,
    scanStatuses,
    fetchLibraries,
    createLibrary,
    deleteLibrary,
    startScan,
    fetchScanStatus
  }
})
