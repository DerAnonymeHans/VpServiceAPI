var me=Object.defineProperty;var W=Object.getOwnPropertySymbols;var ve=Object.prototype.hasOwnProperty,pe=Object.prototype.propertyIsEnumerable;var Q=(n,e,t)=>e in n?me(n,e,{enumerable:!0,configurable:!0,writable:!0,value:t}):n[e]=t,P=(n,e)=>{for(var t in e||(e={}))ve.call(e,t)&&Q(n,t,e[t]);if(W)for(var t of W(e))pe.call(e,t)&&Q(n,t,e[t]);return n};import{S as ke,K as X,a as we,I as R}from"./KeyLabelPair.b5d11460.js";import{o as d,c as f,d as k,i as _e,n as N,_ as H,b as i,F as O,g as E,t as w,v as ye,z as xe,u as C,a as I,p as ee,e as ne,l as te,f as Y,j as B}from"./index.1749c1f4.js";class p{constructor(e,t,r,u,b,o,v,m){this.name=e,this.field=t,this.specificField=r,this.canBeDeselectedBy=u,this.writeExamPossible=b,this.oralExamPossible=o,this.canBeLeistungskurs=v,this.examMandatory=m}}class S{constructor(e,t){this.name=e,this.duplicate=t}}const g=Object.freeze({science:"NaWi",language_literature_art:"Sprache-Kunst",society:"GeWi"}),h=Object.freeze({language:"Sprache",art_music:"Kunst-Musik",german_math:"Deutsch-Mathe",society:"GeWi",science:"NaWi",informatics:"Informatik",ethics_religion:"Etihk-Religion",sports:"Sport",astro_philo:"Astro-Philo"}),Oe=new p("Mathe",g.science,h.german_math,[],!0,!0,!0,!0),Se=new p("Deutsch",g.language_literature_art,h.german_math,[],!0,!0,!0,!0),Ee=new p("Englisch",g.language_literature_art,h.language,[],!0,!0,!0),je=new p("Franz\xF6sisch",g.language_literature_art,h.language,[],!1,!0),Ie=new p("Russisch",g.language_literature_art,h.language,[],!1,!0),$e=new p("Latein",g.language_literature_art,h.language,[],!1,!0),Ae=new p("Kunst",g.language_literature_art,h.art_music,[],!1,!0),Le=new p("Musik",g.language_literature_art,h.art_music,[],!1,!0),Ne=new p("Geschichte",g.society,h.art_music,[],!0,!0,!0),Ge=new p("GRW",g.society,h.society,[],!0,!0),Pe=new p("Geographie",g.society,h.society,[],!0,!0),De=new p("Physik",g.science,h.science,[],!0,!0,!0),Ce=new p("Chemie",g.science,h.science,[],!0,!0,!0),Te=new p("Biologie",g.science,h.science,[],!0,!0,!0),Ye=new p("Informatik",g.science,h.informatics,[],!1,!0),He=new p("Religion",g.other,h.ethics_religion,[],!1,!0),Ke=new p("Ethik",g.other,h.ethics_religion,[],!1,!0),Me=new p("Sport",g.other,h.sports,[],!1,!1),Fe=new p("Astronomie",g.other,h.astro_philo,[],!1,!1),Re=new p("Philosophie",g.other,h.astro_philo,[],!1,!1),l={ALL:[Oe,Se,Ee,je,$e,Ie,Ae,Le,Ne,Ge,Pe,De,Ce,Te,Ye,He,Ke,Me,Fe,Re],sciences:[],languages:[],secondLanguages:[],society:[],possibleLeistungskurse:[],possibleLeistungskurse2:[],writeExamPossible:[],oralExamPossible:[],getSubject(n){return l.ALL.find(e=>e.name===n)}};l.sciences=l.ALL.filter(n=>n.specificField===h.science);l.languages=l.ALL.filter(n=>n.specificField===h.language);l.secondLanguages=l.languages.filter(n=>n.name!=="Englisch");l.society=l.ALL.filter(n=>n.field===g.society);l.possibleLeistungskurse=l.ALL.filter(n=>n.canBeLeistungskurs);l.possibleLeistungskurse2=l.ALL.filter(n=>n.canBeLeistungskurs&&n.name!=="Deutsch"&&n.name!=="Mathe");l.writeExamPossible=l.ALL.filter(n=>n.writeExamPossible);l.oralExamPossible=l.ALL.filter(n=>n.oralExamPossible);const re={lk1:"Mathe",lk2:"",gk1:"Deutsch",lang1:"",lang2:"",nawi1:"",nawi2:"",nawi3:"",art_music:"",reli_ethic:"",gewi1:"",gewi2:"",history:"Geschichte",sports:"Sport"};function z(){return P({},re)}const Be={points:null,einbringen:""};function T(){return P({},Be)}function ze(){const n={};for(const e in re)n[e]={name:"",exam:null,examPoints:null,years:{1:T(),2:T(),3:T(),4:T()}};return n}const qe={p1:"",p2:"",p3:"",p4:"",p5:""};function ie(){return P({},qe)}const Je={props:{verified:{type:Boolean,required:!0}}},ae=Object.assign(Je,{__name:"VerifiedIcon",setup(n){return(e,t)=>(d(),f("div",{class:N(["verified-icon",n.verified?"verified":"warning"])},[k(_e,{name:n.verified?"verified":"warning"},null,8,["name"])],2))}}),Ve={props:{subjects:{type:Array,required:!0}}},Ue={label:"Nicht verwendet"},We=["value"],Qe={label:"Bereits verwendet"},Xe=["value"];function Ze(n,e,t,r,u,b){return d(),f(O,null,[i("optgroup",Ue,[(d(!0),f(O,null,E(t.subjects.filter(o=>!o.duplicate),o=>(d(),f("option",{key:o.name,value:o.name,class:N({duplicate:o.duplicate})},w(o.name),11,We))),128))]),i("optgroup",Qe,[(d(!0),f(O,null,E(t.subjects.filter(o=>o.duplicate),o=>(d(),f("option",{key:o.name,value:o.name,class:N({duplicate:o.duplicate})},w(o.name),11,Xe))),128))])],64)}var j=H(Ve,[["render",Ze]]);const x=n=>(ee("data-v-4499b814"),n=n(),ne(),n),en=x(()=>i("h4",null,"Leistungskurse",-1)),nn={class:"selectors"},tn={class:"label-select-container"},rn=x(()=>i("div",{class:"label"},"Leistungskurs 1",-1)),an=["value"],sn=x(()=>i("option",{value:"Deutsch"},"Deutsch",-1)),on=x(()=>i("option",{value:"Mathe"},"Mathe",-1)),ln=[sn,on],cn={class:"label-select-container"},dn=x(()=>i("div",{class:"label"},"Leistungskurs 2",-1)),un={class:"label-select-container"},hn=x(()=>i("div",{class:"label"},"Grundkurs (Je nach LK1)",-1)),fn=x(()=>i("h4",null,"Naturwissenschaften und Sprachen",-1)),bn={class:"center"},gn={class:"selectors"},mn={class:"label"},vn=["value","onChange"],pn={class:"label"},kn=["value","onChange"],wn=x(()=>i("h4",null,"Gesellschaftswissenschaften",-1)),_n={class:"selectors"},yn={key:0,class:"label-select-container"},xn=x(()=>i("div",{class:"label"},"Gewi 1",-1)),On=x(()=>i("div",null,"Geschichte",-1)),Sn=[xn,On],En={class:"label"},jn=["value","onChange"],In=x(()=>i("h4",null,"Andere",-1)),$n={class:"selectors"},An={class:"label-select-container"},Ln=x(()=>i("div",{class:"label"},"Musik oder Kunst",-1)),Nn=["value"],Gn={class:"label-select-container"},Pn=x(()=>i("div",{class:"label"},"Ethik oder Religion",-1)),Dn=["value"],Cn=x(()=>i("div",{class:"label-select-container"},[i("div",{class:"label"},"Sport"),i("div",null,"Sport")],-1)),Tn={inject:["mq"],data(){return{kurswahl:z(),nawiLangOptions:[new X("3-nawi","3 Nawi, 1 Sprache"),new X("2-nawi","2 Nawi, 2 Sprachen")],nawiLangOption:"3-nawi",numberOfGkNawi:3,numberOfGkLangs:1,lk2:[],nawi:[],langs:[],gewi:[],art_music:[],reli_ethic:[],isHistoryGk:!0,correctness:!1}},mounted(){this.load(),this.calculateAll(!1)},methods:{calculateAll(n=!0){this.getAllGroups(),this.getNumberOfGkNawiAndLangs(),this.checkCorrectNess(),this.correctness&&n&&this.save()},setAndRemoveDuplicate(n,e){for(const t of Object.entries(this.kurswahl))t[1]===e&&t[0]!==n&&(this.kurswahl[t[0]]="");this.kurswahl[n]=e,this.calculateAll()},isDuplicate(n){for(const e of Object.entries(this.kurswahl))if(e[1]===n)return!0;return!1},getAllGroups(){this.lk2=l.possibleLeistungskurse2.map(n=>new S(n.name,this.isDuplicate(n.name))),this.nawi=[...l.sciences,l.getSubject("Informatik")].map(n=>new S(n.name,this.isDuplicate(n.name))),this.langs=l.languages.map(n=>new S(n.name,this.isDuplicate(n.name))),this.gewi=[...l.society.filter(n=>n.name!=="Geschichte"),l.getSubject("Astronomie"),l.getSubject("Philosophie"),l.getSubject("Informatik")].map(n=>new S(n.name,this.isDuplicate(n.name))),this.art_music=[l.getSubject("Musik"),l.getSubject("Kunst")].map(n=>new S(n.name,this.isDuplicate(n.name))),this.reli_ethic=[l.getSubject("Ethik"),l.getSubject("Religion")].map(n=>new S(n.name,this.isDuplicate(n.name))),this.isHistoryGk=this.kurswahl.lk2!=="Geschichte",this.isHistoryGk?this.kurswahl.history="Geschichte":this.kurswahl.history=""},selectLk1(n){this.kurswahl.lk1=n,this.kurswahl.gk1=this.kurswahl.lk1==="Mathe"?"Deutsch":"Mathe"},changeNawiLangOption(n){this.nawiLangOption=n,n==="3-nawi"&&(this.kurswahl.lang2=""),n==="2-nawi"&&(this.kurswahl.nawi3=""),this.calculateAll()},getNumberOfGkNawiAndLangs(){var t,r;const n=(this.nawiLangOption==="3-nawi"?3:2)-(((t=l.getSubject(this.kurswahl.lk2))==null?void 0:t.field)===g.science?1:0);n<this.numberOfGkNawi&&(this.kurswahl.nawi3=""),this.numberOfGkNawi=n;const e=(this.nawiLangOption==="3-nawi"?1:2)-(((r=l.getSubject(this.kurswahl.lk2))==null?void 0:r.specificField)===h.language?1:0);e<this.numberOfGkLangs&&(this.kurswahl.lang2=""),this.numberOfGkLangs=e},checkCorrectNess(){this.correctness=(()=>!(this.kurswahl.lk1.length===0||this.kurswahl.lk2.length===0||this.numberOfGkNawi===1&&this.kurswahl.nawi1.length===0||this.numberOfGkNawi===2&&(this.kurswahl.nawi1.length===0||this.kurswahl.nawi2.length===0)||this.numberOfGkNawi===3&&(this.kurswahl.nawi1.length===0||this.kurswahl.nawi2.length===0||this.kurswahl.nawi3.length===0)||this.numberOfGkLangs===1&&this.kurswahl.lang1.length===0||this.numberOfGkLangs===2&&(this.kurswahl.lang1.length===0||this.kurswahl.lang2.length===0)||this.kurswahl.art_music.length===0||this.kurswahl.reli_ethic.length===0||this.kurswahl.gewi1.length===0||this.kurswahl.gewi2.length===0))()},save(){const n=P({},this.kurswahl);for(const e in n)n[e].length>0||delete n[e];localStorage.setItem("kurswahl",JSON.stringify(n)),localStorage.setItem("abif\xE4cher",void 0),localStorage.setItem("einbringen",void 0)},load(){try{const n=JSON.parse(localStorage.getItem("kurswahl"));if(n===void 0)return;for(const e in n)this.kurswahl[e]=n[e]}catch{}}}},Yn=Object.assign(Tn,{__name:"Kurswahl",setup(n){return(e,t)=>(d(),f("div",{class:N([e.mq.current,"container"])},[i("section",null,[en,i("div",nn,[i("div",tn,[rn,i("select",{class:"select",name:"leistungskurs-1",value:e.kurswahl.lk1,onChange:t[0]||(t[0]=r=>e.selectLk1(r.target.value))},ln,40,an)]),i("div",cn,[dn,ye(i("select",{class:"select",name:"leistungskurs-2","onUpdate:modelValue":t[1]||(t[1]=r=>e.kurswahl.lk2=r),onChange:t[2]||(t[2]=r=>e.setAndRemoveDuplicate("lk2",r.target.value))},[k(j,{subjects:e.lk2},null,8,["subjects"])],544),[[xe,e.kurswahl.lk2]])]),i("div",un,[hn,i("div",null,w(e.kurswahl.gk1),1)])])]),i("section",null,[fn,i("div",bn,[k(ke,{options:e.nawiLangOptions,default:e.nawiLangOption,onSwitch:e.changeNawiLangOption,invert:!0},null,8,["options","default","onSwitch"])]),i("div",gn,[(d(!0),f(O,null,E(e.numberOfGkNawi,r=>{var u;return d(),f("div",{class:"label-select-container",key:r},[i("div",mn,"Naturwissenschaft "+w(r+(((u=C(l).getSubject(e.kurswahl.lk2))==null?void 0:u.field)===C(g).science?1:0)),1),i("select",{class:"select",name:"naturwissenschaften-1",value:e.kurswahl[`nawi${r}`],onChange:b=>e.setAndRemoveDuplicate(`nawi${r}`,b.target.value)},[k(j,{subjects:e.nawi},null,8,["subjects"])],40,vn)])}),128)),(d(!0),f(O,null,E(e.numberOfGkLangs,r=>{var u;return d(),f("div",{class:"label-select-container",key:r},[i("div",pn,"Sprache "+w(r+(((u=C(l).getSubject(e.kurswahl.lk2))==null?void 0:u.specificField)===C(h).language?1:0)),1),i("select",{class:"select",name:"sprachen-1",value:e.kurswahl[`lang${r}`],onChange:b=>e.setAndRemoveDuplicate(`lang${r}`,b.target.value)},[k(j,{subjects:e.langs},null,8,["subjects"])],40,kn)])}),128))])]),i("section",null,[wn,i("div",_n,[e.isHistoryGk?(d(),f("div",yn,Sn)):I("",!0),(d(),f(O,null,E(2,r=>i("div",{class:"label-select-container",key:r},[i("div",En,"Gewi "+w(r+(e.isHistoryGk?1:0)),1),i("select",{class:"select",name:"gesellschaftswissenschaft-1",value:e.kurswahl[`gewi${r}`],onChange:u=>e.setAndRemoveDuplicate(`gewi${r}`,u.target.value)},[k(j,{subjects:e.gewi},null,8,["subjects"])],40,jn)])),64))])]),i("section",null,[In,i("div",$n,[i("div",An,[Ln,i("select",{class:"select",name:"musik-kunst-1",value:e.kurswahl.art_music,onChange:t[3]||(t[3]=r=>e.setAndRemoveDuplicate("art_music",r.target.value))},[k(j,{subjects:e.art_music},null,8,["subjects"])],40,Nn)]),i("div",Gn,[Pn,i("select",{class:"select",name:"ethik-religion-1",value:e.kurswahl.reli_ethic,onChange:t[4]||(t[4]=r=>e.setAndRemoveDuplicate("reli_ethic",r.target.value))},[k(j,{subjects:e.reli_ethic},null,8,["subjects"])],40,Dn)]),Cn])]),k(ae,{verified:e.correctness},null,8,["verified"])],2))}});var Hn=H(Yn,[["__scopeId","data-v-4499b814"]]);const Kn=i("h4",null,"Schriftliche Pr\xFCfungen",-1),Mn={class:"selectors"},Fn={class:"label-select-container"},Rn=i("div",{class:"label"},"P1",-1),Bn={class:"label-select-container"},zn=i("div",{class:"label"},"P2",-1),qn={class:"label-select-container"},Jn=i("div",{class:"label"},"P3",-1),Vn=["value"],Un=i("h4",null,"M\xFCndliche Pr\xFCfungen",-1),Wn={class:"selectors"},Qn={class:"label-select-container"},Xn=i("div",{class:"label"},"P4",-1),Zn=["value"],et={class:"label-select-container"},nt=i("div",{class:"label"},"P5",-1),tt=["value"],rt={data(){return{kurswahl:z(),exams:ie(),p3s:[],p4s:[],p5s:[],correctness:!1,showModal:!1,modalTitle:"",modalContent:""}},mounted(){this.load(),this.calculateAll()},methods:{calculateAll(){this.getAllGroups(),this.removeWrongExams(),this.checkCorrectness(),this.correctness&&this.save()},setAndRemoveDuplicate(n,e){for(const t of Object.entries(this.exams))t[1]===e&&t[0]!==n&&(this.exams[t[0]]="");this.exams[n]=e,this.calculateAll()},isDuplicate(n){return Object.values(this.exams).includes(n)},getAllGroups(){const n=Object.entries(this.kurswahl).filter(t=>!t[0].startsWith("lk")),e=t=>n.findIndex(r=>r[1]===t.name)!==-1;this.p3s=l.writeExamPossible.filter(e).map(t=>new S(t.name,this.isDuplicate(t.name))),this.p4s=(()=>this.exams.p3!==this.kurswahl.gk1?[new S(this.kurswahl.gk1,!1)]:l.oralExamPossible.filter(e))().map(t=>new S(t.name,this.isDuplicate(t.name))),this.p5s=(()=>{let t=l.oralExamPossible.filter(b=>e(b)&&b.name!==this.kurswahl.gk1);const r=(()=>{const b=Object.values(g);for(const o of Object.values(this.exams).slice(0,4)){const v=l.getSubject(o);v!==void 0&&b.includes(v.field)&&b.splice(b.indexOf(v.field),1)}return b})(),u=!Object.values(this.exams).some(b=>{const o=l.getSubject(b);return o?(o.specificField===h.language||o.specificField===h.science)&&o.name!=="Deutsch":!1});return r.length>0&&(t=t.filter(b=>r.includes(b.field))),u&&(t=t.filter(b=>b.specificField===h.language||b.specificField===h.science)),t})().map(t=>new S(t.name,this.isDuplicate(t.name)))},removeWrongExams(){for(let n=3;n<=5;n++)!this[`p${n}s`].find(e=>e.name===this.exams[`p${n}`])&&this.exams[`p${n}`].length>0&&(this.exams[`p${n}`]="")},checkCorrectness(){this.correctness=Object.values(this.exams).every(n=>n.length!==0)},save(){localStorage.setItem("abif\xE4cher",JSON.stringify(this.exams))},load(){try{const n=JSON.parse(localStorage.getItem("kurswahl"));if(n==null)throw new Error("Keine Kurswahl");for(const e in n)this.kurswahl[e]=n[e];this.exams.p1=this.kurswahl.lk1,this.exams.p2=this.kurswahl.lk2}catch{this.modalTitle="Fehlende Kurswahl",this.modalContent="Bitte gib zuerst deine Kurse an, bevor du deine Abif\xE4cher w\xE4hlen kannst.",this.showModal=!0}try{const n=JSON.parse(localStorage.getItem("abif\xE4cher"));if(n===void 0)return;for(const e in n)this.exams[e]=n[e]}catch{}}}},it=Object.assign(rt,{__name:"Abif\xE4cher",setup(n){return(e,t)=>(d(),f("div",null,[i("section",null,[Kn,i("div",Mn,[i("div",Fn,[Rn,i("div",null,w(e.exams.p1),1)]),i("div",Bn,[zn,i("div",null,w(e.exams.p2),1)]),i("div",qn,[Jn,i("select",{class:"select",name:"p3-1",value:e.exams.p3,onChange:t[0]||(t[0]=r=>e.setAndRemoveDuplicate("p3",r.target.value))},[k(j,{subjects:e.p3s},null,8,["subjects"])],40,Vn)])])]),i("section",null,[Un,i("div",Wn,[i("div",Qn,[Xn,i("select",{class:"select",name:"p4",value:e.exams.p4,onChange:t[1]||(t[1]=r=>e.setAndRemoveDuplicate("p4",r.target.value))},[k(j,{subjects:e.p4s},null,8,["subjects"])],40,Zn)]),i("div",et,[nt,i("select",{class:"select",name:"p5",value:e.exams.p5,onChange:t[2]||(t[2]=r=>e.setAndRemoveDuplicate("p5",r.target.value))},[k(j,{subjects:e.p5s},null,8,["subjects"])],40,tt)])])]),k(te,{isOpen:e.showModal,onClose:t[3]||(t[3]=r=>e.showModal=!e.showModal),title:e.modalTitle,content:e.modalContent,buttons:[]},null,8,["isOpen","title","content"]),k(ae,{verified:e.correctness},null,8,["verified"])]))}});var se={};(function(n){Object.defineProperty(n,"__esModule",{value:!0}),n.emulateTab=void 0;var e=new Array("offsetHeight","scrollHeight","clientHeight"),t=!0,r=/text|password|search|tel|url/;function u(){var a=m()||document.body,s=G(a);return o(a,s)}n.emulateTab=u,function(a){a.from=b,a.to=function(s,c){return c===void 0&&(c=!1),b(m()).to(s,c)},a.backwards=function(){return a.to($(m()),t)},a.forwards=function(){return a()},a.findSelectableElements=K}(u=n.emulateTab||(n.emulateTab={}));function b(a){return a===void 0&&(a=document.body),{toPreviousElement:function(){return o(a,$(),t)},toNextElement:function(){return o(a,G(a))},to:function(s,c){return c===void 0&&(c=!1),o(a,s,c)}}}function o(a,s,c){c===void 0&&(c=!1);var _=a.dispatchEvent(v("keydown",c));if(_){if(a.blur(),s.focus(),document.activeElement!==s)try{document.activeElement=s}catch{console.warn("could not switch active element")}s instanceof HTMLInputElement&&r.test(s.type)&&(s.selectionStart=0);var y=v("keyup",c);return s.dispatchEvent(y),!0}else return a.dispatchEvent(v("keypress",c)),a.dispatchEvent(v("keyup",c)),!1}function v(a,s){return s===void 0&&(s=!1),new KeyboardEvent(a,{code:"Tab",key:"Tab",cancelable:!0,bubbles:!0,shiftKey:s})}function m(){var a=document.activeElement;return a instanceof HTMLElement?a:void 0}function $(a){a===void 0&&(a=document.body);var s=K();if(s.length<1)throw new Error("no selectable elements found");var c=s.indexOf(a),_=(c>0?c:s.length)-1,y=s[_];return y}function G(a){a===void 0&&(a=document.body);var s=K();if(s.length<1)throw new Error("no selectable elements found");var c=s.indexOf(a),_=c+1<s.length?c+1:0,y=s[_];return y}function K(){var a=Array.from(document.querySelectorAll("*")).filter(oe);q();var s=a.filter(ce(de,M,ue,he,fe)).reduce(function(_,y){var A=y.tabIndex,D=_[""+A]||{tabIndex:A,elements:[]};return D.elements.push(y),_[A]=D,_},{}),c=Object.values(s).sort(be).reduce(function(_,y){return _.concat(y.elements)},new Array);return c}var oe=function(a){return a instanceof HTMLElement},M,q=function(){var a=J();a?M=le(a):M=V,q=function(){}},le=function(a){return function(s){var c=s[a];return!!c&&typeof c=="number"&&c>0}};function J(a){a===void 0&&(a=document.body);var s=a,c=e.find(function(ge){var F=s[ge];return F&&typeof F=="number"&&F>0});if(c)return c;for(var _=a.children,y=0,A=Array.from(_);y<A.length;y++){var D=A[y],U=J(D);if(U)return U}}var V=function(a){if(!a.isConnected)return!1;if(a.tagName==="BODY")return!0;var s=getComputedStyle(a);if(s.display==="none"||s.visibility==="collapse")return!1;var c=a.parentElement;return c?V(c):!1};function ce(){for(var a=[],s=0;s<arguments.length;s++)a[s]=arguments[s];return function(c){return!a.some(function(_){return!_(c)})}}function de(a){return typeof a.tabIndex=="number"&&a.tabIndex>=0}var ue=function(a){return!a.disabled};function he(a){return!(a instanceof HTMLAnchorElement)||!!a.href||a.getAttribute("tabIndex")!==null}var fe=function(a){return getComputedStyle(a).visibility!=="collapse"};function be(a,s){if(a.tabIndex>0&&s.tabIndex>0)return a.tabIndex-s.tabIndex;if(a.tabIndex>0)return-a.tabIndex;if(s.tabIndex>0)return s.tabIndex;throw new Error("same tab index for two groups")}})(se);const at={class:"kurs-row"},st={class:"name"},ot={class:"points"},lt=["value","onKeydown","disabled"],ct=["value","disabled"],dt={props:{kurs:{type:Object,required:!0},numberOfEndedYears:{type:Number,required:!0}},mounted(){},methods:{onKeyDown(n,e){try{if(e.key==="Tab")return;e.preventDefault();let t;if(e.key!=="Delete"&&e.key!=="Backspace"&&(t=parseInt(e.target.value+e.key),t>15||t<0||Number.isNaN(t)))return;n===5?this.kurs.examPoints=t:this.kurs.years[n].points=t,t>1&&se.emulateTab()}catch{}}}},Z=Object.assign(dt,{__name:"Kurs",setup(n){return(e,t)=>(d(),f("div",at,[i("div",st,w(n.kurs.name),1),i("div",ot,[(d(),f(O,null,E(4,r=>i("input",{type:"number",class:N(["input",n.kurs.years[r].einbringen]),min:"0",max:"15",key:r,value:n.kurs.years[r].points,onKeydown:u=>e.onKeyDown(r,u),disabled:r>n.numberOfEndedYears},null,42,lt)),64)),n.kurs.exam!==null?(d(),f("input",{key:0,type:"number",class:"input",min:"0",max:"15",value:n.kurs.examPoints,onKeydown:t[0]||(t[0]=r=>e.onKeyDown(5,r)),disabled:n.numberOfEndedYears<5},null,40,ct)):I("",!0)])]))}});const L=n=>(ee("data-v-5e1dd194"),n=n(),ne(),n),ut={class:"messages"},ht={class:"number-of-ended-years center"},ft=L(()=>i("span",null,"Beendete Halbjahre:",-1)),bt=["value"],gt=["value"],mt=L(()=>i("option",{value:5},"4 + Pr\xFCfung",-1)),vt=L(()=>i("h4",null,"Pr\xFCfungsf\xE4cher",-1)),pt=Y(" In diesen F\xE4chern m\xFCssen alle Halbjahre eingebracht werden. Die Punkte der Leistungskurse z\xE4hlen doppelt."),kt=L(()=>i("br",null,null,-1)),wt={key:0},_t=L(()=>i("h4",null,"Andere Grundkurse",-1)),yt=L(()=>i("div",{class:"legend"},[i("p",null,[i("span",{class:"square lila"}),Y("Pflichteinbringungen")]),i("p",null,[i("span",{class:"square pink"}),Y("Mindestens 1")]),i("p",null,[i("span",{class:"square green"}),Y("Auff\xFCllen")])],-1)),xt=L(()=>i("h4",null,"Schnitt berechnen",-1)),Ot={key:0},St={class:"center"},Et={key:1,class:"schnitt"},jt={class:"flex-n-margin center"},It={class:"schnitt"},$t={data(){return{kurswahl:z(),exams:ie(),einbringen:ze(),messages:[],numberOfEndedYears:1,schnitt:{block1:0,block2:0,note:null,punkte:null},showModal:!1,modalTitle:"",modalContent:""}},computed:{examKurse:function(){const n=[];for(const e in this.einbringen)this.einbringen[e].exam!==null&&n.push(this.einbringen[e]);return n.sort((e,t)=>e-t),n},nonExamKurse:function(){const n=[];for(const e in this.einbringen)this.einbringen[e].exam===null&&n.push(this.einbringen[e]);return n}},mounted(){this.load()},methods:{calculateAbischnitt(){try{this.checkAndFillInputs(),this.makePflichtEinbringungen(),this.makeAuff\u00FCllenEinbringungen(),this.calculateSchnitt(),this.save()}catch(n){this.modalTitle="Problem",this.modalContent=n.message,this.showModal=!0,console.error(n)}},sumYears(n){return Object.values(n).map(e=>e.points).reduce((e,t)=>e+t,0)},checkAndFillInputs(){for(const n in this.einbringen){const e=this.einbringen[n];let t=0;for(let r=1;r<=this.numberOfEndedYears&&r<=4;r++){if(typeof e.years[r].points=="number"){t+=e.years[r].points;continue}throw new Error(`Ups, du scheinst vergessen zu haben beim Fach ${e.name} das ${r}. Halbjahr auszuf\xFCllen`)}t=Math.round(t/this.numberOfEndedYears);for(let r=this.numberOfEndedYears+1;r<=4;r++)e.years[r].points=t;e.exam!==null&&this.numberOfEndedYears!==5&&(e.examPoints=t)}},makePflichtEinbringungen(){const n=t=>{for(const r in t.years)t.years[r].einbringen="pflicht"},e=(t,r,u="pflicht")=>{const b=Object.values(t.years);b.sort((o,v)=>v.points-o.points);for(let o=0;o<r;o++)b[o].einbringen=u};this.einbringen.art_music.exam===null&&e(this.einbringen.art_music,2),this.einbringen.reli_ethic.exam===null&&e(this.einbringen.reli_ethic,2),(()=>{var $,G;if(this.einbringen.lang1!==void 0&&(($=this.einbringen.lang1)==null?void 0:$.exam)!==null||this.einbringen.lang2!==void 0&&((G=this.einbringen.lang2)==null?void 0:G.exam)!==null||l.getSubject(this.kurswahl.lk2).specificField===h.language)return;let o=-1;this.einbringen.lang2&&(o=this.sumYears(this.einbringen.lang2.years));let v=this.sumYears(this.einbringen.lang1.years),m=1;v<o&&(m=2),n(this.einbringen[`lang${m}`])})(),(()=>{l.getSubject(this.kurswahl.lk2).name!=="Geschichte"&&this.einbringen.history.exam===null&&n(this.einbringen.history)})(),(()=>{let o=0;l.getSubject(this.kurswahl.lk2).specificField===h.science&&o++;let v=[];for(let m=1;m<=3&&this.einbringen[`nawi${m}`]!==void 0;m++){if(this.einbringen[`nawi${m}`].exam===null){v.push(this.einbringen[`nawi${m}`]);continue}o++}v=v.sort((m,$)=>this.sumYears($.years)-this.sumYears(m.years));for(let m=0;o<2;m++)n(v[m]),o++})(),(()=>{if(this.einbringen.gewi1.exam!==null||this.einbringen.gewi2.exam!==null)return;let o=this.sumYears(this.einbringen.gewi1.years),v=this.sumYears(this.einbringen.gewi2.years),m=1;o<v&&(m=2),e(this.einbringen[`gewi${m}`],2)})();for(const t in this.einbringen){const r=this.einbringen[t];r.exam===null&&(Object.values(r.years).some(u=>u.einbringen.length>0)||e(r,1,"min1"))}},makeAuff\u00FCllenEinbringungen(){let n=20;const e=[];for(const t in this.einbringen){const r=this.einbringen[t];if(r.exam===null)for(let u=1;u<=4;u++){if(r.years[u].einbringen.length>0){n--;continue}e.push({kursKey:t,year:r.years[u]})}}e.sort((t,r)=>t.year.points-r.year.points).reverse();for(let t=0;t<n;t++)e[t].year.einbringen="auff\xFCllen"},calculateSchnitt(){let n=0;for(const t in this.einbringen){const r=this.einbringen[t];for(const u in r.years)r.years[u].einbringen.length===0&&r.exam===null||(console.log(t,r.years[u].points*(t.startsWith("lk")?2:1)),n+=r.years[u].points*(t.startsWith("lk")?2:1))}this.schnitt.block1=Math.round(n/(40+8)*40),this.schnitt.block2=this.examKurse.reduce((t,r)=>t+r.examPoints*4,0),this.schnitt.punkte=Math.round(n/(40+8)*100)/100;const e=this.schnitt.block1+this.schnitt.block2;this.schnitt.note=(()=>e>=823?"1,0":e>=805?"1,1":e>=787?"1,2":e>=769?"1,3":e>=751?"1,4":e>=733?"1,5":e>=715?"1,6":e>=697?"1,7":e>=679?"1,8":e>=661?"1,9":e>=643?"2,0":e>=625?"2,1":e>=607?"2,2":e>=589?"2,3":e>=571?"2,4":e>=553?"2,5":e>=535?"2,6":e>=517?"2,7":e>=499?"2,8":e>=481?"2,9":e>=463?"3,0":e>=445?"3,1":e>=427?"3,2":e>=409?"3,3":e>=391?"3,4":e>=373?"3,5":e>=355?"3,6":e>=337?"3,7":e>=319?"3,8":e>=301?"3,9":e===300?"4,0":"Nicht bestanden :(")()},save(){const n={};for(const e in this.einbringen){n[e]=[];for(let t=1;t<=this.numberOfEndedYears&&t<=4;t++)n[e].push(this.einbringen[e].years[t].points);this.numberOfEndedYears===5&&n[e].push(this.einbringen[e].examPoints)}localStorage.setItem("einbringen",JSON.stringify(n))},load(){let n="";try{const e=JSON.parse(localStorage.getItem("kurswahl"));if(e==null)throw new Error("Keine Kurswahl");this.kurswahl=e}catch{n="Gib erst deine Kurse an bevor du deine Punkte eintragen kannst. "}try{const e=JSON.parse(localStorage.getItem("abif\xE4cher"));if(e==null)throw new Error("Keine Abif\xE4cher");for(const t in e)this.exams[t]=e[t]}catch{n+="Gib erst deine Abif\xE4cher an bevor du deine Punkte eintragen kannst."}try{const e=JSON.parse(localStorage.getItem("einbringen"));if(e==null)throw new Error;for(let t in e){this.numberOfEndedYears=e[t].length;for(let r=0;r<e[t].length&&r<4;r++)this.einbringen[t].years[r+1].points=e[t][r];e.length===5&&(this.einbringen[t].examPoints=e[t][4])}}catch{}for(const e in this.einbringen){if(!Object.keys(this.kurswahl).includes(e)){delete this.einbringen[e];continue}this.einbringen[e].name=this.kurswahl[e];const t=Object.entries(this.exams).filter(r=>r[1]===this.kurswahl[e])[0];this.einbringen[e].exam=t?parseInt(t[0][1])<4?"write":"oral":null}n.length>0&&(this.modalTitle="Fehlende Daten",this.modalContent=n,this.showModal=!0)}}},At=Object.assign($t,{__name:"Einbringen",setup(n){return(e,t)=>(d(),f("div",null,[i("div",ut,[(d(!0),f(O,null,E(e.messages,r=>(d(),f("div",{class:"message",key:r},w(r),1))),128))]),i("div",ht,[ft,i("select",{class:"select",value:e.numberOfEndedYears,onChange:t[0]||(t[0]=r=>e.numberOfEndedYears=parseInt(r.target.value))},[(d(),f(O,null,E(4,r=>i("option",{key:r,value:r},w(r),9,gt)),64)),mt],40,bt)]),i("section",null,[vt,i("p",null,[pt,kt,e.numberOfEndedYears===5?(d(),f("span",wt,"Trage im f\xFCnften Feld deine Pr\xFCfungsnote ein.")):I("",!0)]),(d(!0),f(O,null,E(e.examKurse,r=>(d(),f("div",{class:"kurs",key:r.name},[k(Z,{kurs:r,numberOfEndedYears:e.numberOfEndedYears},null,8,["kurs","numberOfEndedYears"])]))),128))]),i("section",null,[_t,yt,(d(!0),f(O,null,E(e.nonExamKurse,r=>(d(),f("div",{class:"kurs",key:r.name},[k(Z,{kurs:r,numberOfEndedYears:e.numberOfEndedYears},null,8,["kurs","numberOfEndedYears"])]))),128))]),i("section",null,[xt,e.numberOfEndedYears<5?(d(),f("p",Ot," Noch nicht belegte Halbjahre und Pr\xFCfungen werden mit Hilfe des Durchschnitts der vorherigen Halbjahre ausgerechnet ")):I("",!0),i("div",St,[i("button",{class:"btn-focus",onClick:t[1]||(t[1]=(...r)=>e.calculateAbischnitt&&e.calculateAbischnitt(...r))},"Abischnitt berechnen")]),e.schnitt.note!==null?(d(),f("div",Et,[i("p",null,"Block I: "+w(e.schnitt.block1),1),i("p",null,"Block II: "+w(e.schnitt.block2),1),i("p",null,"Block I + II: "+w(e.schnitt.block1+e.schnitt.block2),1),i("p",null,"Punktzahl: "+w(e.schnitt.punkte),1),i("div",jt,[i("h2",It,"Abinote: "+w(e.schnitt.note),1)])])):I("",!0)]),k(te,{isOpen:e.showModal,onClose:t[2]||(t[2]=r=>e.showModal=!e.showModal),title:e.modalTitle,content:e.modalContent,buttons:[]},null,8,["isOpen","title","content"])]))}});var Lt=H(At,[["__scopeId","data-v-5e1dd194"]]);const Nt={class:"abirechner-container"},Gt={class:"selector"},Pt={inject:["mq"],data(){return{stage:"kurswahl",stages:[new R("Kurswahl","kurswahl"),new R("Abiturf\xE4cher","abif\xE4cher"),new R("Punkte","punkte")]}}},Dt=Object.assign(Pt,{__name:"Index",setup(n){return(e,t)=>(d(),f("div",Nt,[i("div",Gt,[k(we,{onSelect:t[0]||(t[0]=r=>{e.stage=r.key}),default:e.stage,items:e.stages},null,8,["default","items"])]),i("div",{class:N(["main",e.mq.current])},[e.stage==="kurswahl"?(d(),B(Hn,{key:0})):I("",!0),e.stage==="abif\xE4cher"?(d(),B(it,{key:1})):I("",!0),e.stage==="punkte"?(d(),B(Lt,{key:2})):I("",!0)],2)]))}});var Ht=H(Dt,[["__scopeId","data-v-6ae11bf8"]]);export{Ht as default};
