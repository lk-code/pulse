<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useLibraryStore } from '../stores/libraryStore'

const libraryStore = useLibraryStore()
const newName = ref('')
const newPath = ref('')
const adding = ref(false)
const error = ref('')

onMounted(async () => {
  await libraryStore.fetchLibraries()
  for (const lib of libraryStore.libraries) {
    libraryStore.fetchScanStatus(lib.id)
  }
})

async function addLibrary() {
  if (!newName.value.trim() || !newPath.value.trim()) return
  adding.value = true
  error.value = ''
  try {
    await libraryStore.createLibrary(newName.value.trim(), newPath.value.trim())
    newName.value = ''
    newPath.value = ''
  } catch (e) {
    error.value = 'Failed to add library.'
  } finally {
    adding.value = false
  }
}

async function deleteLibrary(id: number) {
  await libraryStore.deleteLibrary(id)
}

async function scanLibrary(id: number) {
  await libraryStore.startScan(id)
}

function scanStatus(id: number) {
  return libraryStore.scanStatuses.get(id)
}
</script>

<template>
  <div class="p-6 max-w-2xl">
    <h1 class="text-2xl font-bold text-white mb-8">Settings</h1>

    <section class="mb-8">
      <h2 class="text-lg font-semibold text-white mb-4">Add Library</h2>
      <div class="space-y-3">
        <input
          v-model="newName"
          placeholder="Library name"
          class="w-full bg-[#1e1e2e] border border-[#2a2a3e] text-white text-sm rounded-lg px-3 py-2.5 focus:outline-none focus:border-violet-500 placeholder-slate-500"
        />
        <input
          v-model="newPath"
          placeholder="/media or C:\Media"
          class="w-full bg-[#1e1e2e] border border-[#2a2a3e] text-white text-sm rounded-lg px-3 py-2.5 focus:outline-none focus:border-violet-500 placeholder-slate-500"
        />
        <p v-if="error" class="text-red-400 text-sm">{{ error }}</p>
        <button
          @click="addLibrary"
          :disabled="adding || !newName.trim() || !newPath.trim()"
          class="px-5 py-2.5 bg-violet-600 hover:bg-violet-500 disabled:opacity-50 text-white text-sm rounded-lg transition-colors font-medium"
        >
          Add Library
        </button>
      </div>
    </section>

    <section>
      <h2 class="text-lg font-semibold text-white mb-4">Libraries</h2>

      <div v-if="libraryStore.libraries.length === 0" class="text-slate-400">
        No libraries configured.
      </div>

      <div v-else class="space-y-3">
        <div
          v-for="lib in libraryStore.libraries"
          :key="lib.id"
          class="bg-[#13131a] border border-[#1e1e2e] rounded-xl p-4"
        >
          <div class="flex items-start justify-between gap-4 mb-3">
            <div class="min-w-0">
              <p class="text-white font-medium">{{ lib.name }}</p>
              <p class="text-slate-500 text-sm truncate">{{ lib.rootPath }}</p>
              <p v-if="lib.lastScannedAt" class="text-slate-600 text-xs mt-1">
                Last scanned: {{ new Date(lib.lastScannedAt).toLocaleString() }}
              </p>
            </div>
            <div class="flex items-center gap-2 flex-shrink-0">
              <button
                @click="scanLibrary(lib.id)"
                :disabled="scanStatus(lib.id)?.isScanning"
                class="px-3 py-1.5 bg-[#1e1e2e] hover:bg-[#2a2a3e] disabled:opacity-50 text-white text-xs rounded-lg transition-colors"
              >
                {{ scanStatus(lib.id)?.isScanning ? 'Scanning...' : 'Scan' }}
              </button>
              <button
                @click="deleteLibrary(lib.id)"
                class="px-3 py-1.5 bg-red-900/30 hover:bg-red-900/50 text-red-400 text-xs rounded-lg transition-colors"
              >
                Delete
              </button>
            </div>
          </div>

          <div v-if="scanStatus(lib.id)?.isScanning" class="space-y-1.5">
            <div class="flex justify-between text-xs text-slate-400">
              <span class="truncate mr-2">{{ scanStatus(lib.id)?.currentFile ?? 'Starting...' }}</span>
              <span class="flex-shrink-0">
                {{ scanStatus(lib.id)?.processedFiles }} / {{ scanStatus(lib.id)?.totalFiles }}
              </span>
            </div>
            <div class="h-1 bg-[#1e1e2e] rounded-full overflow-hidden">
              <div
                class="h-full bg-violet-500 transition-all duration-300 rounded-full"
                :style="{
                  width: scanStatus(lib.id)!.totalFiles > 0
                    ? (scanStatus(lib.id)!.processedFiles / scanStatus(lib.id)!.totalFiles * 100) + '%'
                    : '0%'
                }"
              />
            </div>
          </div>

          <div v-else-if="scanStatus(lib.id)" class="text-xs text-slate-500">
            {{ scanStatus(lib.id)?.trackCount }} tracks
          </div>
        </div>
      </div>
    </section>
  </div>
</template>
