<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePlaylistStore } from '../stores/playlistStore'
import { usePlayerStore } from '../stores/playerStore'
import { formatTime } from '../composables/useFormatTime'
import type { PlaylistTrackEntry } from '../types'

const route = useRoute()
const router = useRouter()
const playlistStore = usePlaylistStore()
const player = usePlayerStore()

const playlistId = computed(() => Number(route.params.id))
const entries = ref<PlaylistTrackEntry[]>([])
const loading = ref(true)
const draggingIndex = ref<number | null>(null)
const dragOverIndex = ref<number | null>(null)

const playlist = computed(() => playlistStore.playlists.find(p => p.id === playlistId.value))
const totalDuration = computed(() => entries.value.reduce((s, e) => s + e.track.durationSeconds, 0))

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

function playFromIndex(i: number) {
  const tracks = entries.value.map(e => e.track)
  player.setQueue(tracks, i)
  player.isPlaying = true
}

async function removeTrack(trackId: number) {
  await playlistStore.removeTrackFromPlaylist(playlistId.value, trackId)
  entries.value = entries.value.filter(e => e.track.id !== trackId)
}

function onDragStart(i: number) { draggingIndex.value = i }
function onDragOver(e: DragEvent, i: number) { e.preventDefault(); dragOverIndex.value = i }

async function onDrop(i: number) {
  if (draggingIndex.value === null || draggingIndex.value === i) {
    draggingIndex.value = null; dragOverIndex.value = null; return
  }
  const trackId = entries.value[draggingIndex.value].track.id
  await playlistStore.reorderTrack(playlistId.value, trackId, i)
  entries.value = [...playlistStore.currentPlaylistTracks]
  draggingIndex.value = null; dragOverIndex.value = null
}
</script>

<template>
  <div class="flex flex-col min-h-full">
    <div class="relative overflow-hidden flex-shrink-0" style="min-height: 200px;">
      <div class="absolute inset-0" style="background: linear-gradient(135deg, rgba(124,58,237,0.12) 0%, rgba(8,8,13,0) 60%);" />
      <div class="relative z-10 px-7 pt-5 pb-6">
        <button
          @click="router.back()"
          class="flex items-center gap-1.5 text-white/50 hover:text-white/90 text-sm mb-6 transition-colors"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <path d="M15 18l-6-6 6-6"/>
          </svg>
          Playlists
        </button>

        <div class="flex items-end gap-5">
          <div
            class="w-28 h-28 rounded-2xl flex items-center justify-center flex-shrink-0"
            style="background: linear-gradient(135deg, rgba(124,58,237,0.2), rgba(168,85,247,0.1)); border: 1px solid rgba(124,58,237,0.2); box-shadow: 0 8px 32px rgba(0,0,0,0.4);"
          >
            <svg class="w-10 h-10 text-violet-400/60" fill="none" stroke="currentColor" stroke-width="1.25" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
              <path d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3"/>
            </svg>
          </div>
          <div class="pb-1 min-w-0">
            <p class="text-white/40 text-xs font-semibold uppercase tracking-widest mb-1.5">Playlist</p>
            <h1 class="text-white font-bold tracking-tight mb-1 truncate" style="font-size: clamp(1.4rem, 3vw, 2rem);">{{ playlist?.name ?? '' }}</h1>
            <p class="text-white/30 text-sm mb-4">
              {{ entries.length }} tracks · {{ formatTime(totalDuration) }}
            </p>
            <button
              @click="playAll"
              class="flex items-center gap-2 px-5 py-2 rounded-full text-sm font-semibold transition-all duration-200 hover:scale-[1.02] active:scale-[0.98]"
              style="background: linear-gradient(135deg, #7c3aed, #a855f7); box-shadow: 0 4px 20px rgba(124,58,237,0.4);"
            >
              <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
              Play all
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="flex-1 px-5 pb-8">
      <div v-if="loading" class="flex justify-center py-12">
        <svg class="w-4 h-4 animate-spin text-white/30" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
        </svg>
      </div>
      <div v-else-if="entries.length === 0" class="text-center py-12 text-white/30 text-sm">No tracks in this playlist.</div>
      <div v-else class="space-y-1 pt-2">
        <div
          v-for="(entry, i) in entries"
          :key="entry.track.id"
          draggable="true"
          @dragstart="onDragStart(i)"
          @dragover="onDragOver($event, i)"
          @drop="onDrop(i)"
          @dblclick="playFromIndex(i)"
          class="flex items-center gap-2.5 px-3 py-3 rounded-xl transition-all duration-150 group cursor-grab active:cursor-grabbing"
          :class="[
            dragOverIndex === i ? 'border border-violet-500/30' : 'border border-transparent',
            player.currentTrack?.id === entry.track.id
              ? 'bg-violet-500/10 border-violet-500/20'
              : 'hover:bg-white/[0.04]'
          ]"
          :style="dragOverIndex === i ? 'background: rgba(124,58,237,0.08);' : ''"
        >
          <svg class="w-2.5 h-4 text-white/15 group-hover:text-white/30 flex-shrink-0 transition-colors" fill="currentColor" viewBox="0 0 8 20">
            <circle cx="2" cy="4" r="1.5"/><circle cx="6" cy="4" r="1.5"/>
            <circle cx="2" cy="10" r="1.5"/><circle cx="6" cy="10" r="1.5"/>
            <circle cx="2" cy="16" r="1.5"/><circle cx="6" cy="16" r="1.5"/>
          </svg>

          <span class="text-white/20 text-xs tabular-nums w-5 text-right flex-shrink-0">{{ i + 1 }}</span>

          <div class="flex-1 min-w-0">
            <p class="truncate text-sm font-medium leading-tight" :class="player.currentTrack?.id === entry.track.id ? 'text-violet-300' : 'text-white/85'">
              {{ entry.track.title }}
            </p>
            <p class="truncate text-xs mt-0.5 text-white/30">{{ entry.track.artist.name }}</p>
          </div>

          <p class="text-white/20 text-xs tabular-nums flex-shrink-0">{{ formatTime(entry.track.durationSeconds) }}</p>

          <button
            @click.stop="removeTrack(entry.track.id)"
            class="p-1 rounded-lg text-white/0 group-hover:text-white/20 hover:!text-red-400 hover:bg-red-400/10 transition-all duration-150 flex-shrink-0"
          >
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
