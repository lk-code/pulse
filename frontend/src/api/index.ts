import type {
  Library, TracksResponse, Playlist, PlaylistTrackEntry,
  ScanStatus, PlaybackProgress, ArtistSummary, ArtistDetail, AlbumSummary, AlbumDetail
} from '../types'

const apiBase = import.meta.env.VITE_API_BASE_URL ?? ''

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${apiBase}${path}`, options)
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

  artists: {
    list: (params: { search?: string; page?: number; pageSize?: number } = {}) => {
      const qs = new URLSearchParams()
      if (params.search) qs.set('search', params.search)
      if (params.page != null) qs.set('page', String(params.page))
      if (params.pageSize != null) qs.set('pageSize', String(params.pageSize))
      return request<{ total: number; artists: ArtistSummary[] }>(`/api/artists?${qs}`)
    },
    get: (id: number) => request<ArtistDetail>(`/api/artists/${id}`)
  },

  albums: {
    list: (params: { search?: string; page?: number; pageSize?: number } = {}) => {
      const qs = new URLSearchParams()
      if (params.search) qs.set('search', params.search)
      if (params.page != null) qs.set('page', String(params.page))
      if (params.pageSize != null) qs.set('pageSize', String(params.pageSize))
      return request<{ total: number; albums: AlbumSummary[] }>(`/api/albums?${qs}`)
    },
    get: (id: number) => request<AlbumDetail>(`/api/albums/${id}`),
    coverUrl: (id: number) => `${apiBase}/api/albums/${id}/cover`
  },

  tracks: {
    list: (params: {
      libraryId?: number
      albumId?: number
      artistId?: number
      search?: string
      page?: number
      pageSize?: number
    } = {}) => {
      const qs = new URLSearchParams()
      if (params.libraryId != null) qs.set('libraryId', String(params.libraryId))
      if (params.albumId != null) qs.set('albumId', String(params.albumId))
      if (params.artistId != null) qs.set('artistId', String(params.artistId))
      if (params.search) qs.set('search', params.search)
      if (params.page != null) qs.set('page', String(params.page))
      if (params.pageSize != null) qs.set('pageSize', String(params.pageSize))
      return request<TracksResponse>(`/api/tracks?${qs}`)
    },
    streamUrl: (id: number) => `${apiBase}/api/tracks/${id}/stream`
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
