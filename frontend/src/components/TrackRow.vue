<script setup lang="ts">
import { computed } from 'vue'
import { usePlayerStore } from '../stores/playerStore'
import { formatTime } from '../composables/useFormatTime'
import type { Track } from '../types'

const props = defineProps<{
  track: Track
  index?: number
  queue?: Track[]
  queueStart?: number
}>()

const player = usePlayerStore()

const isActive = computed(() => player.currentTrack?.id === props.track.id)

function play() {
  const q = props.queue ?? [props.track]
  const idx = props.queue ? (props.queueStart ?? 0) + (props.index ?? 0) : 0
  player.setQueue(q, idx)
  player.isPlaying = true
}
</script>

<template>
  <div
    @dblclick="play"
    class="flex items-center gap-3 px-4 py-2 rounded-lg cursor-pointer group transition-colors"
    :class="isActive ? 'bg-violet-600/20' : 'hover:bg-white/5'"
  >
    <div class="w-8 text-center flex-shrink-0">
      <span v-if="!isActive" class="text-slate-500 text-sm group-hover:hidden">
        {{ index != null ? index + 1 : '' }}
      </span>
      <button v-if="!isActive" @click.stop="play" class="hidden group-hover:flex items-center justify-center w-full">
        <svg class="w-4 h-4 text-white" fill="currentColor" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
      </button>
      <svg v-if="isActive" class="w-4 h-4 text-violet-400 mx-auto" viewBox="0 0 24 24" fill="currentColor">
        <path d="M12 3v10.55c-.59-.34-1.27-.55-2-.55-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4V7h4V3h-6z"/>
      </svg>
    </div>

    <div class="flex-1 min-w-0">
      <p class="truncate text-sm" :class="isActive ? 'text-violet-300' : 'text-white'">
        {{ track.title }}
      </p>
      <p class="truncate text-xs text-slate-400">{{ track.artist }}</p>
    </div>

    <p class="text-slate-400 text-sm hidden md:block truncate max-w-32">{{ track.album }}</p>

    <span
      v-if="track.fileType === 'Video'"
      class="text-xs px-1.5 py-0.5 bg-violet-900/50 text-violet-300 rounded"
    >video</span>

    <p class="text-slate-500 text-sm tabular-nums flex-shrink-0">
      {{ formatTime(track.durationSeconds) }}
    </p>
  </div>
</template>
