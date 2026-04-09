<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { usePlayerStore } from '../stores/playerStore'
import { api } from '../api'

const player = usePlayerStore()
const videoRef = ref<HTMLVideoElement | null>(null)

const streamUrl = computed(() =>
  player.currentTrack ? api.tracks.streamUrl(player.currentTrack.id) : ''
)

watch(() => player.isPlaying, (playing) => {
  if (!videoRef.value) return
  if (playing) videoRef.value.play()
  else videoRef.value.pause()
})

watch(() => player.currentTime, (time) => {
  if (!videoRef.value) return
  if (Math.abs(videoRef.value.currentTime - time) > 1) {
    videoRef.value.currentTime = time
  }
})

watch(() => player.isMuted, (muted) => {
  if (videoRef.value) videoRef.value.muted = muted
})

watch(() => player.volume, (vol) => {
  if (videoRef.value) videoRef.value.volume = vol
})

function onTimeUpdate() {
  if (!videoRef.value) return
  player.currentTime = videoRef.value.currentTime
}

function onLoadedMetadata() {
  if (!videoRef.value) return
  player.duration = videoRef.value.duration
  if (player.currentTime > 0) videoRef.value.currentTime = player.currentTime
  if (player.isPlaying) videoRef.value.play()
}
</script>

<template>
  <div class="fixed inset-0 z-50 bg-black/90 flex items-center justify-center pb-20">
    <button
      @click="player.toggleMediaMode()"
      class="absolute top-4 right-4 text-white/60 hover:text-white transition-colors p-2"
    >
      <svg class="w-6 h-6" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
        <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
      </svg>
    </button>

    <video
      ref="videoRef"
      :src="streamUrl"
      class="max-w-full max-h-full rounded-lg shadow-2xl"
      preload="auto"
      @timeupdate="onTimeUpdate"
      @loadedmetadata="onLoadedMetadata"
      @ended="player.playNext()"
    />
  </div>
</template>
