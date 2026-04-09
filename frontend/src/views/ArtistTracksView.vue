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

const artistName = computed(() => decodeURIComponent(route.params.artist as string))
const tracks = ref<Track[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await api.tracks.list({ artist: artistName.value, pageSize: 1000 })
    tracks.value = res.tracks
  } finally {
    loading.value = false
  }
})

const totalDuration = computed(() => tracks.value.reduce((s, t) => s + t.durationSeconds, 0))

const albumCount = computed(() => new Set(tracks.value.map(t => t.album)).size)

function playAll() {
  if (!tracks.value.length) return
  player.setQueue(tracks.value, 0)
  player.isPlaying = true
}
</script>

<template>
  <div class="flex flex-col min-h-full">
    <div class="relative overflow-hidden flex-shrink-0" style="min-height: 220px;">
      <div class="absolute inset-0" style="background: linear-gradient(135deg, rgba(124,58,237,0.15) 0%, rgba(8,8,13,0) 60%);" />
      <div class="relative z-10 px-7 pt-5 pb-7">
        <button
          @click="router.back()"
          class="flex items-center gap-1.5 text-white/50 hover:text-white/90 text-sm mb-6 transition-colors"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <path d="M15 18l-6-6 6-6"/>
          </svg>
          Artists
        </button>

        <div class="flex items-end gap-5">
          <div
            class="w-28 h-28 rounded-full flex items-center justify-center flex-shrink-0 text-4xl font-bold text-white/30"
            style="background: rgba(124,58,237,0.15); border: 2px solid rgba(124,58,237,0.2); box-shadow: 0 8px 32px rgba(0,0,0,0.4);"
          >
            {{ artistName[0]?.toUpperCase() }}
          </div>
          <div class="pb-1 min-w-0">
            <p class="text-white/40 text-xs font-semibold uppercase tracking-widest mb-1.5">Artist</p>
            <h1 class="text-white font-bold tracking-tight mb-1 truncate" style="font-size: clamp(1.4rem, 3vw, 2rem);">{{ artistName }}</h1>
            <p class="text-white/30 text-sm mb-4">
              {{ albumCount }} {{ albumCount === 1 ? 'album' : 'albums' }} · {{ tracks.length }} tracks · {{ formatTime(totalDuration) }}
            </p>
            <button
              @click="playAll"
              class="flex items-center gap-2 px-5 py-2 rounded-full text-sm font-semibold transition-all duration-200 hover:scale-[1.02] active:scale-[0.98]"
              style="background: linear-gradient(135deg, #7c3aed, #a855f7); box-shadow: 0 4px 20px rgba(124,58,237,0.4);"
            >
              <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
              Play all
            </button>
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
          :show-cover="true"
        />
      </div>
    </div>
  </div>
</template>
