<script setup lang="ts">
import { ref, watch } from 'vue'
import { api } from '../api'
import TrackRow from '../components/TrackRow.vue'
import type { Track } from '../types'

const query = ref('')
const tracks = ref<Track[]>([])
const loading = ref(false)
let debounce: ReturnType<typeof setTimeout> | null = null

watch(query, (val) => {
  if (debounce) clearTimeout(debounce)
  if (!val.trim()) {
    tracks.value = []
    return
  }
  debounce = setTimeout(async () => {
    loading.value = true
    try {
      const res = await api.tracks.list({ search: val.trim(), pageSize: 100 })
      tracks.value = res.tracks
    } finally {
      loading.value = false
    }
  }, 300)
})
</script>

<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold text-white mb-6">Search</h1>

    <div class="relative max-w-lg mb-6">
      <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
      </svg>
      <input
        v-model="query"
        placeholder="Search tracks, artists, albums..."
        autofocus
        class="w-full bg-[#1e1e2e] border border-[#2a2a3e] text-white text-sm rounded-xl pl-10 pr-4 py-3 focus:outline-none focus:border-violet-500 placeholder-slate-500"
      />
    </div>

    <div v-if="loading" class="text-slate-400">Searching...</div>

    <div v-else-if="query && tracks.length === 0" class="text-slate-400">
      No results for "{{ query }}"
    </div>

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
