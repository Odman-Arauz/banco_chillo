// Service Worker mínimo de Banco Chillo.
// Estrategia: la "cáscara" de la app (HTML, JS, íconos) se sirve desde caché si no hay red.
// Las llamadas a /api/ NUNCA se guardan en caché: los datos bancarios siempre vienen del servidor.
// El Issue de la PWA (#7) amplía esto (pantalla "sin conexión", versión de caché por despliegue, etc.).

const CACHE = 'banco-chillo-v1';
const CASCARA = ['/', '/manifest.json', '/icon-192.png', '/icon-512.png'];

self.addEventListener('install', (evento) => {
  evento.waitUntil(caches.open(CACHE).then((cache) => cache.addAll(CASCARA)));
  self.skipWaiting();
});

self.addEventListener('activate', (evento) => {
  evento.waitUntil(
    caches.keys().then((claves) => Promise.all(claves.filter((c) => c !== CACHE).map((c) => caches.delete(c)))),
  );
  self.clients.claim();
});

self.addEventListener('fetch', (evento) => {
  const url = new URL(evento.request.url);
  if (evento.request.method !== 'GET' || url.pathname.startsWith('/api/') || url.origin !== self.location.origin) {
    return; // red directa
  }
  evento.respondWith(
    fetch(evento.request)
      .then((respuesta) => {
        const copia = respuesta.clone();
        caches.open(CACHE).then((cache) => cache.put(evento.request, copia));
        return respuesta;
      })
      .catch(() => caches.match(evento.request).then((r) => r ?? caches.match('/'))),
  );
});
