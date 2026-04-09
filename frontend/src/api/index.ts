import type { Library, Track, TracksResponse, Playlist, PlaylistTrackEntry, ScanStatus, PlaybackProgress } from '../types'

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(path, options)
  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  if (res.status === 204) return undefined as T
  return res.json()
}

export const api = {
  libraries: {
    list: () => request<Library[]>('/api/libraries'),
    create: (name: string, rootPath: string) =>
      request<Library>('/api/libraries', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, rootPath })
      }),
    delete: (id: number) =>
      request<void>(`/api/libraries/${id}`, { method: 'DELETE' }),
    scan: (id: number) =>
      request<void>(`/api/libraries/${id}/scan`, { method: 'POST' }),
    scanStatus: (id: number) =>
      request<ScanStatus>(`/api/libraries/${id}/scan/status`)
  },

  tracks: {
    list: (params: {
      libraryId?: number
      search?: string
      artist?: string
      album?: string
      page?: number
      pageSize?: number
    }) => {
      const qs = new URLSearchParams()
      if (params.libraryId != null) qs.set('libraryId', String(params.libraryId))
      if (params.search) qs.set('search', params.search)
      if (params.artist) qs.set('artist', params.artist)
      if (params.album) qs.set('album', params.album)
      if (params.page != null) qs.set('page', String(params.page))
      if (params.pageSize != null) qs.set('pageSize', String(params.pageSize))
      return request<TracksResponse>(`/api/tracks?${qs}`)
    },
    get: (id: number) => request<Track>(`/api/tracks/${id}`),
    streamUrl: (id: number) => `/api/tracks/${id}/stream`,
    coverUrl: (id: number) => `/api/tracks/${id}/cover`
  },

  playlists: {
    list: () => request<Playlist[]>('/api/playlists'),
    create: (name: string) =>
      request<Playlist>('/api/playlists', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name })
      }),
    delete: (id: number) =>
      request<void>(`/api/playlists/${id}`, { method: 'DELETE' }),
    getTracks: (id: number) =>
      request<PlaylistTrackEntry[]>(`/api/playlists/${id}/tracks`),
    addTrack: (playlistId: number, trackId: number) =>
      request<void>(`/api/playlists/${playlistId}/tracks`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ trackId })
      }),
    removeTrack: (playlistId: number, trackId: number) =>
      request<void>(`/api/playlists/${playlistId}/tracks/${trackId}`, { method: 'DELETE' }),
    reorder: (playlistId: number, trackId: number, newPosition: number) =>
      request<void>(`/api/playlists/${playlistId}/tracks/reorder`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ trackId, newPosition })
      })
  },

  progress: {
    get: (trackId: number) => request<PlaybackProgress>(`/api/progress/${trackId}`),
    update: (trackId: number, positionSeconds: number) =>
      request<void>(`/api/progress/${trackId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ positionSeconds })
      })
  }
}
