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
  if (!val.trim()) { tracks.value = []; return }
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
  <div class="p-7">
    <h1 class="text-2xl font-bold text-white tracking-tight mb-5">Search</h1>

    <div class="relative mb-6 max-w-xl">
      <svg
        class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-white/30 pointer-events-none"
        fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round"
      >
        <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
      </svg>
      <input
        v-model="query"
        placeholder="Tracks, artists, albums…"
        autofocus
        class="w-full text-white text-sm rounded-2xl pl-11 pr-4 py-3 focus:outline-none placeholder-white/20 transition-all duration-200"
        style="background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.08);"
        :style="query ? 'border-color: rgba(124,58,237,0.4);' : ''"
      />
      <button
        v-if="query"
        @click="query = ''"
        class="absolute right-3 top-1/2 -translate-y-1/2 text-white/25 hover:text-white/60 transition-colors"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
          <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
        </svg>
      </button>
    </div>

    <div v-if="!query" class="flex flex-col items-center justify-center mt-20 gap-3">
      <svg class="w-10 h-10 text-white/10" fill="none" stroke="currentColor" stroke-width="1.25" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
      </svg>
      <p class="text-white/20 text-sm">Type to search your library</p>
    </div>

    <div v-else-if="loading" class="flex items-center gap-3 justify-center mt-16 text-white/30">
      <svg class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
      </svg>
      <span class="text-sm">Searching…</span>
    </div>

    <div v-else-if="tracks.length === 0" class="flex flex-col items-center justify-center mt-20 gap-2">
      <p class="text-white/50 font-medium">No results</p>
      <p class="text-white/25 text-sm">Nothing found for "{{ query }}"</p>
    </div>

    <div v-else>
      <p class="text-white/25 text-xs font-semibold uppercase tracking-wider mb-3 px-3">
        {{ tracks.length }} result{{ tracks.length !== 1 ? 's' : '' }}
      </p>
      <div class="space-y-1">
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
