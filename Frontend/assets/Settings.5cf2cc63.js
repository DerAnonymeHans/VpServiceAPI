import{_ as m,o as i,c,F as u,g as f,n as l,t as d,b as r,h as b,d as _,i as y,a as h,j as v}from"./index.44b6aa09.js";const w={props:{options:{type:Array,required:!0},default:String,invert:Boolean,value:String},data(){return{selected:0,left:"0"}},watch:{value(t){const e=this.options.findIndex(n=>t===n.key);e!==-1&&this.doSwitch(e)}},mounted(){const t=this.options.findIndex(e=>this.default===e.key);t!==-1&&this.doSwitch(t)},methods:{doSwitch(t){this.selected=t,this.left=100/this.options.length*t+"%",this.$emit("switch",this.options[this.selected].key)}}};class L{constructor(e,n){this.options=e,this.value=n}}const k=["onClick"];function x(t,e,n,o,a,s){return i(),c("div",{class:l(["switch-container",{invert:n.invert}]),style:b({"grid-template-columns":"1fr ".repeat(n.options.length)})},[(i(!0),c(u,null,f(n.options,(p,g)=>(i(),c("div",{key:g,class:l({selected:a.selected===g}),onClick:F=>s.doSwitch(g)},d(p.label),11,k))),128)),r("div",{class:"switcher",style:b({left:a.left,width:100/n.options.length+"%"})},null,4)],6)}var S=m(w,[["render",x],["__scopeId","data-v-79ba7b35"]]);const C=["onClick"],$={data(){return{selected:0,mydefault:this.default}},inject:["mq"],props:{items:{type:Array,required:!0},default:String},mounted(){setTimeout(()=>{const t=this.items.findIndex(e=>e.key===this.mydefault);t!==-1&&this.onClick(t)},0)},updated(){if(this.items.findIndex(e=>e.key===this.mydefault)===-1){this.onClick(0);return}},methods:{onClick(t){this.selected=t,this.mydefault=this.items[this.selected].key,this.$emit("select",this.items[this.selected])}}};class M{constructor(e,n){this.name=e,this.key=n}}const q=Object.assign($,{__name:"ScrollSelector",setup(t){return(e,n)=>(i(),c("div",{class:l(["scroll-selector-container flex",e.mq.current])},[(i(!0),c(u,null,f(t.items,(o,a)=>(i(),c("div",{class:l(["item",{selected:a===e.selected}]),key:a,onClick:s=>e.onClick(a)},[_(y,{name:o.key},null,8,["name"]),r("div",null,d(o.name),1)],10,C))),128))],2))}});var D=m(q,[["__scopeId","data-v-cb646256"]]);class E{constructor(e,n){this.key=e,this.label=n}}const I={class:"text"},j={class:"name"},V={class:"desc"},B={inject:["mq"],props:{_key:{type:String,required:!0},name:{type:String,required:!0},description:String,options:{type:Array,required:!0},defaultValue:String}},z=Object.assign(B,{__name:"Setting",setup(t){return(e,n)=>(i(),c("div",{class:l([e.mq.current,"setting"])},[r("div",{class:l(["setting-content",{isSwitch:t.options.length>1}])},[r("div",I,[r("p",j,d(t.name),1),r("p",V,d(t.description),1)]),t.options.length===1?(i(),c("button",{key:0,class:"btn",onClick:n[0]||(n[0]=o=>e.$emit("optionChange",{option:t._key,value:t.options[0].key}))},d(t.options[0].label),1)):h("",!0),t.options.length>1?(i(),v(S,{key:1,invert:!0,options:t.options,default:t.defaultValue,onSwitch:n[1]||(n[1]=o=>e.$emit("optionChange",{option:t._key,value:o}))},null,8,["options","default"])):h("",!0)],2)],2))}});var A=m(z,[["__scopeId","data-v-cec8682c"]]);const O={props:{settings:{type:Array,required:!0}}};class P{constructor(e,n){this.title=e,this.settings=n}}class T{constructor(e,n,o,a,s){this.key=e,this.name=n,this.desc=o,this.options=a,this.defaultVal=s}}const N=Object.assign(O,{__name:"Settings",setup(t){return(e,n)=>(i(!0),c(u,null,f(t.settings,o=>(i(),c("div",{class:"box",key:o.title},[r("h2",null,d(o.title),1),(i(!0),c(u,null,f(o.settings,a=>(i(),v(A,{key:a.key,_key:a.key,name:a.name,description:a.desc,options:a.options,defaultValue:a.defaultVal,onOptionChange:n[0]||(n[0]=s=>e.$emit("settingChange",s))},null,8,["_key","name","description","options","defaultValue"]))),128))]))),128))}});var G=m(N,[["__scopeId","data-v-32ce7dcf"]]);export{M as I,E as K,S,D as a,L as b,G as c,P as d,T as e};
