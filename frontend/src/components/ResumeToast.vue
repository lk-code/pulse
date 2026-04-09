<script setup lang="ts">
import { usePlayerStore } from '../stores/playerStore'
import { formatTime } from '../composables/useFormatTime'

const player = usePlayerStore()
</script>

<template>
  <div
    class="fixed bottom-24 left-1/2 -translate-x-1/2 z-40 flex items-center gap-4 px-5 py-3.5 rounded-2xl"
    style="background: rgba(20,18,35,0.9); backdrop-filter: blur(24px); border: 1px solid rgba(124,58,237,0.25); box-shadow: 0 8px 32px rgba(0,0,0,0.5), 0 0 0 1px rgba(124,58,237,0.1);"
  >
    <div>
      <p class="text-white/70 text-xs mb-0.5">Resume where you left off?</p>
      <p class="text-violet-300 text-sm font-semibold">{{ formatTime(player.resumePosition) }}</p>
    </div>
    <div class="flex gap-2">
      <button
        @click="player.acceptResume()"
        class="px-3.5 py-1.5 rounded-xl text-sm font-semibold transition-all duration-200 hover:scale-[1.03]"
        style="background: linear-gradient(135deg, #7c3aed, #a855f7);"
      >
        Resume
      </button>
      <button
        @click="player.dismissResumeToast()"
        class="px-3.5 py-1.5 rounded-xl text-sm font-medium transition-all duration-200 text-white/50 hover:text-white/80"
        style="background: rgba(255,255,255,0.07); border: 1px solid rgba(255,255,255,0.08);"
      >
        Start over
      </button>
    </div>
  </div>
</template>
