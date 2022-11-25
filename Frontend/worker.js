/** @format */

console.log("Service worker loaded");
const CACHE_NAME = "kepleraner-cache-v1";
// const API_URL = "https://vp-service-api.herokuapp.com";
const API_URL = "";

self.addEventListener("push", (e) => {
   const options = e.data.json();
   console.log("SW: received push", options);
   fetch(API_URL + "/api/User/ConfirmPush", { method: "POST", body: options.data.UserName });
   self.registration.showNotification(options.title, options);
});

self.addEventListener("notificationclick", (e) => {
   e.notification.close();
   console.log(e.notification);
   e.waitUntil(clients.openWindow(`https://kepleraner.onrender.com${e.notification.data.Action}`));
});

self.addEventListener("install", (e) => {
   e.waitUntil(
      caches.open(CACHE_NAME).then((cache) => {
         console.log("Start caching");
         cache.addAll().then(() => self.skipWaiting());
      })
   );
});

self.addEventListener("activate", (e) => {
   e.waitUntil(
      caches.keys().then((cacheNames) => {
         return Promise.all(
            cacheNames.map((cache) => {
               if (cache !== CACHE_NAME) {
                  console.log("SW: Clear old Cache");
                  return caches.delete(cache);
               }
            })
         );
      })
   );
});

self.addEventListener("fetch", (e) => {
   if (e.request.method !== "GET") return;
   if (e.request.url.includes("api")) return;
   // if (!e.request.url.includes("http://localhost:8080") && !e.request.url.includes("https://kepleraner")) return;

   console.log("SW: Responsing fetch", e);
   e.respondWith(
      fetch(e.request)
         .then((res) => {
            const resClone = res.clone();
            caches.open(CACHE_NAME).then((cache) => {
               cache.put(e.request, resClone);
            });
            return res;
         })
         .catch((err) => caches.match(e.request).then((res) => res))
   );
});
