/** @format */

const PUBLIC_KEY = "BDdFjo9vM6wT1xcsqEXKPYS5EkU7NmhBmVXxLoI_TaVXIsQBl31RbEgFmym2XLb-1HN0uYVWFV6_48pxt8LqVfY";
const API_URL = "";
// const API_URL = "https://vp-service-api.herokuapp.com";

if ("serviceWorker" in navigator) {
   window.addEventListener("registerpush", (e) => {
      registerPush();
   });
}

function registerServiceWorker() {
   return new Promise((resolve, reject) => {
      navigator.serviceWorker
         .register("/worker.js", {
            scope: "/",
         })
         .then((registration) => {
            console.log("Service Worker registered");
            return resolve(registration);
         });
   });
}
registerServiceWorker();

async function registerPush() {
   if (Notification.permission === "denied") {
      alert(
         "Du hast Push Nachrichten blockiert. Bei Chrome musst du um sie zu erlauben auf das Schloss bei der URL klicken, anschließend auf Berechtigungen und Berechtigungen zurücksetzen."
      );
      return;
   }
   navigator.serviceWorker.ready.then(async (registration) => {
      if (registration === null || registration === undefined) {
         registration = await registerServiceWorker();
         if (registration === null || registration === undefined) {
            alert("Der Service Worker funktioniert nicht.");
            return;
         }
      }

      if (Notification.permission === "granted") {
         const subscription = await registration.pushManager.getSubscription();
         if (subscription !== null && subscription !== undefined) {
            updateSubscription(subscription);
            return;
         }
      }

      registration.pushManager
         .subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(PUBLIC_KEY),
         })
         .then(async (subscription) => {
            console.log("Push registered");

            updateSubscription(subscription);
         });
   });
}

async function updateSubscription(subscription) {
   if (subscription === null || subscription === undefined) {
      alert("Bei der Anmeldung der Push Nachrichten ist etwas schief gelaufen.");
      return;
   }
   const form = new FormData();
   form.append("subscribtion", JSON.stringify(subscription));

   await fetch(`${API_URL}/api/User/SetPushSubscribtion`, {
      method: "POST",
      body: form,
      credentials: "include",
   });
}

function urlBase64ToUint8Array(base64String) {
   const padding = "=".repeat((4 - (base64String.length % 4)) % 4);
   const base64 = (base64String + padding).replace(/\-/g, "+").replace(/_/g, "/");

   const rawData = window.atob(base64);
   const outputArray = new Uint8Array(rawData.length);

   for (let i = 0; i < rawData.length; ++i) {
      outputArray[i] = rawData.charCodeAt(i);
   }
   return outputArray;
}
