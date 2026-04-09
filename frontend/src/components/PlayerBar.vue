<script setup lang="ts">
import { computed, watch, ref } from 'vue'
import { usePlayerStore } from '../stores/playerStore'
import { api } from '../api'
import { formatTime } from '../composables/useFormatTime'

const player = usePlayerStore()
const audioRef = ref<HTMLAudioElement | null>(null)

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
  if (!audioRef.value) return
  if (Math.abs(audioRef.value.currentTime - time) > 1) audioRef.value.currentTime = time
})
watch(() => player.isMuted, (muted) => { if (audioRef.value) audioRef.value.muted = muted })
watch(() => player.volume, (vol) => { if (audioRef.value) audioRef.value.volume = vol })

function onTimeUpdate() {
  if (!audioRef.value) return
  player.currentTime = audioRef.value.currentTime
}
function onLoadedMetadata() {
  if (!audioRef.value) return
  player.duration = audioRef.value.duration
  if (player.currentTime > 0) audioRef.value.currentTime = player.currentTime
}
function seekTo(e: MouseEvent) {
  const bar = e.currentTarget as HTMLElement
  const rect = bar.getBoundingClientRect()
  const newTime = ((e.clientX - rect.left) / rect.width) * player.duration
  player.currentTime = newTime
  if (audioRef.value) audioRef.value.currentTime = newTime
}
function onVolumeInput(e: Event) {
  player.volume = Number((e.target as HTMLInputElement).value)
  if (audioRef.value) audioRef.value.volume = player.volume
}
</script>

