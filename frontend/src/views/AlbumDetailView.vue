<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { api } from '../api'
import { usePlayerStore } from '../stores/playerStore'
import { formatTime } from '../composables/useFormatTime'
import TrackRow from '../components/TrackRow.vue'
import type { Track } from '../types'

const route = useRoute()
const router = useRouter()
const player = usePlayerStore()

const albumArtist = computed(() => decodeURIComponent(route.params.albumArtist as string))
const albumName = computed(() => decodeURIComponent(route.params.album as string))

const tracks = ref<Track[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await api.tracks.list({ albumArtist: albumArtist.value, album: albumName.value, pageSize: 500 })
    tracks.value = res.tracks
  } finally {
    loading.value = false
  }
})

const totalDuration = computed(() => tracks.value.reduce((s, t) => s + t.durationSeconds, 0))
const coverUrl = computed(() => tracks.value.length ? api.tracks.coverUrl(tracks.value[0].id) : '')

function playAll(startIndex = 0) {
  if (!tracks.value.length) return
  player.setQueue(tracks.value, startIndex)
  player.isPlaying = true
}
</script>

<template>
  <div class="flex flex-col min-h-full">
    <div class="relative overflow-hidden flex-shrink-0" style="min-height: 260px;">
      <div
        v-if="coverUrl"
        class="absolute inset-0 bg-cover bg-center scale-110"
        :style="{ backgroundImage: `url(${coverUrl})` }"
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
          Library
        </button>

        <div class="flex gap-6 items-end">
          <div class="flex-shrink-0 rounded-2xl overflow-hidden" style="width: 140px; height: 140px; box-shadow: 0 16px 48px rgba(0,0,0,0.6);">
            <img v-if="coverUrl" :src="coverUrl" class="w-full h-full object-cover" alt="" />
          </div>

          <div class="min-w-0 pb-1">
            <p class="text-white/40 text-xs font-semibold uppercase tracking-widest mb-2">Album</p>
            <h1 class="text-white font-bold leading-tight mb-1 tracking-tight" style="font-size: clamp(1.4rem, 3vw, 2rem);">{{ albumName }}</h1>
            <p class="text-white/55 font-medium mb-4">{{ albumArtist }}</p>

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
                {{ tracks.length }} tracks · {{ formatTime(totalDuration) }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="flex-1 px-5 pb-8">
      <div v-if="loading" class="flex justify-center py-12">
        <svg class="w-4 h-4 animate-spin text-white/30" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
        </svg>
      </div>
      <div v-else class="space-y-1 pt-2">
        <TrackRow
          v-for="(track, i) in tracks"
          :key="track.id"
          :track="track"
          :index="i"
          :queue="tracks"
          :queue-start="0"
        />
      </div>
    </div>
  </div>
</template>
