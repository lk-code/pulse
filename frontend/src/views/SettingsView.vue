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
  } catch {
    error.value = 'Failed to add library. Check the path and try again.'
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
  <div style="padding: 2.5rem; max-width: 640px;">
    <h1 class="text-2xl font-bold text-white tracking-tight" style="margin-bottom: 2.5rem;">Settings</h1>

    <section style="margin-bottom: 2.5rem;">
      <p class="text-xs font-semibold uppercase tracking-widest" style="color: rgba(255,255,255,0.3); margin-bottom: 1rem;">
        Add Library
      </p>

      <div class="rounded-2xl" style="background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.07); padding: 1.75rem;">
        <div style="margin-bottom: 1.25rem;">
          <label class="block text-xs font-semibold uppercase tracking-wider" style="color: rgba(255,255,255,0.35); margin-bottom: 0.5rem;">
            Name
          </label>
          <input
            v-model="newName"
            placeholder="My Music"
            class="w-full text-white text-sm rounded-xl focus:outline-none transition-all"
            style="background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.09); padding: 0.75rem 1rem; color: white;"
          />
        </div>
        <div style="margin-bottom: 1.25rem;">
          <label class="block text-xs font-semibold uppercase tracking-wider" style="color: rgba(255,255,255,0.35); margin-bottom: 0.5rem;">
            Path
          </label>
          <input
            v-model="newPath"
            placeholder="/media"
            class="w-full text-white text-sm rounded-xl focus:outline-none transition-all font-mono"
            style="background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.09); padding: 0.75rem 1rem; color: white;"
          />
          <p class="text-xs" style="color: rgba(255,255,255,0.2); margin-top: 0.4rem;">Absolute path inside the container, e.g. <span style="font-family: monospace;">/media</span> or <span style="font-family: monospace;">/media/Rock</span></p>
        </div>
        <p v-if="error" class="text-sm" style="color: rgba(248,113,113,0.8); margin-bottom: 1rem;">{{ error }}</p>
        <button
          @click="addLibrary"
          :disabled="adding || !newName.trim() || !newPath.trim()"
          class="w-full text-sm font-semibold text-white rounded-xl transition-all duration-200 disabled:opacity-40"
          style="background: linear-gradient(135deg, #7c3aed, #a855f7); padding: 0.75rem; margin-top: 0.5rem;"
        >
          {{ adding ? 'Adding…' : 'Add Library' }}
        </button>
      </div>
    </section>

    <section>
      <p class="text-xs font-semibold uppercase tracking-widest" style="color: rgba(255,255,255,0.3); margin-bottom: 1rem;">
        Libraries
      </p>

      <div
        v-if="libraryStore.libraries.length === 0"
        class="rounded-2xl text-center"
        style="background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.07); padding: 3rem 1.5rem;"
      >
        <p class="text-sm" style="color: rgba(255,255,255,0.25);">No libraries configured</p>
      </div>

      <div
        v-else
        class="rounded-2xl overflow-hidden"
        style="background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.07);"
      >
        <div
          v-for="(lib, i) in libraryStore.libraries"
          :key="lib.id"
          :style="i < libraryStore.libraries.length - 1 ? 'border-bottom: 1px solid rgba(255,255,255,0.05);' : ''"
          style="padding: 1.5rem 1.75rem;"
        >
          <div class="flex items-start justify-between gap-4" style="margin-bottom: 0.75rem;">
            <div class="min-w-0" style="display: flex; flex-direction: column; gap: 0.35rem;">
              <p class="text-sm font-semibold" style="color: rgba(255,255,255,0.85);">{{ lib.name }}</p>
              <p class="text-xs font-mono truncate" style="color: rgba(255,255,255,0.3);">{{ lib.rootPath }}</p>
              <p v-if="lib.lastScannedAt" class="text-xs" style="color: rgba(255,255,255,0.2);">
                Last scanned {{ new Date(lib.lastScannedAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }) }}
              </p>
            </div>
            <div class="flex items-center flex-shrink-0" style="gap: 0.625rem; padding-top: 0.125rem;">
              <button
                @click="scanLibrary(lib.id)"
                :disabled="scanStatus(lib.id)?.isScanning"
                class="flex items-center rounded-xl text-xs font-semibold transition-all duration-200 disabled:opacity-40"
                style="gap: 0.375rem; padding: 0.5rem 0.875rem; background: rgba(255,255,255,0.07); color: rgba(255,255,255,0.6); border: 1px solid rgba(255,255,255,0.1);"
              >
                <svg
                  class="w-3 h-3"
                  :class="scanStatus(lib.id)?.isScanning ? 'animate-spin' : ''"
                  fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24"
                  stroke-linecap="round" stroke-linejoin="round"
                >
                  <path d="M21 12a9 9 0 11-6.219-8.56"/>
                </svg>
                {{ scanStatus(lib.id)?.isScanning ? 'Scanning' : 'Scan' }}
              </button>
              <button
                @click="deleteLibrary(lib.id)"
                class="rounded-xl text-xs font-semibold transition-all duration-200"
                style="padding: 0.5rem 0.875rem; background: rgba(239,68,68,0.1); color: rgba(252,165,165,0.7); border: 1px solid rgba(239,68,68,0.15);"
              >
                Delete
              </button>
            </div>
          </div>

          <div v-if="scanStatus(lib.id)?.isScanning" style="display: flex; flex-direction: column; gap: 0.625rem; margin-top: 1rem;">
            <div class="flex justify-between items-center">
              <p class="text-xs truncate" style="color: rgba(255,255,255,0.3); margin-right: 0.75rem;">
                {{ scanStatus(lib.id)?.currentFile ?? 'Starting…' }}
              </p>
              <p class="text-xs flex-shrink-0 tabular-nums" style="color: rgba(255,255,255,0.25);">
                {{ scanStatus(lib.id)?.processedFiles }} / {{ scanStatus(lib.id)?.totalFiles }}
              </p>
            </div>
            <div class="rounded-full overflow-hidden" style="height: 2px; background: rgba(255,255,255,0.07);">
              <div
                class="h-full rounded-full transition-all duration-300"
                style="background: linear-gradient(90deg, #7c3aed, #a855f7);"
                :style="{
                  width: scanStatus(lib.id)!.totalFiles > 0
                    ? (scanStatus(lib.id)!.processedFiles / scanStatus(lib.id)!.totalFiles * 100) + '%'
                    : '2%'
                }"
              />
            </div>
          </div>

          <div v-else-if="scanStatus(lib.id)" class="text-xs" style="color: rgba(255,255,255,0.2); margin-top: 0.5rem;">
            {{ scanStatus(lib.id)?.trackCount }} tracks indexed
          </div>
        </div>
      </div>
    </section>
  </div>
</template>
