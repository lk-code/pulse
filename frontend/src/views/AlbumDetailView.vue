<script setup lang="ts">
import { ref, onMounted, computed, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { api } from '../api'
import { usePlayerStore } from '../stores/playerStore'
import { formatTime } from '../composables/useFormatTime'
import { parseSlug } from '../composables/useSlug'
import type { AlbumDetail, Track } from '../types'

const route = useRoute()
const router = useRouter()
const player = usePlayerStore()

const albumId = computed(() => parseSlug(route.params.albumSlug as string).id)
const highlightTrackId = computed(() => route.query.track ? Number(route.query.track) : null)

const album = ref<AlbumDetail | null>(null)
const loading = ref(true)

const primaryArtist = computed(() => album.value?.artists.find(a => a.isPrimary) ?? album.value?.artists[0])
const totalDuration = computed(() => album.value?.tracks.reduce((s, t) => s + t.durationSeconds, 0) ?? 0)

// Convert AlbumDetail tracks to the Track shape the player expects
function toPlayerTrack(t: AlbumDetail['tracks'][number]): Track {
  return {
    id: t.id,
    libraryId: 0,
    title: t.title,
    normalizedName: t.normalizedName,
    trackNumber: t.trackNumber,
    discNumber: t.discNumber,
    durationSeconds: t.durationSeconds,
    fileType: t.fileType,
    hasMatchingPair: t.hasMatchingPair,
    pairedTrackId: t.pairedTrackId,
    isAvailable: true,
    artist: t.artist,
    album: {
      id: album.value!.id,
      name: album.value!.name,
      normalizedName: album.value!.normalizedName,
      year: album.value!.year,
      primaryArtist: primaryArtist.value ?? null
    }
  }
}

onMounted(async () => {
  try {
    album.value = await api.albums.get(albumId.value)
    if (highlightTrackId.value) {
      await nextTick()
      document.getElementById(`track-${highlightTrackId.value}`)?.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }
  } finally {
    loading.value = false
  }
})

function playAll(startIndex = 0) {
  if (!album.value?.tracks.length) return
  player.setQueue(album.value.tracks.map(toPlayerTrack), startIndex)
  player.isPlaying = true
}

function playTrack(index: number) {
  playAll(index)
}
</script>

<template>
  <div class="flex flex-col min-h-full">
    <div class="relative overflow-hidden flex-shrink-0" style="min-height: 260px;">
      <div
        v-if="album"
        class="absolute inset-0 bg-cover bg-center scale-110"
        :style="{ backgroundImage: `url(${api.albums.coverUrl(album.id)})` }"
      />
      <div class="absolute inset-0" style="background: linear-gradient(to bottom, rgba(8,8,13,0.3) 0%, rgba(8,8,13,0.7) 60%, rgba(8,8,13,1) 100%); backdrop-filter: blur(40px) saturate(0.6);"/>

      <div class="relative z-10 px-7 pt-5 pb-7">
        <button
          @click="router.back()"
          class="flex items-center gap-1.5 text-white/50 hover:text-white/90 text-sm mb-6 transition-colors"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <path d="M15 18l-6-6 6-6"/>
          </svg>
          Back
        </button>

        <div v-if="loading" class="flex justify-center mt-8">
          <svg class="w-4 h-4 animate-spin text-white/30" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
          </svg>
        </div>

        <div v-else-if="album" class="flex gap-6 items-end">
          <div class="flex-shrink-0 rounded-2xl overflow-hidden" style="width: 140px; height: 140px; box-shadow: 0 16px 48px rgba(0,0,0,0.6);">
            <img :src="api.albums.coverUrl(album.id)" class="w-full h-full object-cover" alt="" />
          </div>

          <div class="min-w-0 pb-1">
            <p class="text-white/40 text-xs font-semibold uppercase tracking-widest mb-2">Album</p>
            <h1 class="text-white font-bold leading-tight mb-1 tracking-tight" style="font-size: clamp(1.4rem, 3vw, 2rem);">{{ album.name }}</h1>
            <p class="text-white/55 font-medium mb-4">{{ primaryArtist?.name }}</p>

            <div class="flex items-center gap-3">
              <button
                @click="playAll()"
                class="flex items-center gap-2 px-5 py-2 rounded-full text-sm font-semibold transition-all duration-200 hover:scale-[1.02] active:scale-[0.98]"
                style="background: linear-gradient(135deg, #7c3aed, #a855f7); box-shadow: 0 4px 20px rgba(124,58,237,0.4);"
              >
                <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
                Play
              </button>
              <span class="text-white/30 text-xs">
                {{ album.tracks.length }} tracks · {{ formatTime(totalDuration) }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="flex-1 px-5 pb-8">
      <div v-if="album" class="space-y-1 pt-2">
        <div
          v-for="(track, i) in album.tracks"
          :key="track.id"
          :id="`track-${track.id}`"
          @dblclick="playTrack(i)"
          class="flex items-center gap-3 px-3 py-3 rounded-xl cursor-pointer group transition-all duration-150"
          :class="[
            player.currentTrack?.id === track.id
              ? 'bg-violet-500/10 border border-violet-500/20'
              : 'hover:bg-white/[0.04] border border-transparent',
            highlightTrackId === track.id && player.currentTrack?.id !== track.id
              ? 'ring-1 ring-violet-500/30'
              : ''
          ]"
        >
          <div class="w-7 flex items-center justify-center flex-shrink-0">
            <span v-if="player.currentTrack?.id !== track.id" class="text-white/25 text-xs tabular-nums group-hover:hidden">
              {{ track.trackNumber || i + 1 }}
            </span>
            <button v-if="player.currentTrack?.id !== track.id" @click.stop="playTrack(i)" class="hidden group-hover:flex items-center justify-center">
              <svg class="w-3.5 h-3.5 text-white/70" fill="currentColor" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
            </button>
            <div v-if="player.currentTrack?.id === track.id" class="flex items-end gap-0.5 h-3.5">
              <div class="w-0.5 bg-violet-400 rounded-full animate-[soundbar_0.8s_ease-in-out_infinite]" style="height: 60%;"/>
              <div class="w-0.5 bg-violet-400 rounded-full animate-[soundbar_0.8s_ease-in-out_0.2s_infinite]" style="height: 100%;"/>
              <div class="w-0.5 bg-violet-400 rounded-full animate-[soundbar_0.8s_ease-in-out_0.4s_infinite]" style="height: 40%;"/>
            </div>
          </div>

          <div class="flex-1 min-w-0">
            <p class="truncate text-sm font-medium leading-tight" :class="player.currentTrack?.id === track.id ? 'text-violet-300' : 'text-white/90'">
              {{ track.title }}
            </p>
            <p v-if="track.artist.id !== primaryArtist?.id" class="truncate text-xs mt-0.5 text-white/35">
              {{ track.artist.name }}
            </p>
          </div>

          <div class="flex items-center gap-2 flex-shrink-0">
            <span
              v-if="track.fileType === 'Video'"
              class="text-[10px] px-1.5 py-0.5 rounded-md font-medium"
              style="background: rgba(124,58,237,0.2); color: rgba(196,181,253,0.8); border: 1px solid rgba(124,58,237,0.25);"
            >VIDEO</span>
            <p class="text-white/25 text-xs tabular-nums w-9 text-right">{{ formatTime(track.durationSeconds) }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
