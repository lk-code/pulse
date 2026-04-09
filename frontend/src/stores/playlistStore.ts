import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '../api'
import type { Playlist, PlaylistTrackEntry } from '../types'

export const usePlaylistStore = defineStore('playlist', () => {
  const playlists = ref<Playlist[]>([])
  const currentPlaylistTracks = ref<PlaylistTrackEntry[]>([])

  async function fetchPlaylists() {
    playlists.value = await api.playlists.list()
  }

  async function createPlaylist(name: string) {
    const playlist = await api.playlists.create(name)
    playlists.value.push(playlist)
    return playlist
  }

  async function deletePlaylist(id: number) {
    await api.playlists.delete(id)
    playlists.value = playlists.value.filter(p => p.id !== id)
  }

  async function fetchPlaylistTracks(id: number) {
    currentPlaylistTracks.value = await api.playlists.getTracks(id)
    return currentPlaylistTracks.value
  }

  async function addTrackToPlaylist(playlistId: number, trackId: number) {
    await api.playlists.addTrack(playlistId, trackId)
    const idx = playlists.value.findIndex(p => p.id === playlistId)
    if (idx !== -1) playlists.value[idx].trackCount++
  }

  async function removeTrackFromPlaylist(playlistId: number, trackId: number) {
    await api.playlists.removeTrack(playlistId, trackId)
    currentPlaylistTracks.value = currentPlaylistTracks.value.filter(
      pt => pt.track.id !== trackId
    )
    const idx = playlists.value.findIndex(p => p.id === playlistId)
    if (idx !== -1) playlists.value[idx].trackCount--
  }

  async function reorderTrack(playlistId: number, trackId: number, newPosition: number) {
    await api.playlists.reorder(playlistId, trackId, newPosition)
    await fetchPlaylistTracks(playlistId)
  }

  return {
    playlists,
    currentPlaylistTracks,
    fetchPlaylists,
    createPlaylist,
    deletePlaylist,
    fetchPlaylistTracks,
    addTrackToPlaylist,
    removeTrackFromPlaylist,
    reorderTrack
  }
})
