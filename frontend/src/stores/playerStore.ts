import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '../api'
import type { Track, RepeatMode, MediaMode } from '../types'

export const usePlayerStore = defineStore('player', () => {
  const currentTrack = ref<Track | null>(null)
  const queue = ref<Track[]>([])
  const queueIndex = ref(0)
  const isPlaying = ref(false)
  const currentTime = ref(0)
  const duration = ref(0)
  const shuffle = ref(false)
  const repeat = ref<RepeatMode>('off')
  const mediaMode = ref<MediaMode>('audio')
  const volume = ref(1)
  const isMuted = ref(false)
  const resumeToastVisible = ref(false)
  const resumePosition = ref(0)

  let progressInterval: ReturnType<typeof setInterval> | null = null

  const canSwitchMode = computed(() => currentTrack.value?.hasMatchingPair === true)

  function setQueue(tracks: Track[], startIndex = 0) {
    queue.value = tracks
    queueIndex.value = startIndex
    loadTrack(tracks[startIndex])
  }

  async function loadTrack(track: Track) {
    currentTrack.value = track
    mediaMode.value = track.fileType === 'Video' ? 'video' : 'audio'
    currentTime.value = 0
    duration.value = 0

    const progress = await api.progress.get(track.id).catch(() => null)
    if (progress && progress.positionSeconds > 10) {
      resumePosition.value = progress.positionSeconds
      resumeToastVisible.value = true
    }

    startProgressTracking()
  }

  function dismissResumeToast() {
    resumeToastVisible.value = false
  }

  function acceptResume() {
    currentTime.value = resumePosition.value
    resumeToastVisible.value = false
  }

  function startProgressTracking() {
    if (progressInterval) clearInterval(progressInterval)
    progressInterval = setInterval(() => {
      if (isPlaying.value && currentTrack.value) {
        api.progress.update(currentTrack.value.id, currentTime.value).catch(() => {})
      }
    }, 5000)
  }

  function stopProgressTracking() {
    if (progressInterval) {
      clearInterval(progressInterval)
      progressInterval = null
    }
  }

  function playNext() {
    if (queue.value.length === 0) return

    if (repeat.value === 'one') {
      currentTime.value = 0
      isPlaying.value = true
      return
    }

    let nextIndex: number
    if (shuffle.value) {
      nextIndex = Math.floor(Math.random() * queue.value.length)
    } else {
      nextIndex = queueIndex.value + 1
      if (nextIndex >= queue.value.length) {
        if (repeat.value === 'all') {
          nextIndex = 0
        } else {
          isPlaying.value = false
          return
        }
      }
    }

    queueIndex.value = nextIndex
    loadTrack(queue.value[nextIndex])
    isPlaying.value = true
  }

  function playPrevious() {
    if (queue.value.length === 0) return

    if (currentTime.value > 3) {
      currentTime.value = 0
      return
    }

    let prevIndex = queueIndex.value - 1
    if (prevIndex < 0) prevIndex = repeat.value === 'all' ? queue.value.length - 1 : 0

    queueIndex.value = prevIndex
    loadTrack(queue.value[prevIndex])
    isPlaying.value = true
  }

  function toggleShuffle() {
    shuffle.value = !shuffle.value
  }

  function cycleRepeat() {
    const modes: RepeatMode[] = ['off', 'all', 'one']
    const idx = modes.indexOf(repeat.value)
    repeat.value = modes[(idx + 1) % modes.length]
  }

  function toggleMediaMode() {
    if (!canSwitchMode.value) return
    mediaMode.value = mediaMode.value === 'audio' ? 'video' : 'audio'
  }

  function toggleMute() {
    isMuted.value = !isMuted.value
  }

  return {
    currentTrack,
    queue,
    queueIndex,
    isPlaying,
    currentTime,
    duration,
    shuffle,
    repeat,
    mediaMode,
    volume,
    isMuted,
    resumeToastVisible,
    resumePosition,
    canSwitchMode,
    setQueue,
    loadTrack,
    playNext,
    playPrevious,
    toggleShuffle,
    cycleRepeat,
    toggleMediaMode,
    toggleMute,
    dismissResumeToast,
    acceptResume,
    stopProgressTracking
  }
})
