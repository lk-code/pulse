<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '../api'
import type { Track } from '../types'

interface Album {
  albumArtist: string
  album: string
  tracks: Track[]
  coverId: number
}

const router = useRouter()
const albums = ref<Album[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const res = await api.tracks.list({ pageSize: 2000 })
    const map = new Map<string, Album>()
    for (const track of res.tracks) {
      const key = `${track.albumArtist || track.artist}||${track.album}`
      if (!map.has(key)) {
        map.set(key, {
          albumArtist: track.albumArtist || track.artist,
          album: track.album || 'Unknown Album',
          tracks: [],
          coverId: track.id
        })
      }
      map.get(key)!.tracks.push(track)
    }
    albums.value = [...map.values()].sort((a, b) => {
      const ar = a.albumArtist.localeCompare(b.albumArtist)
      return ar !== 0 ? ar : a.album.localeCompare(b.album)
    })
  } finally {
    loading.value = false
  }
})

function openAlbum(album: Album) {
  router.push(`/library/album/${encodeURIComponent(album.albumArtist)}/${encodeURIComponent(album.album)}`)
}
</script>

<template>
  <div class="p-7">
    <div class="flex items-baseline justify-between mb-6">
      <h1 class="text-2xl font-bold text-white tracking-tight">Library</h1>
      <span v-if="albums.length" class="text-white/25 text-sm">{{ albums.length }} albums</span>
    </div>

    <div v-if="loading" class="flex items-center gap-3 text-white/30 mt-16 justify-center">
      <svg class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
      </svg>
      <span class="text-sm">Loading library…</span>
    </div>

    <div
      v-else-if="albums.length === 0"
      class="flex flex-col items-center justify-center mt-24 gap-4"
    >
      <div class="w-16 h-16 rounded-2xl flex items-center justify-center" style="background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.07);">
        <svg class="w-7 h-7 text-white/20" fill="currentColor" viewBox="0 0 24 24">
          <path d="M12 3v10.55c-.59-.34-1.27-.55-2-.55-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4V7h4V3h-6z"/>
        </svg>
      </div>
      <div class="text-center">
        <p class="text-white/60 font-medium">No tracks found</p>
        <p class="text-white/25 text-sm mt-1">Add a library in Settings and trigger a scan</p>
      </div>
    </div>

    <div v-else class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-6">
      <div
        v-for="album in albums"
        :key="`${album.albumArtist}-${album.album}`"
        @click="openAlbum(album)"
        class="cursor-pointer group"
      >
        <div class="aspect-square rounded-2xl overflow-hidden mb-2.5 relative" style="box-shadow: 0 8px 24px rgba(0,0,0,0.4);">
          <img
            :src="api.tracks.coverUrl(album.coverId)"
            :alt="album.album"
            class="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
          />
          <div class="absolute inset-0 transition-all duration-300 flex items-end justify-end p-2.5"
            style="background: linear-gradient(to top, rgba(0,0,0,0) 0%, rgba(0,0,0,0) 100%);"
            :style="{background: 'linear-gradient(to top, rgba(0,0,0,0.5) 0%, transparent 60%)'}"
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
        <p class="text-white/85 text-sm font-medium truncate leading-tight mt-3">{{ album.album }}</p>
        <p class="text-white/35 text-xs truncate mt-1">{{ album.albumArtist }}</p>
      </div>
    </div>
  </div>
</template>
