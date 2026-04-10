export type FileType = 'Audio' | 'Video'

export interface Library {
  id: number
  name: string
  rootPath: string
  createdAt: string
  lastScannedAt: string | null
}

export interface Artist {
  id: number
  name: string
  normalizedName: string
}

export interface ArtistSummary extends Artist {
  albumCount: number
  trackCount: number
}

export interface AlbumSummary {
  id: number
  name: string
  normalizedName: string
  year: number
  genre: string
  coverArtPath: string | null
  trackCount: number
  primaryArtist: Artist | null
}

export interface AlbumDetail {
  id: number
  name: string
  normalizedName: string
  year: number
  genre: string
  coverArtPath: string | null
  artists: (Artist & { isPrimary: boolean })[]
  tracks: TrackInAlbum[]
}

export interface TrackInAlbum {
  id: number
  title: string
  normalizedName: string
  trackNumber: number
  discNumber: number
  durationSeconds: number
  fileType: FileType
  hasMatchingPair: boolean
  pairedTrackId: number | null
  artist: Artist
}

export interface Track {
  id: number
  libraryId: number
  title: string
  normalizedName: string
  trackNumber: number
  discNumber: number
  durationSeconds: number
  fileType: FileType
  hasMatchingPair: boolean
  pairedTrackId: number | null
  isAvailable: boolean
  artist: Artist
  album: {
    id: number
    name: string
    normalizedName: string
    year: number
    primaryArtist: Artist | null
  }
}

export interface TracksResponse {
  total: number
  page: number
  pageSize: number
  tracks: Track[]
}

export interface ArtistDetail extends Artist {
  albums: AlbumSummary[]
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
