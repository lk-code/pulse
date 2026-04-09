import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/library' },
    { path: '/library', component: () => import('../views/LibraryView.vue') },
    { path: '/library/album/:albumArtist/:album', component: () => import('../views/AlbumDetailView.vue') },
    { path: '/artists', component: () => import('../views/ArtistsView.vue') },
    { path: '/artists/:artist', component: () => import('../views/ArtistTracksView.vue') },
    { path: '/playlists', component: () => import('../views/PlaylistsView.vue') },
    { path: '/playlists/:id', component: () => import('../views/PlaylistDetailView.vue') },
    { path: '/search', component: () => import('../views/SearchView.vue') },
    { path: '/settings', component: () => import('../views/SettingsView.vue') }
  ]
})

export default router
