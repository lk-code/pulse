<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { api } from '../api'
import { usePlayerStore } from '../stores/playerStore'
import { toSlug, parseSlug } from '../composables/useSlug'
import type { ArtistDetail } from '../types'

const route = useRoute()
const router = useRouter()
const player = usePlayerStore()

const artistSlug = computed(() => route.params.artistSlug as string)
const artistId = computed(() => parseSlug(artistSlug.value).id)

const artist = ref<ArtistDetail | null>(null)
const loading = ref(true)

onMounted(async () => {
  try {
    artist.value = await api.artists.get(artistId.value)
  } finally {
    loading.value = false
  }
})

async function playAll() {
  if (!artist.value?.albums.length) return
  // load all tracks for artist and play
  const res = await api.tracks.list({ artistId: artistId.value, pageSize: 2000 })
  if (!res.tracks.length) return
  player.setQueue(res.tracks, 0)
  player.isPlaying = true
}

function albumUrl(albumId: number, albumNorm: string) {
  return `/artist/${artistSlug.value}/album/${toSlug(albumNorm, albumId)}`
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

        <div v-if="loading" class="flex justify-center mt-8">
          <svg class="w-4 h-4 animate-spin text-white/30" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
          </svg>
        </div>

        <div v-else-if="artist" class="flex items-end gap-5">
          <div
            class="w-28 h-28 rounded-full flex items-center justify-center flex-shrink-0 text-4xl font-bold text-white/30"
            style="background: rgba(124,58,237,0.15); border: 2px solid rgba(124,58,237,0.2); box-shadow: 0 8px 32px rgba(0,0,0,0.4);"
          >
            {{ artist.name[0]?.toUpperCase() }}
          </div>
          <div class="pb-1 min-w-0">
            <p class="text-white/40 text-xs font-semibold uppercase tracking-widest mb-1.5">Artist</p>
            <h1 class="text-white font-bold tracking-tight mb-1 truncate" style="font-size: clamp(1.4rem, 3vw, 2rem);">{{ artist.name }}</h1>
            <p class="text-white/30 text-sm mb-4">
              {{ artist.albums.length }} {{ artist.albums.length === 1 ? 'album' : 'albums' }}
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

    <div v-if="artist" class="flex-1 px-7 pb-8">
      <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-6">
        <div
          v-for="album in artist.albums"
          :key="album.id"
          @click="router.push(albumUrl(album.id, album.normalizedName))"
          class="cursor-pointer group"
        >
          <div class="aspect-square rounded-2xl overflow-hidden mb-2.5 relative" style="box-shadow: 0 8px 24px rgba(0,0,0,0.4);">
            <img
              :src="api.albums.coverUrl(album.id)"
              :alt="album.name"
              class="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
            />
            <div
              class="absolute inset-0 flex items-end justify-end p-2.5"
              style="background: linear-gradient(to top, rgba(0,0,0,0.5) 0%, transparent 60%);"
            >
              <div
                class="w-9 h-9 rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all duration-200 translate-y-1 group-hover:translate-y-0"
                style="background: rgba(255,255,255,0.95); box-shadow: 0 4px 16px rgba(0,0,0,0.4);"
              >
                <svg class="w-4 h-4 text-black ml-0.5" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M8 5v14l11-7z"/>
                </svg>
              </div>
            </div>
          </div>
          <p class="text-white/85 text-sm font-medium truncate leading-tight mt-3">{{ album.name }}</p>
          <p class="text-white/35 text-xs truncate mt-1">{{ album.year > 0 ? album.year : '' }}</p>
        </div>
      </div>
    </div>
  </div>
</template>
