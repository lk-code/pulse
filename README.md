# Pulse

Pulse is a self-hosted music player for your local media library. Built with .NET 9 and Vue 3, served as a single Docker image. Add multiple libraries, scan your files, and stream audio and music videos straight from your browser — with automatic audio/video pairing, playlist management, and playback progress sync.

## Features

- **Library scanning** — recursively scans directories for audio and video files, reads metadata via TagLibSharp
- **Audio & video** — supports MP3, FLAC, OGG, WAV, AAC, M4A, Opus, MP4, WebM, MKV, MOV
- **Audio/video pairing** — when a track has both an audio and a video file, a toggle appears to switch modes while preserving playback position
- **Cover art** — extracts embedded cover art; falls back to an SVG placeholder
- **Playlists** — create playlists, add/remove tracks, drag to reorder
- **Playback progress** — saves position every 5 seconds; offers to resume on next load
- **Search** — real-time search across title, artist, and album
- **HTTP Range requests** — full seek support in the browser, including mid-file jumps
- **Keyboard shortcuts** — Space (play/pause), ← / → (seek ±5s), M (mute)

## Quick start

```yaml
# docker-compose.yml
services:
  pulse:
    image: pulse:latest
    build: .
    ports:
      - "8080:80"
    volumes:
      - /path/to/your/media:/media
      - pulse_data:/data
    restart: unless-stopped

volumes:
  pulse_data:
```

```bash
docker compose up --build -d
```

Open `http://localhost:8080`, go to **Settings**, add a library pointing to `/media`, then click **Scan**.

## Project structure

```
/
├── backend/          .NET 9 Minimal API (C#)
├── frontend/         Vue 3 + Vite + TypeScript + Pinia + Tailwind CSS
├── nginx/
│   └── nginx.conf
├── supervisord.conf
└── Dockerfile
```

## Volumes

| Path     | Mode       | Purpose                               |
|----------|------------|---------------------------------------|
| `/media` | read-write | Media files (mount your library here) |
| `/data`  | read-write | SQLite database + extracted cover art |

## Environment variables

| Variable           | Default              | Description                   |
|--------------------|----------------------|-------------------------------|
| `MUSIC_PATH`       | `/media`             | Root path for media files     |
| `DATA_PATH`        | `/data`              | Path for DB and cover art     |
| `ASPNETCORE_URLS`  | `http://localhost:5000` | Internal .NET listen address |

## API

All endpoints are prefixed with `/api`.

### Libraries
- `GET /api/libraries`
- `POST /api/libraries` — `{ name, rootPath }`
- `DELETE /api/libraries/{id}`
- `POST /api/libraries/{id}/scan`
- `GET /api/libraries/{id}/scan/status`

### Tracks
- `GET /api/tracks` — supports `libraryId`, `search`, `artist`, `album`, `page`, `pageSize`
- `GET /api/tracks/{id}`
- `GET /api/tracks/{id}/stream` — HTTP Range request support
- `GET /api/tracks/{id}/cover`

### Playlists
- `GET /api/playlists`
- `POST /api/playlists` — `{ name }`
- `DELETE /api/playlists/{id}`
- `GET /api/playlists/{id}/tracks`
- `POST /api/playlists/{id}/tracks` — `{ trackId }`
- `DELETE /api/playlists/{id}/tracks/{trackId}`
- `PUT /api/playlists/{id}/tracks/reorder` — `{ trackId, newPosition }`

### Progress
- `GET /api/progress/{trackId}`
- `PUT /api/progress/{trackId}` — `{ positionSeconds }`

## Development

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22](https://nodejs.org/) and npm

### Backend

```bash
cd backend

# Restore dependencies
dotnet restore

# Run (DB and cover art will be written to ./data/ by default)
DATA_PATH=./data ASPNETCORE_URLS=http://localhost:5000 dotnet run
```

The API is now available at `http://localhost:5000/api`.

### Frontend

```bash
cd frontend

# Install dependencies
npm install

# Start dev server (proxies /api/* to localhost:5000 automatically)
npm run dev
```

The app is now available at `http://localhost:5173`.

Both processes must run at the same time. The Vite dev server handles hot-reload for the frontend; the backend must be restarted manually after C# changes (or use `dotnet watch run` instead of `dotnet run`).

### Windows

Substitute the environment variables on Windows:

```powershell
$env:DATA_PATH="./data"; $env:ASPNETCORE_URLS="http://localhost:5000"; dotnet run
```

## Stack

- **Backend**: .NET 9 Minimal API, Entity Framework Core, SQLite, TagLibSharp
- **Frontend**: Vue 3, Vite, TypeScript, Pinia, Tailwind CSS v4, Vue Router
- **Runtime**: nginx (reverse proxy + SPA), supervisord (process manager)
- **Packaging**: Multi-stage Docker build
