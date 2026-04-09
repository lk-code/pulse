<script setup lang="ts">
import { RouterLink, useRoute } from 'vue-router'

const route = useRoute()

const navItems = [
  {
    to: '/library',
    label: 'Library',
    icon: `<path stroke-linecap="round" stroke-linejoin="round" d="M9 17.25v1.007a3 3 0 01-.879 2.122L7.5 21h9l-.621-.621A3 3 0 0115 18.257V17.25m6-12V15a2.25 2.25 0 01-2.25 2.25H5.25A2.25 2.25 0 013 15V5.25m18 0A2.25 2.25 0 0018.75 3H5.25A2.25 2.25 0 003 5.25m18 0H3"/>`
  },
  {
    to: '/artists',
    label: 'Artists',
    icon: `<path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z"/>`
  },
  {
    to: '/playlists',
    label: 'Playlists',
    icon: `<path stroke-linecap="round" stroke-linejoin="round" d="M8.25 6.75h12M8.25 12h12m-12 5.25h12M3.75 6.75h.007v.008H3.75V6.75zm.375 0a.375.375 0 11-.75 0 .375.375 0 01.75 0zM3.75 12h.007v.008H3.75V12zm.375 0a.375.375 0 11-.75 0 .375.375 0 01.75 0zm-.375 5.25h.007v.008H3.75v-.008zm.375 0a.375.375 0 11-.75 0 .375.375 0 01.75 0z"/>`
  },
  {
    to: '/search',
    label: 'Search',
    icon: `<path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z"/>`
  },
  {
    to: '/settings',
    label: 'Settings',
    icon: `<path stroke-linecap="round" stroke-linejoin="round" d="M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.325.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 011.37.49l1.296 2.247a1.125 1.125 0 01-.26 1.431l-1.003.827c-.293.241-.438.613-.43.992a7.723 7.723 0 010 .255c-.008.378.137.75.43.991l1.004.827c.424.35.534.955.26 1.43l-1.298 2.247a1.125 1.125 0 01-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.47 6.47 0 01-.22.128c-.331.183-.581.495-.644.869l-.213 1.281c-.09.543-.56.94-1.11.94h-2.594c-.55 0-1.019-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 01-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 01-1.369-.49l-1.297-2.247a1.125 1.125 0 01.26-1.431l1.004-.827c.292-.24.437-.613.43-.991a6.932 6.932 0 010-.255c.007-.38-.138-.751-.43-.992l-1.004-.827a1.125 1.125 0 01-.26-1.43l1.297-2.247a1.125 1.125 0 011.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.086.22-.128.332-.183.582-.495.644-.869l.214-1.28z"/><path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>`
  }
]

function isActive(to: string) {
  return route.path === to || (to !== '/' && route.path.startsWith(to))
}
</script>

<template>
  <aside
    class="w-56 flex-shrink-0 flex flex-col border-r border-white border-opacity-5"
    style="background: rgba(8,8,13,0.75); backdrop-filter: blur(20px);"
  >
    <div class="px-6 pt-10 pb-8">
      <div class="flex items-center gap-3">
        <div
          class="w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0"
          style="background: linear-gradient(135deg, #7c3aed, #a855f7);"
        >
          <svg class="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 24 24">
            <path d="M12 3v10.55c-.59-.34-1.27-.55-2-.55-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4V7h4V3h-6z"/>
          </svg>
        </div>
        <span class="text-white font-semibold text-xl tracking-tight">Pulse</span>
      </div>
    </div>

    <nav class="flex-1 px-3 flex flex-col gap-1">
      <RouterLink
        v-for="item in navItems"
        :key="item.to"
        :to="item.to"
        class="nav-item flex items-center gap-3 px-4 py-3 rounded-2xl text-sm font-medium transition-all duration-200 relative"
        :class="isActive(item.to) ? 'nav-item--active' : 'nav-item--inactive'"
      >
        <div
          v-if="isActive(item.to)"
          class="absolute inset-0 rounded-2xl"
          style="background: linear-gradient(135deg, rgba(124,58,237,0.3), rgba(168,85,247,0.15)); border: 1px solid rgba(139,92,246,0.25);"
        />
        <svg
          class="w-5 h-5 flex-shrink-0 relative z-10 transition-colors"
          :class="isActive(item.to) ? 'text-violet-300' : 'text-white text-opacity-30'"
          fill="none"
          stroke="currentColor"
          stroke-width="1.75"
          viewBox="0 0 24 24"
          v-html="item.icon"
        />
        <span class="relative z-10">{{ item.label }}</span>
      </RouterLink>
    </nav>

    <div class="px-6 py-6" style="border-top: 1px solid rgba(255,255,255,0.04);">
      <p class="text-xs" style="color: rgba(255,255,255,0.15);">Pulse · v1.0</p>
    </div>
  </aside>
</template>

<style scoped>
.nav-item--active {
  color: white;
}
.nav-item--inactive {
  color: rgba(255, 255, 255, 0.4);
}
.nav-item--inactive:hover {
  color: rgba(255, 255, 255, 0.75);
  background: rgba(255, 255, 255, 0.05);
}
</style>
