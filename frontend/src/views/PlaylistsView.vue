<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { usePlaylistStore } from '../stores/playlistStore'

const router = useRouter()
const playlistStore = usePlaylistStore()
const newName = ref('')
const creating = ref(false)
const showInput = ref(false)

onMounted(() => playlistStore.fetchPlaylists())

async function createPlaylist() {
  if (!newName.value.trim()) return
  creating.value = true
  try {
    const playlist = await playlistStore.createPlaylist(newName.value.trim())
    newName.value = ''
    showInput.value = false
    router.push(`/playlists/${playlist.id}`)
  } finally {
    creating.value = false
  }
}

async function deletePlaylist(id: number) {
  await playlistStore.deletePlaylist(id)
}
</script>

<template>
  <div class="p-7">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-white tracking-tight">Playlists</h1>
      <button
        @click="showInput = !showInput"
        class="flex items-center gap-1.5 px-3.5 py-1.5 rounded-full text-sm font-medium transition-all duration-200"
        style="background: rgba(124,58,237,0.2); color: rgba(196,181,253,0.9); border: 1px solid rgba(124,58,237,0.3);"
      >
        <svg class="w-3.5 h-3.5 transition-transform duration-200" :class="showInput ? 'rotate-45' : ''" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
          <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
        </svg>
        New
      </button>
    </div>

    <div
      v-if="showInput"
      class="mb-5 rounded-2xl p-4"
      style="background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.08);"
    >
      <p class="text-white/50 text-xs font-semibold uppercase tracking-wider mb-3">New Playlist</p>
      <div class="flex gap-2">
        <input
          v-model="newName"
          @keyup.enter="createPlaylist"
          @keyup.escape="showInput = false; newName = ''"
          placeholder="Playlist name…"
          autofocus
          class="flex-1 text-white text-sm rounded-xl px-3.5 py-2.5 focus:outline-none placeholder-white/20 transition-all"
          style="background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.08);"
        />
        <button
          @click="createPlaylist"
          :disabled="creating || !newName.trim()"
          class="px-4 py-2 rounded-xl text-sm font-semibold disabled:opacity-40 transition-all duration-200"
          style="background: linear-gradient(135deg, #7c3aed, #a855f7);"
        >
          Create
        </button>
      </div>
    </div>

    <div
      v-if="playlistStore.playlists.length === 0"
      class="flex flex-col items-center justify-center mt-20 gap-4"
    >
      <div class="w-16 h-16 rounded-2xl flex items-center justify-center" style="background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.07);">
        <svg class="w-7 h-7 text-white/20" fill="none" stroke="currentColor" stroke-width="1.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
          <path d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75zm.375 0a.375.375 0 11-.75 0 .375.375 0 01.75 0zM3.75 12h.007v.008H3.75V12zm.375 0a.375.375 0 11-.75 0 .375.375 0 01.75 0zm-.375 5.25h.007v.008H3.75v-.008zm.375 0a.375.375 0 11-.75 0 .375.375 0 01.75 0z"/>
        </svg>
      </div>
      <div class="text-center">
        <p class="text-white/60 font-medium">No playlists yet</p>
        <p class="text-white/25 text-sm mt-1">Create one to get started</p>
      </div>
    </div>

    <div
      v-else
      class="rounded-2xl overflow-hidden"
      style="background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.06);"
    >
      <div
        v-for="(playlist, i) in playlistStore.playlists"
        :key="playlist.id"
        class="flex items-center gap-3.5 px-5 py-4 hover:bg-white/[0.04] transition-colors group cursor-pointer"
        :class="i < playlistStore.playlists.length - 1 ? 'border-b border-white/[0.04]' : ''"
        @click="router.push(`/playlists/${playlist.id}`)"
      >
        <div
          class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
          style="background: linear-gradient(135deg, rgba(124,58,237,0.25), rgba(168,85,247,0.15)); border: 1px solid rgba(124,58,237,0.2);"
        >
          <svg class="w-4.5 h-4.5 text-violet-400" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <path d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3"/>
          </svg>
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-white/85 font-medium text-sm truncate">{{ playlist.name }}</p>
          <p class="text-white/30 text-xs mt-0.5">{{ playlist.trackCount }} tracks</p>
        </div>
        <div class="flex items-center gap-1.5">
          <button
            @click.stop="deletePlaylist(playlist.id)"
            class="p-1.5 rounded-lg text-white/0 group-hover:text-white/25 hover:!text-red-400 hover:bg-red-400/10 transition-all duration-150"
          >
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 01-2 2H8a2 2 0 01-2-2L5 6"/><path d="M10 11v6M14 11v6M9 6V4h6v2"/>
            </svg>
          </button>
          <svg class="w-3.5 h-3.5 text-white/15 group-hover:text-white/30 transition-colors" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <path d="M9 18l6-6-6-6"/>
          </svg>
        </div>
      </div>
    </div>
  </div>
</template>
