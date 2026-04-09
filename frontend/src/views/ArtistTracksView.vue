<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { api } from '../api'
import { usePlayerStore } from '../stores/playerStore'
import TrackRow from '../components/TrackRow.vue'
import type { Track } from '../types'

const route = useRoute()
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

function playAll() {
  if (tracks.value.length === 0) return
  player.setQueue(tracks.value, 0)
  player.isPlaying = true
}
</script>

<template>
  <div class="p-6">
    <div class="flex items-end gap-6 mb-8">
      <div class="w-32 h-32 rounded-full bg-[#1e1e2e] flex items-center justify-center flex-shrink-0">
        <svg class="w-12 h-12 text-slate-500" fill="none" stroke="currentColor" stroke-width="1.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
          <path d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
        </svg>
      </div>
      <div>
        <p class="text-slate-400 text-sm mb-1">Artist</p>
        <h1 class="text-3xl font-bold text-white mb-3">{{ artistName }}</h1>
        <button
          @click="playAll"
          class="px-5 py-2 bg-violet-600 hover:bg-violet-500 text-white rounded-full text-sm font-medium transition-colors"
        >
          Play all
        </button>
      </div>
    </div>

    <div v-if="loading" class="text-slate-400">Loading...</div>
    <div v-else>
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
