<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { usePlaylistStore } from '../stores/playlistStore'
import { usePlayerStore } from '../stores/playerStore'
import { formatTime } from '../composables/useFormatTime'
import type { PlaylistTrackEntry } from '../types'

const route = useRoute()
const playlistStore = usePlaylistStore()
const player = usePlayerStore()

const playlistId = computed(() => Number(route.params.id))
const entries = ref<PlaylistTrackEntry[]>([])
const loading = ref(true)
const draggingIndex = ref<number | null>(null)
const dragOverIndex = ref<number | null>(null)

const playlist = computed(() =>
  playlistStore.playlists.find(p => p.id === playlistId.value)
)

onMounted(async () => {
  try {
    await playlistStore.fetchPlaylists()
    entries.value = await playlistStore.fetchPlaylistTracks(playlistId.value)
  } finally {
    loading.value = false
  }
})

function playAll() {
  const tracks = entries.value.map(e => e.track)
  if (!tracks.length) return
  player.setQueue(tracks, 0)
  player.isPlaying = true
}

async function removeTrack(trackId: number) {
  await playlistStore.removeTrackFromPlaylist(playlistId.value, trackId)
  entries.value = entries.value.filter(e => e.track.id !== trackId)
}

function onDragStart(index: number) {
  draggingIndex.value = index
}

function onDragOver(e: DragEvent, index: number) {
  e.preventDefault()
  dragOverIndex.value = index
}

async function onDrop(index: number) {
  if (draggingIndex.value === null || draggingIndex.value === index) {
    draggingIndex.value = null
    dragOverIndex.value = null
    return
  }

  const trackId = entries.value[draggingIndex.value].track.id
  await playlistStore.reorderTrack(playlistId.value, trackId, index)
  entries.value = [...playlistStore.currentPlaylistTracks]
  draggingIndex.value = null
  dragOverIndex.value = null
}
</script>

<template>
  <div class="p-6">
    <div class="flex items-center gap-4 mb-8">
      <div class="w-32 h-32 rounded-xl bg-[#1e1e2e] flex items-center justify-center flex-shrink-0">
        <svg class="w-12 h-12 text-slate-500" fill="none" stroke="currentColor" stroke-width="1.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
          <path d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3"/>
        </svg>
      </div>
      <div>
        <p class="text-slate-400 text-sm mb-1">Playlist</p>
        <h1 class="text-3xl font-bold text-white mb-3">{{ playlist?.name ?? '' }}</h1>
        <button
          @click="playAll"
          class="px-5 py-2 bg-violet-600 hover:bg-violet-500 text-white rounded-full text-sm font-medium transition-colors"
        >
          Play all
        </button>
      </div>
    </div>

    <div v-if="loading" class="text-slate-400">Loading...</div>
    <div v-else-if="entries.length === 0" class="text-slate-400">No tracks in this playlist.</div>
    <div v-else>
      <div
        v-for="(entry, i) in entries"
        :key="entry.track.id"
        draggable="true"
        @dragstart="onDragStart(i)"
        @dragover="onDragOver($event, i)"
        @drop="onDrop(i)"
        class="flex items-center gap-3 px-4 py-2.5 rounded-lg transition-colors group cursor-grab active:cursor-grabbing"
        :class="[
          dragOverIndex === i ? 'bg-violet-600/20 border border-violet-500/40' : 'hover:bg-white/5',
          player.currentTrack?.id === entry.track.id ? 'bg-violet-600/20' : ''
        ]"
      >
        <svg class="w-3 h-5 text-slate-600 flex-shrink-0" fill="currentColor" viewBox="0 0 6 20">
          <circle cx="1.5" cy="4" r="1.5"/><circle cx="4.5" cy="4" r="1.5"/>
          <circle cx="1.5" cy="10" r="1.5"/><circle cx="4.5" cy="10" r="1.5"/>
          <circle cx="1.5" cy="16" r="1.5"/><circle cx="4.5" cy="16" r="1.5"/>
        </svg>

        <span class="text-slate-500 text-sm w-6 text-right flex-shrink-0">{{ i + 1 }}</span>

        <div class="flex-1 min-w-0">
          <p class="truncate text-sm" :class="player.currentTrack?.id === entry.track.id ? 'text-violet-300' : 'text-white'">
            {{ entry.track.title }}
          </p>
          <p class="truncate text-xs text-slate-400">{{ entry.track.artist }}</p>
        </div>

        <p class="text-slate-500 text-sm tabular-nums">{{ formatTime(entry.track.durationSeconds) }}</p>

        <button
          @click.stop="removeTrack(entry.track.id)"
          class="opacity-0 group-hover:opacity-100 p-1 text-slate-500 hover:text-red-400 transition-all"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>
