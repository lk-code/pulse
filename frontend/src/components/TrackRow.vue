<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { usePlayerStore } from '../stores/playerStore'
import { formatTime } from '../composables/useFormatTime'
import { toSlug } from '../composables/useSlug'
import type { Track } from '../types'

const props = defineProps<{
  track: Track
  index?: number
  queue?: Track[]
  queueStart?: number
  showCover?: boolean
}>()

const router = useRouter()
const player = usePlayerStore()
const isActive = computed(() => player.currentTrack?.id === props.track.id)

function play() {
  const q = props.queue ?? [props.track]
  const idx = props.queue ? (props.queueStart ?? 0) + (props.index ?? 0) : 0
  player.setQueue(q, idx)
  player.isPlaying = true
}

function goToAlbum() {
  const album = props.track.album
  const artist = album.primaryArtist
  if (!artist) return
  const trackParam = `?track=${props.track.id}`
  router.push(
    `/artist/${toSlug(artist.normalizedName, artist.id)}/album/${toSlug(album.normalizedName, album.id)}${trackParam}`
  )
}

const coverUrl = computed(() => {
  const albumId = props.track.album.id
  return `/api/albums/${albumId}/cover`
})
</script>

<template>
  <div
    @click="goToAlbum"
    @dblclick.stop="play"
    class="flex items-center gap-3 px-3 py-3 rounded-xl cursor-pointer group transition-all duration-150"
    :class="isActive
      ? 'bg-violet-500/10 border border-violet-500/20'
      : 'hover:bg-white/[0.04] border border-transparent'"
  >
    <div class="w-7 flex items-center justify-center flex-shrink-0">
      <span v-if="!isActive" class="text-white/25 text-xs tabular-nums group-hover:hidden">
        {{ index != null ? index + 1 : '' }}
      </span>
      <button v-if="!isActive" @click.stop="play" class="hidden group-hover:flex items-center justify-center">
        <svg class="w-3.5 h-3.5 text-white/70" fill="currentColor" viewBox="0 0 24 24">
          <path d="M8 5v14l11-7z"/>
        </svg>
      </button>
      <div v-if="isActive" class="flex items-end gap-0.5 h-3.5">
        <div class="w-0.5 bg-violet-400 rounded-full animate-[soundbar_0.8s_ease-in-out_infinite]" style="height: 60%;"/>
        <div class="w-0.5 bg-violet-400 rounded-full animate-[soundbar_0.8s_ease-in-out_0.2s_infinite]" style="height: 100%;"/>
        <div class="w-0.5 bg-violet-400 rounded-full animate-[soundbar_0.8s_ease-in-out_0.4s_infinite]" style="height: 40%;"/>
      </div>
    </div>

    <div v-if="showCover" class="w-9 h-9 rounded-lg overflow-hidden flex-shrink-0 bg-white/5">
      <img :src="coverUrl" class="w-full h-full object-cover" alt="" />
    </div>

    <div class="flex-1 min-w-0">
      <p class="truncate text-sm font-medium leading-tight" :class="isActive ? 'text-violet-300' : 'text-white/90'">
        {{ track.title }}
      </p>
      <p class="truncate text-xs mt-0.5" :class="isActive ? 'text-violet-400/70' : 'text-white/35'">
        {{ track.artist.name || 'Unknown Artist' }}
        <span class="text-white/20"> · {{ track.album.name }}</span>
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
</template>

<style>
@keyframes soundbar {
  0%, 100% { transform: scaleY(0.4); }
  50% { transform: scaleY(1); }
}
</style>
