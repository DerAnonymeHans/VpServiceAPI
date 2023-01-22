import{_ as u,o,c as i,b as n,F as w,g as _,t as d,k as v,n as p,d as s,l as x,w as b,p as y,e as k,m as I}from"./index.8e60bec0.js";import{I as S}from"./Input.8709466c.js";const T={inject:["mq"],props:{title:{type:Object,required:!0},isInvert:Boolean,text:String},data(){return{invert:this.isInvert?"invert":""}}},z={class:"heading-container"},C={key:0,class:"colored"},F={key:1},D={key:0,class:"content-container"},M={key:1,class:"content-container"};function $(a,t,e,r,f,c){return o(),i("section",{class:p([c.mq.current,f.invert])},[n("div",z,[(o(!0),i(w,null,_(e.title,l=>(o(),i("div",{key:l},[l.isColored?(o(),i("h2",C,d(l.value),1)):(o(),i("h2",F,d(l.value),1))]))),128))]),e.isInvert&&c.mq.desktop?(o(),i("div",D,[v(a.$slots,"default"),n("article",null,d(e.text),1)])):(o(),i("div",M,[n("article",null,d(e.text),1),v(a.$slots,"default")]))],2)}var g=u(T,[["render",$]]);const q={props:{type:{type:String,required:!0},label:{type:String,required:!0},name:String,isInvert:{type:Boolean,required:!0},defaultRows:Number}},B=["name","rows"];function V(a,t,e,r,f,c){return o(),i("div",{class:p({invert:e.isInvert})},[n("label",null,d(e.label),1),n("textarea",{name:e.name,rows:e.defaultRows},null,8,B)],2)}var h=u(q,[["render",V],["__scopeId","data-v-41151761"]]);const m=a=>(y("data-v-04e5e897"),a=a(),k(),a),A=m(()=>n("iframe",{id:"dummy",style:{display:"none",opacity:"0"}},null,-1)),E=m(()=>n("div",{class:"center margin-vert"},[n("button",{class:"btn-focus"},"Vorschlagen")],-1)),j=m(()=>n("div",{class:"center margin-vert"},[n("button",{class:"btn-focus"},"Senden")],-1)),N={data(){return{kleinAberFeinText:"Das kleine aber feine Extra ist ein wichtiger Bestandteil der Mails. Durch interessante Fakten, lustige Spr\xFCche oder weise Zitate wird die Email um ein Vielfaches aufgewertet. Deshalb ist es wichtig, dass es stets Nachschub an kleinen Extras gibt... und daf\xFCr bist auch DU verantwortlich. Also, suche etwas und schlage es doch vor.",aufmerksamMachenText:"Wenn dir etwas auff\xE4llt, bspw ein Bug oder unerwartetes Verhalten, dann kannst du es hier melden. Wenn du Verbesserungsvorschl\xE4ge hast oder gar Ideen f\xFCr ganz neue Features dann schreib diese bitte auch hier. Durch Feedback kann Kepleraner immer besser werden, also trau dich und schreib etwas (nat\xFCrlich ganz anonym).",showModal:!1,modalTitle:"",modalContent:""}},methods:{submit(a){a.preventDefault();const t=a.target.attributes.action.nodeValue,e=new FormData(a.target);I("/User"+t,{method:"POST",body:e}).then(r=>r.json()).then(r=>{r.isSuccess?(this.modalTitle="Erfolg",a.target.reset()):this.modalTitle="Fehlschlag",this.modalContent=r.message,this.showModal=!0})}}},P=Object.assign(N,{__name:"Index",setup(a){return(t,e)=>(o(),i("main",null,[s(x,{isOpen:t.showModal,onClose:e[0]||(e[0]=r=>t.showModal=!t.showModal),title:t.modalTitle,content:t.modalContent,buttons:[]},null,8,["isOpen","title","content"]),A,s(g,{title:[{value:"Klein aber\xA0"},{value:"fein",isColored:!0}],isInvert:!1,text:t.kleinAberFeinText},{default:b(()=>[n("form",{action:"/ProposeSmallExtra",onSubmit:e[1]||(e[1]=(...r)=>t.submit&&t.submit(...r)),target:"dummy"},[s(h,{isInvert:!0,label:"Text",name:"text"}),s(S,{isInvert:!0,label:"Autor (optional)",type:"text",name:"author"}),E],32)]),_:1},8,["text"]),s(g,{title:[{value:"Aufmerksam\xA0",isColored:!0},{value:"machen"}],isInvert:!0,text:t.aufmerksamMachenText},{default:b(()=>[n("form",{action:"/Proposal",onSubmit:e[2]||(e[2]=(...r)=>t.submit&&t.submit(...r)),target:"dummy"},[s(h,{isInvert:!0,label:"Text",name:"text",defaultRows:7}),j],32)]),_:1},8,["text"])]))}});var O=u(P,[["__scopeId","data-v-04e5e897"]]);export{O as default};
