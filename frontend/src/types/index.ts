export type FileType = 'Audio' | 'Video'

export interface Library {
  id: number
  name: string
  rootPath: string
  createdAt: string
  lastScannedAt: string | null
}

export interface Track {
  id: number
  libraryId: number
  title: string
  artist: string
  album: string
  albumArtist: string
  year: number
  genre: string
  durationSeconds: number
  fileType: FileType
  hasMatchingPair: boolean
  isAvailable: boolean
}

export interface TracksResponse {
  total: number
  page: number
  pageSize: number
  tracks: Track[]
}

export interface Playlist {
  id: number
  name: string
  createdAt: string
  trackCount: number
}

export interface PlaylistTrackEntry {
  position: number
  track: Track
}

export interface ScanStatus {
  isScanning: boolean
  lastScannedAt: string | null
  trackCount: number
  currentFile: string | null
  processedFiles: number
  totalFiles: number
}

export interface PlaybackProgress {
  trackId: number
  positionSeconds: number
  updatedAt: string | null
}

export type RepeatMode = 'off' | 'all' | 'one'
export type MediaMode = 'audio' | 'video'
