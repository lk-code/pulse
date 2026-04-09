<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { api } from '../api'
import { usePlayerStore } from '../stores/playerStore'
import { formatTime } from '../composables/useFormatTime'
import TrackRow from '../components/TrackRow.vue'
import type { Track } from '../types'

const route = useRoute()
const player = usePlayerStore()

const albumArtist = computed(() => decodeURIComponent(route.params.albumArtist as string))
const albumName = computed(() => decodeURIComponent(route.params.album as string))

const tracks = ref<Track[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await api.tracks.list({ artist: albumArtist.value, album: albumName.value, pageSize: 500 })
    tracks.value = res.tracks
  } finally {
    loading.value = false
  }
})

const totalDuration = computed(() =>
  tracks.value.reduce((sum, t) => sum + t.durationSeconds, 0)
)

function playAll() {
  if (tracks.value.length === 0) return
  player.setQueue(tracks.value, 0)
  player.isPlaying = true
}
</script>

<template>
  <div class="p-6">
    <div class="flex gap-6 mb-8">
      <img
        v-if="tracks.length"
        :src="api.tracks.coverUrl(tracks[0].id)"
        class="w-40 h-40 rounded-xl object-cover shadow-2xl bg-[#1e1e2e] flex-shrink-0"
        alt=""
      />
      <div class="flex flex-col justify-end">
        <p class="text-slate-400 text-sm mb-1">Album</p>
        <h1 class="text-3xl font-bold text-white mb-1">{{ albumName }}</h1>
        <p class="text-slate-300 mb-4">{{ albumArtist }}</p>
        <div class="flex items-center gap-3">
          <button
            @click="playAll"
            class="px-5 py-2 bg-violet-600 hover:bg-violet-500 text-white rounded-full text-sm font-medium transition-colors"
          >
            Play all
          </button>
          <span class="text-slate-400 text-sm">
            {{ tracks.length }} tracks · {{ formatTime(totalDuration) }}
          </span>
        </div>
      </div>
    </div>

    <div v-if="loading" class="text-slate-400">Loading...</div>
    <div v-else>
      <div class="text-slate-500 text-xs uppercase tracking-wider px-4 py-2 grid grid-cols-[2rem_1fr_auto_6rem] gap-3">
        <span>#</span>
        <span>Title</span>
        <span></span>
        <span class="text-right">Duration</span>
      </div>
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
</template>
