<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '../api'

interface ArtistEntry {
  name: string
  trackCount: number
}

const artists = ref<ArtistEntry[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await api.tracks.list({ pageSize: 5000 })
    const map = new Map<string, number>()

    for (const track of res.tracks) {
      const name = track.artist || 'Unknown Artist'
      map.set(name, (map.get(name) ?? 0) + 1)
    }

    artists.value = [...map.entries()]
      .map(([name, trackCount]) => ({ name, trackCount }))
      .sort((a, b) => a.name.localeCompare(b.name))
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold text-white mb-6">Artists</h1>

    <div v-if="loading" class="text-slate-400">Loading...</div>
    <div v-else class="space-y-1">
      <button
        v-for="artist in artists"
        :key="artist.name"
        @click="$router.push(`/artists/${encodeURIComponent(artist.name)}`)"
        class="w-full flex items-center gap-4 px-4 py-3 rounded-lg hover:bg-white/5 transition-colors text-left"
      >
        <div class="w-10 h-10 rounded-full bg-[#1e1e2e] flex items-center justify-center flex-shrink-0">
          <svg class="w-5 h-5 text-slate-400" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
            <path d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
          </svg>
        </div>
        <div>
          <p class="text-white font-medium">{{ artist.name }}</p>
          <p class="text-slate-400 text-sm">{{ artist.trackCount }} tracks</p>
        </div>
      </button>
    </div>
  </div>
</template>