<template>
  <div
    class="flex-shrink-0 border-t border-white/[0.06]"
    style="background: rgba(8,8,13,0.85); backdrop-filter: blur(24px); height: 88px;"
  >
    <audio
      ref="audioRef"
      v-if="player.currentTrack && player.mediaMode === 'audio'"
      :src="streamUrl"
      preload="auto"
      @timeupdate="onTimeUpdate"
      @loadedmetadata="onLoadedMetadata"
      @ended="player.playNext()"
    />

    <div class="h-full flex items-center px-6 gap-6">
      <div class="flex items-center gap-3 w-56 min-w-0">
        <div class="relative flex-shrink-0">
          <img
            v-if="player.currentTrack"
            :src="coverUrl"
            :key="player.currentTrack.id"
            class="w-11 h-11 rounded-lg object-cover"
            style="box-shadow: 0 4px 16px rgba(0,0,0,0.5);"
            alt=""
          />
          <div
            v-else
            class="w-11 h-11 rounded-lg flex items-center justify-center"
            style="background: rgba(255,255,255,0.05);"
          >
            <svg class="w-4.5 h-4.5 text-white/20" fill="currentColor" viewBox="0 0 24 24">
              <path d="M12 3v10.55c-.59-.34-1.27-.55-2-.55-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4V7h4V3h-6z"/>
            </svg>
          </div>
        </div>

        <div v-if="player.currentTrack" class="min-w-0">
          <p class="text-white/90 text-sm font-medium truncate leading-tight">{{ player.currentTrack.title }}</p>
          <p class="text-white/35 text-xs truncate mt-0.5">{{ player.currentTrack.artist }}</p>
        </div>
        <div v-else class="min-w-0">
          <p class="text-white/20 text-sm">Nothing playing</p>
        </div>
      </div>

      <div class="flex-1 flex flex-col items-center gap-2 max-w-md mx-auto">
        <div class="flex items-center gap-6">
          <button
            @click="player.toggleShuffle()"
            class="transition-all duration-200"
            :class="player.shuffle ? 'text-violet-400' : 'text-white/25 hover:text-white/60'"
          >
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="16 3 21 3 21 8"/><line x1="4" y1="20" x2="21" y2="3"/><polyline points="21 16 21 21 16 21"/><line x1="15" y1="15" x2="21" y2="21"/>
            </svg>
          </button>

          <button @click="player.playPrevious()" class="text-white/50 hover:text-white/90 transition-colors">
            <svg class="w-4.5 h-4.5" fill="currentColor" viewBox="0 0 24 24">
              <path d="M6 6h2v12H6zm3.5 6 8.5 6V6z"/>
            </svg>
          </button>

          <button
            @click="player.isPlaying = !player.isPlaying"
            class="w-8 h-8 rounded-full flex items-center justify-center transition-all duration-200 hover:scale-105 active:scale-95"
            style="background: rgba(255,255,255,0.95); box-shadow: 0 2px 12px rgba(139,92,246,0.3);"
          >
            <svg v-if="!player.isPlaying" class="w-4 h-4 text-black ml-0.5" fill="currentColor" viewBox="0 0 24 24">
              <path d="M8 5v14l11-7z"/>
            </svg>
            <svg v-else class="w-4 h-4 text-black" fill="currentColor" viewBox="0 0 24 24">
              <path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z"/>
            </svg>
          </button>

          <button @click="player.playNext()" class="text-white/50 hover:text-white/90 transition-colors">
            <svg class="w-4.5 h-4.5" fill="currentColor" viewBox="0 0 24 24">
              <path d="M6 18l8.5-6L6 6v12zm2.5-6 6-4.269V16.27L8.5 12zM16 6h2v12h-2z"/>
            </svg>
          </button>

          <button
            @click="player.cycleRepeat()"
            class="relative transition-all duration-200"
            :class="player.repeat !== 'off' ? 'text-violet-400' : 'text-white/25 hover:text-white/60'"
          >
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="17 1 21 5 17 9"/><path d="M3 11V9a4 4 0 014-4h14"/><polyline points="7 23 3 19 7 15"/><path d="M21 13v2a4 4 0 01-4 4H3"/>
            </svg>
            <span
              v-if="player.repeat === 'one'"
              class="absolute -top-1 -right-1 text-[7px] font-bold bg-violet-500 text-white rounded-full w-2.5 h-2.5 flex items-center justify-center"
            >1</span>
          </button>
        </div>

        <div class="w-full flex items-center gap-2.5">
          <span class="text-white/25 text-[10px] tabular-nums w-7 text-right">{{ formatTime(player.currentTime) }}</span>
          <div
            class="flex-1 h-0.5 rounded-full cursor-pointer group relative"
            style="background: rgba(255,255,255,0.1);"
            @click="seekTo"
          >
            <div
              class="h-full rounded-full relative transition-all"
              style="background: linear-gradient(90deg, #7c3aed, #a855f7);"
              :style="{ width: progressPercent + '%' }"
            >
              <div
                class="absolute right-0 top-1/2 -translate-y-1/2 w-2.5 h-2.5 bg-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                style="box-shadow: 0 0 6px rgba(168,85,247,0.6);"
              />
            </div>
          </div>
          <span class="text-white/25 text-[10px] tabular-nums w-7">{{ formatTime(player.duration) }}</span>
        </div>
      </div>

      <div class="flex items-center gap-3 w-44 justify-end">
        <button
          v-if="player.canSwitchMode"
          @click="player.toggleMediaMode()"
          class="px-2.5 py-1 rounded-lg text-[11px] font-semibold transition-all duration-200"
          :class="player.mediaMode === 'video'
            ? 'text-violet-200 border border-violet-500/40'
            : 'text-white/40 border border-white/10 hover:text-white/70'"
          :style="player.mediaMode === 'video' ? 'background: rgba(124,58,237,0.2);' : 'background: rgba(255,255,255,0.04);'"
        >
          {{ player.mediaMode === 'video' ? 'VIDEO' : 'AUDIO' }}
        </button>

        <button @click="player.toggleMute()" class="text-white/30 hover:text-white/70 transition-colors">
          <svg v-if="player.isMuted || player.volume === 0" class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/><line x1="23" y1="9" x2="17" y2="15"/><line x1="17" y1="9" x2="23" y2="15"/>
          </svg>
          <svg v-else-if="player.volume < 0.5" class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/><path d="M15.54 8.46a5 5 0 010 7.07"/>
          </svg>
          <svg v-else class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/><path d="M19.07 4.93a10 10 0 010 14.14M15.54 8.46a5 5 0 010 7.07"/>
          </svg>
        </button>

        <input
          type="range" min="0" max="1" step="0.01"
          :value="player.isMuted ? 0 : player.volume"
          @input="onVolumeInput"
          class="w-20 cursor-pointer"
        />
      </div>
    </div>
  </div>
</template>
