<script setup lang="ts">
import { RouterLink } from 'vue-router'

const navItems = [
  { to: '/library', label: 'Library', icon: 'M4 6h16M4 10h16M4 14h16M4 18h16' },
  { to: '/artists', label: 'Artists', icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z' },
  { to: '/playlists', label: 'Playlists', icon: 'M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3' },
  { to: '/search', label: 'Search', icon: 'M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z' },
  { to: '/settings', label: 'Settings', icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z' }
]
</script>

<template>
  <aside class="w-56 flex-shrink-0 bg-[#09090e] border-r border-[#1e1e2e] flex flex-col">
    <div class="px-5 py-6 border-b border-[#1e1e2e]">
      <div class="flex items-center gap-2">
        <div class="w-8 h-8 rounded-lg bg-violet-600 flex items-center justify-center">
          <svg class="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 24 24">
            <path d="M12 3v10.55c-.59-.34-1.27-.55-2-.55-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4V7h4V3h-6z"/>
          </svg>
        </div>
        <span class="text-white font-bold text-xl tracking-tight">Pulse</span>
      </div>
    </div>

    <nav class="flex-1 py-4 px-3 space-y-1">
      <RouterLink
        v-for="item in navItems"
        :key="item.to"
        :to="item.to"
        class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors duration-150 group"
        :class="$route.path.startsWith(item.to)
          ? 'bg-violet-600/20 text-violet-300'
          : 'text-slate-400 hover:text-white hover:bg-white/5'"
      >
        <svg class="w-4.5 h-4.5 flex-shrink-0" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round">
          <path :d="item.icon"/>
        </svg>
        {{ item.label }}
      </RouterLink>
    </nav>
  </aside>
</template>
