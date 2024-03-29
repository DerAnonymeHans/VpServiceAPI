//const URL = "https://localhost:5001"
//const URL = "http://vp-service-api.herokuapp.com"
const URL = "/api";
const request = (path, options={}) => new Promise(async(resolve, reject) => {
   const response = await fetch(URL + path, options)
      .then(res => res.json())
      .catch(res => {
         resolve(res.text())
      })
      .catch(res => resolve(res))
   if(typeof response !== "object") resolve(response);
   if(response.message !== null){
      alert(`Success: ${response.isSuccess}
         ${response.message}
      `)
   }
   if(response.isSuccess === false) return null;
   return resolve(response.body)

})

async function getAllValues(){
   const valueElements = [...document.querySelectorAll("[data-get]")];
   for(const el of valueElements){
       const url = el.dataset.get;
       el.innerText = await request(url);
   }
}

async function post(valElId){
   const el = document.getElementById(valElId);
   if(el === null) return;
   let url = el.dataset.post;
   const val = el.value;
   url = url.replace("{}", val);
   await request(url, {method: "POST"});

   reload()
}


window.onload = () => {
    if (window.location.origin === "http://vp-service-api.herokuapp.com") window.location = "https://vp-service-api.herokuapp.com/admin"
    getAllValues();
}

function reload(){
   getAllValues();
}
reload()