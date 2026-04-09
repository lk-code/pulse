<script setup lang="ts">
import { computed, watch, ref } from 'vue'
import { usePlayerStore } from '../stores/playerStore'
import { api } from '../api'
import { formatTime } from '../composables/useFormatTime'

const player = usePlayerStore()

const audioRef = ref<HTMLAudioElement | null>(null)
const seeking = ref(false)

const streamUrl = computed(() =>
  player.currentTrack ? api.tracks.streamUrl(player.currentTrack.id) : ''
)

const coverUrl = computed(() =>
  player.currentTrack ? api.tracks.coverUrl(player.currentTrack.id) : ''
)

const progressPercent = computed(() =>
  player.duration > 0 ? (player.currentTime / player.duration) * 100 : 0
)

watch(() => player.currentTrack, () => {
  if (!audioRef.value) return
  audioRef.value.load()
  if (player.isPlaying) audioRef.value.play()
})

watch(() => player.isPlaying, (playing) => {
  if (!audioRef.value) return
  if (playing) audioRef.value.play()
  else audioRef.value.pause()
})

watch(() => player.currentTime, (time) => {
  if (!audioRef.value || seeking.value) return
  if (Math.abs(audioRef.value.currentTime - time) > 1) {
    audioRef.value.currentTime = time
  }
})

watch(() => player.isMuted, (muted) => {
  if (audioRef.value) audioRef.value.muted = muted
})

watch(() => player.volume, (vol) => {
  if (audioRef.value) audioRef.value.volume = vol
})

function onTimeUpdate() {
  if (!audioRef.value || seeking.value) return
  player.currentTime = audioRef.value.currentTime
}

function onLoadedMetadata() {
  if (!audioRef.value) return
  player.duration = audioRef.value.duration
  if (player.currentTime > 0) audioRef.value.currentTime = player.currentTime
}

function onEnded() {
  player.playNext()
}

function seekTo(e: MouseEvent) {
  const bar = e.currentTarget as HTMLElement
  const rect = bar.getBoundingClientRect()
  const ratio = (e.clientX - rect.left) / rect.width
  const newTime = ratio * player.duration
  player.currentTime = newTime
  if (audioRef.value) audioRef.value.currentTime = newTime
}

function onVolumeInput(e: Event) {
  const input = e.target as HTMLInputElement
  player.volume = Number(input.value)
  if (audioRef.value) audioRef.value.volume = player.volume
}
</script>

