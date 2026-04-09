<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import Sidebar from './components/Sidebar.vue'
import PlayerBar from './components/PlayerBar.vue'
import VideoOverlay from './components/VideoOverlay.vue'
import ResumeToast from './components/ResumeToast.vue'
import { usePlayerStore } from './stores/playerStore'
import { RouterView } from 'vue-router'

const player = usePlayerStore()

function onKeydown(e: KeyboardEvent) {
  if ((e.target as HTMLElement).tagName === 'INPUT') return

  if (e.code === 'Space') {
    e.preventDefault()
    player.isPlaying = !player.isPlaying
  } else if (e.code === 'ArrowLeft') {
    e.preventDefault()
    player.currentTime = Math.max(0, player.currentTime - 5)
  } else if (e.code === 'ArrowRight') {
    e.preventDefault()
    player.currentTime = Math.min(player.duration, player.currentTime + 5)
  } else if (e.key === 'm' || e.key === 'M') {
    player.toggleMute()
  }
}

onMounted(() => window.addEventListener('keydown', onKeydown))
onUnmounted(() => window.removeEventListener('keydown', onKeydown))
</script>

<template>
  <div class="flex flex-col h-screen bg-[#0f0f13]">
    <div class="flex flex-1 min-h-0">
      <Sidebar />
      <main class="flex-1 overflow-y-auto bg-[#0f0f13]">
        <RouterView />
      </main>
    </div>
    <PlayerBar />
    <VideoOverlay v-if="player.mediaMode === 'video' && player.currentTrack" />
    <ResumeToast v-if="player.resumeToastVisible" />
  </div>
</template>
