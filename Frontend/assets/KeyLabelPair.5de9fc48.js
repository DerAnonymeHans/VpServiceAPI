import{_ as h,o as n,c as s,F as f,g as u,n as r,t as m,b,h as d,d as p,i as _}from"./index.07e0a135.js";const g={props:{options:{type:Array,required:!0},default:String,invert:Boolean,value:String},data(){return{selected:0,left:"0"}},watch:{value(e){const t=this.options.findIndex(i=>e===i.key);t!==-1&&this.doSwitch(t)}},mounted(){const e=this.options.findIndex(t=>this.default===t.key);e!==-1&&this.doSwitch(e)},methods:{doSwitch(e){this.selected=e,this.left=100/this.options.length*e+"%",this.$emit("switch",this.options[this.selected].key)}}};class j{constructor(t,i){this.options=t,this.value=i}}const w=["onClick"];function y(e,t,i,o,a,l){return n(),s("div",{class:r(["switch-container",{invert:i.invert}]),style:d({"grid-template-columns":"1fr ".repeat(i.options.length)})},[(n(!0),s(f,null,u(i.options,(v,c)=>(n(),s("div",{key:c,class:r({selected:a.selected===c}),onClick:C=>l.doSwitch(c)},m(v.label),11,w))),128)),b("div",{class:"switcher",style:d({left:a.left,width:100/i.options.length+"%"})},null,4)],6)}var q=h(g,[["render",y],["__scopeId","data-v-79ba7b35"]]);const x=["onClick"],k={data(){return{selected:0,mydefault:this.default}},inject:["mq"],props:{items:{type:Array,required:!0},default:String},mounted(){setTimeout(()=>{const e=this.items.findIndex(t=>t.key===this.mydefault);e!==-1&&this.onClick(e)},0)},updated(){if(this.items.findIndex(t=>t.key===this.mydefault)===-1){this.onClick(0);return}},methods:{onClick(e){this.selected=e,this.mydefault=this.items[this.selected].key,this.$emit("select",this.items[this.selected])}}};class z{constructor(t,i){this.name=t,this.key=i}}const S=Object.assign(k,{__name:"ScrollSelector",setup(e){return(t,i)=>(n(),s("div",{class:r(["scroll-selector-container flex",t.mq.current])},[(n(!0),s(f,null,u(e.items,(o,a)=>(n(),s("div",{class:r(["item",{selected:a===t.selected}]),key:a,onClick:l=>t.onClick(a)},[p(_,{name:o.key},null,8,["name"]),b("div",null,m(o.name),1)],10,x))),128))],2))}});var B=h(S,[["__scopeId","data-v-cb646256"]]);class ${constructor(t,i){this.key=t,this.label=i}}export{z as I,$ as K,q as S,B as a,j as b};
