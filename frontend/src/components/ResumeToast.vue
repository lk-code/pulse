<script setup lang="ts">
import { usePlayerStore } from '../stores/playerStore'
import { formatTime } from '../composables/useFormatTime'

const player = usePlayerStore()
</script>

<template>
  <div class="fixed bottom-24 left-1/2 -translate-x-1/2 bg-[#1e1b4b] border border-violet-700/50 rounded-xl px-5 py-3 flex items-center gap-4 shadow-2xl z-40">
    <p class="text-white text-sm">
      Resume from <span class="text-violet-300 font-medium">{{ formatTime(player.resumePosition) }}</span>?
    </p>
    <button
      @click="player.acceptResume()"
      class="px-3 py-1 bg-violet-600 hover:bg-violet-500 text-white text-sm rounded-lg transition-colors"
    >
      Resume
    </button>
    <button
      @click="player.dismissResumeToast()"
      class="px-3 py-1 bg-white/10 hover:bg-white/20 text-white text-sm rounded-lg transition-colors"
    >
      Start over
    </button>
  </div>
</template>
