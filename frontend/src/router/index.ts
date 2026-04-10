import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/library' },
    { path: '/library', component: () => import('../views/LibraryView.vue') },
    { path: '/artists', component: () => import('../views/ArtistsView.vue') },
    { path: '/artist/:artistSlug', component: () => import('../views/ArtistView.vue') },
    { path: '/artist/:artistSlug/album/:albumSlug', component: () => import('../views/AlbumDetailView.vue') },
    { path: '/playlists', component: () => import('../views/PlaylistsView.vue') },
    { path: '/playlists/:id', component: () => import('../views/PlaylistDetailView.vue') },
    { path: '/search', component: () => import('../views/SearchView.vue') },
    { path: '/settings', component: () => import('../views/SettingsView.vue') }
  ]
})

export default router
