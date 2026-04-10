<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '../api'
import { toSlug } from '../composables/useSlug'
import type { ArtistSummary } from '../types'

const router = useRouter()
const artists = ref<ArtistSummary[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await api.artists.list({ pageSize: 2000 })
    artists.value = res.artists
  } finally {
    loading.value = false
  }
})

const grouped = computed(() => {
  const groups = new Map<string, ArtistSummary[]>()
  for (const artist of artists.value) {
    const letter = /^[a-z]/i.test(artist.name) ? artist.name[0].toUpperCase() : '#'
    if (!groups.has(letter)) groups.set(letter, [])
    groups.get(letter)!.push(artist)
  }
  return [...groups.entries()].sort((a, b) => {
    if (a[0] === '#') return 1
    if (b[0] === '#') return -1
    return a[0].localeCompare(b[0])
  })
})

function avatarColor(name: string) {
  const colors = [
    'rgba(124,58,237,0.3)', 'rgba(219,39,119,0.3)', 'rgba(2,132,199,0.3)',
    'rgba(5,150,105,0.3)', 'rgba(217,119,6,0.3)', 'rgba(220,38,38,0.3)'
  ]
  let hash = 0
  for (const c of name) hash = (hash + c.charCodeAt(0)) % colors.length
  return colors[hash]
}
</script>

<template>
  <div class="p-7">
    <div class="flex items-baseline justify-between mb-6">
      <h1 class="text-2xl font-bold text-white tracking-tight">Artists</h1>
      <span v-if="artists.length" class="text-white/25 text-sm">{{ artists.length }} artists</span>
    </div>

    <div v-if="loading" class="flex justify-center mt-16">
      <svg class="w-4 h-4 animate-spin text-white/30" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
      </svg>
    </div>

    <div v-else class="space-y-8">
      <div v-for="[letter, group] in grouped" :key="letter">
        <p class="text-white/20 text-xs font-bold uppercase tracking-widest mb-2 px-3">{{ letter }}</p>
        <div class="rounded-2xl overflow-hidden" style="background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.06);">
          <button
            v-for="(artist, i) in group"
            :key="artist.id"
            @click="router.push(`/artist/${toSlug(artist.normalizedName, artist.id)}`)"
            class="w-full flex items-center gap-3.5 px-4 py-3.5 hover:bg-white/[0.04] transition-colors text-left group"
            :class="i < group.length - 1 ? 'border-b border-white/[0.04]' : ''"
          >
            <div
              class="w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0 text-sm font-bold text-white/60"
              :style="{ background: avatarColor(artist.name) }"
            >
              {{ artist.name[0]?.toUpperCase() }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-white/85 font-medium text-sm truncate">{{ artist.name }}</p>
            </div>
            <div class="flex items-center gap-2">
              <span class="text-white/25 text-xs">{{ artist.albumCount }} {{ artist.albumCount === 1 ? 'album' : 'albums' }}</span>
              <svg class="w-3.5 h-3.5 text-white/15 group-hover:text-white/30 transition-colors" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
                <path d="M9 18l6-6-6-6"/>
              </svg>
            </div>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