<template>
  <div class="h-20 bg-[#09090e] border-t border-[#1e1e2e] flex items-center px-4 gap-4 z-10 flex-shrink-0">
    <audio
      ref="audioRef"
      v-if="player.currentTrack && player.mediaMode === 'audio'"
      :src="streamUrl"
      preload="auto"
      @timeupdate="onTimeUpdate"
      @loadedmetadata="onLoadedMetadata"
      @ended="onEnded"
    />

    <div class="flex items-center gap-3 w-56 min-w-0">
      <img
        v-if="player.currentTrack"
        :src="coverUrl"
        :key="player.currentTrack.id"
        class="w-12 h-12 rounded object-cover bg-[#1e1e2e] flex-shrink-0"
        alt=""
      />
      <div v-else class="w-12 h-12 rounded bg-[#1e1e2e] flex-shrink-0 flex items-center justify-center">
        <svg class="w-5 h-5 text-slate-600" fill="currentColor" viewBox="0 0 24 24">
          <path d="M12 3v10.55c-.59-.34-1.27-.55-2-.55-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4V7h4V3h-6z"/>
        </svg>
      </div>
      <div v-if="player.currentTrack" class="min-w-0">
        <p class="text-white text-sm font-medium truncate">{{ player.currentTrack.title }}</p>
        <p class="text-slate-400 text-xs truncate">{{ player.currentTrack.artist }}</p>
      </div>
    </div>

    <div class="flex-1 flex flex-col items-center gap-1.5 max-w-xl mx-auto">
      <div class="flex items-center gap-4">
        <button
          @click="player.toggleShuffle()"
          class="p-1 transition-colors"
          :class="player.shuffle ? 'text-violet-400' : 'text-slate-400 hover:text-white'"
          title="Shuffle"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="16 3 21 3 21 8"/><line x1="4" y1="20" x2="21" y2="3"/><polyline points="21 16 21 21 16 21"/><line x1="15" y1="15" x2="21" y2="21"/>
          </svg>
        </button>

        <button @click="player.playPrevious()" class="text-slate-300 hover:text-white transition-colors p-1">
          <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
            <path d="M6 6h2v12H6zm3.5 6l8.5 6V6z"/>
          </svg>
        </button>

        <button
          @click="player.isPlaying = !player.isPlaying"
          class="w-9 h-9 rounded-full bg-white flex items-center justify-center hover:bg-violet-100 transition-colors"
        >
          <svg v-if="!player.isPlaying" class="w-5 h-5 text-black ml-0.5" fill="currentColor" viewBox="0 0 24 24">
            <path d="M8 5v14l11-7z"/>
          </svg>
          <svg v-else class="w-5 h-5 text-black" fill="currentColor" viewBox="0 0 24 24">
            <path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z"/>
          </svg>
        </button>

        <button @click="player.playNext()" class="text-slate-300 hover:text-white transition-colors p-1">
          <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
            <path d="M6 18l8.5-6L6 6v12zm2.5-6l6-4.269V16.27L8.5 12z"/><path d="M16 6h2v12h-2z"/>
          </svg>
        </button>

        <button
          @click="player.cycleRepeat()"
          class="p-1 transition-colors relative"
          :class="player.repeat !== 'off' ? 'text-violet-400' : 'text-slate-400 hover:text-white'"
          title="Repeat"
        >
          <svg v-if="player.repeat !== 'one'" class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="17 1 21 5 17 9"/><path d="M3 11V9a4 4 0 014-4h14"/><polyline points="7 23 3 19 7 15"/><path d="M21 13v2a4 4 0 01-4 4H3"/>
          </svg>
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="17 1 21 5 17 9"/><path d="M3 11V9a4 4 0 014-4h14"/><polyline points="7 23 3 19 7 15"/><path d="M21 13v2a4 4 0 01-4 4H3"/>
          </svg>
          <span v-if="player.repeat === 'one'" class="absolute -top-0.5 -right-0.5 text-[8px] font-bold bg-violet-500 text-white rounded-full w-3 h-3 flex items-center justify-center">1</span>
        </button>
      </div>

      <div class="w-full flex items-center gap-2">
        <span class="text-xs text-slate-400 tabular-nums w-9 text-right">{{ formatTime(player.currentTime) }}</span>
        <div
          class="flex-1 h-1 bg-[#2a2a3e] rounded-full cursor-pointer group relative"
          @click="seekTo"
        >
          <div
            class="h-full bg-violet-500 group-hover:bg-violet-400 rounded-full transition-colors relative"
            :style="{ width: progressPercent + '%' }"
          >
            <div class="absolute right-0 top-1/2 -translate-y-1/2 w-3 h-3 bg-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity" />
          </div>
        </div>
        <span class="text-xs text-slate-400 tabular-nums w-9">{{ formatTime(player.duration) }}</span>
      </div>
    </div>

    <div class="flex items-center gap-2 w-36 justify-end">
      <button
        v-if="player.canSwitchMode"
        @click="player.toggleMediaMode()"
        class="px-2 py-1 rounded text-xs font-medium transition-colors"
        :class="player.mediaMode === 'video' ? 'bg-violet-600 text-white' : 'bg-[#1e1e2e] text-slate-300 hover:text-white'"
      >
        {{ player.mediaMode === 'video' ? 'Video' : 'Audio' }}
      </button>

      <button @click="player.toggleMute()" class="text-slate-400 hover:text-white transition-colors">
        <svg v-if="player.isMuted || player.volume === 0" class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
          <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/><line x1="23" y1="9" x2="17" y2="15"/><line x1="17" y1="9" x2="23" y2="15"/>
        </svg>
        <svg v-else-if="player.volume < 0.5" class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
          <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/><path d="M15.54 8.46a5 5 0 010 7.07"/>
        </svg>
        <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
          <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/><path d="M19.07 4.93a10 10 0 010 14.14M15.54 8.46a5 5 0 010 7.07"/>
        </svg>
      </button>

      <input
        type="range"
        min="0"
        max="1"
        step="0.01"
        :value="player.isMuted ? 0 : player.volume"
        @input="onVolumeInput"
        class="w-20 h-1 accent-violet-500 cursor-pointer"
      />
    </div>
  </div>
</template>
