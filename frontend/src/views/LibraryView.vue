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
  <div class="p-6">
    <h1 class="text-2xl font-bold text-white mb-6">Library</h1>

    <div v-if="loading" class="text-slate-400">Loading...</div>

    <div v-else-if="albums.length === 0" class="text-slate-400">
      No tracks found. Add a library in Settings and trigger a scan.
    </div>

    <div v-else class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-4">
      <div
        v-for="album in albums"
        :key="`${album.albumArtist}-${album.album}`"
        @click="openAlbum(album)"
        class="cursor-pointer group"
      >
        <div class="aspect-square rounded-lg overflow-hidden bg-[#1e1e2e] mb-2 relative">
          <img
            :src="api.tracks.coverUrl(album.coverId)"
            :alt="album.album"
            class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          />
          <div class="absolute inset-0 bg-black/0 group-hover:bg-black/30 transition-colors flex items-center justify-center">
            <div class="opacity-0 group-hover:opacity-100 transition-opacity w-10 h-10 rounded-full bg-violet-600 flex items-center justify-center shadow-xl">
              <svg class="w-5 h-5 text-white ml-0.5" fill="currentColor" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
            </div>
          </div>
        </div>
        <p class="text-white text-sm font-medium truncate">{{ album.album }}</p>
        <p class="text-slate-400 text-xs truncate">{{ album.albumArtist }}</p>
      </div>
    </div>
  </div>
</template>
