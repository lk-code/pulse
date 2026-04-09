<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { usePlaylistStore } from '../stores/playlistStore'

const router = useRouter()
const playlistStore = usePlaylistStore()
const newName = ref('')
const creating = ref(false)

onMounted(() => playlistStore.fetchPlaylists())

async function createPlaylist() {
  if (!newName.value.trim()) return
  creating.value = true
  try {
    await playlistStore.createPlaylist(newName.value.trim())
    newName.value = ''
  } finally {
    creating.value = false
  }
}

async function deletePlaylist(id: number) {
  await playlistStore.deletePlaylist(id)
}
</script>

<template>
  <div class="p-6">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-white">Playlists</h1>
    </div>

    <div class="flex gap-2 mb-8 max-w-sm">
      <input
        v-model="newName"
        @keyup.enter="createPlaylist"
        placeholder="New playlist name..."
        class="flex-1 bg-[#1e1e2e] border border-[#2a2a3e] text-white text-sm rounded-lg px-3 py-2 focus:outline-none focus:border-violet-500 placeholder-slate-500"
      />
      <button
        @click="createPlaylist"
        :disabled="creating || !newName.trim()"
        class="px-4 py-2 bg-violet-600 hover:bg-violet-500 disabled:opacity-50 text-white text-sm rounded-lg transition-colors"
      >
        Create
      </button>
    </div>

    <div v-if="playlistStore.playlists.length === 0" class="text-slate-400">
      No playlists yet.
    </div>

    <div v-else class="space-y-1">
      <div
        v-for="playlist in playlistStore.playlists"
        :key="playlist.id"
        class="flex items-center gap-3 px-4 py-3 rounded-lg hover:bg-white/5 transition-colors group cursor-pointer"
        @click="router.push(`/playlists/${playlist.id}`)"
      >
        <div class="w-10 h-10 rounded bg-[#1e1e2e] flex items-center justify-center flex-shrink-0">
          <svg class="w-5 h-5 text-slate-400" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <path d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3"/>
          </svg>
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-white font-medium">{{ playlist.name }}</p>
          <p class="text-slate-400 text-sm">{{ playlist.trackCount }} tracks</p>
        </div>
        <button
          @click.stop="deletePlaylist(playlist.id)"
          class="opacity-0 group-hover:opacity-100 p-1.5 text-slate-500 hover:text-red-400 transition-all"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 01-2 2H8a2 2 0 01-2-2L5 6"/><path d="M10 11v6M14 11v6"/><path d="M9 6V4h6v2"/>
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>
