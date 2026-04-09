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
  if (Math.abs(videoRef.value.currentTime - time) > 1) videoRef.value.currentTime = time
})
watch(() => player.isMuted, (muted) => { if (videoRef.value) videoRef.value.muted = muted })
watch(() => player.volume, (vol) => { if (videoRef.value) videoRef.value.volume = vol })

function onTimeUpdate() {
  if (videoRef.value) player.currentTime = videoRef.value.currentTime
}
function onLoadedMetadata() {
  if (!videoRef.value) return
  player.duration = videoRef.value.duration
  if (player.currentTime > 0) videoRef.value.currentTime = player.currentTime
  if (player.isPlaying) videoRef.value.play()
}
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center"
    style="background: rgba(0,0,0,0.92); backdrop-filter: blur(8px); padding-bottom: 80px;"
  >
    <button
      @click="player.toggleMediaMode()"
      class="absolute top-5 right-5 w-9 h-9 rounded-full flex items-center justify-center transition-all duration-200 hover:scale-105"
      style="background: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.12);"
    >
      <svg class="w-4 h-4 text-white/60" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
        <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
      </svg>
    </button>

    <video
      ref="videoRef"
      :src="streamUrl"
      class="max-w-full max-h-full rounded-2xl"
      style="box-shadow: 0 32px 80px rgba(0,0,0,0.8);"
      preload="auto"
      @timeupdate="onTimeUpdate"
      @loadedmetadata="onLoadedMetadata"
      @ended="player.playNext()"
    />
  </div>
</template>
