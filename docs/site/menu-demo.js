(() => {
  'use strict';
  const root=document.getElementById('demo');
  const locale=document.documentElement.lang==='zh-CN'?'zh-CN':'en';
  const zh=locale==='zh-CN';
  const labels=window.CUTL_MENU_LABELS[locale];
  const version=window.CUTL_MENU_VERSION;
  const text={title:zh?'试一试托盘菜单':'Explore the tray menu',intro:zh?'点击或悬停展开菜单，试着切换用量来源、刷新间隔与外观。所有数据均为示例。':'Open a submenu and try changing the usage source, refresh interval, or appearance. All values are sample data.',theme:zh?'演示配色':'Demo appearance',reset:zh?'恢复示例':'Reset demo',note:zh?'浏览器交互演示 · 示例数据 · 不连接账户，不修改电脑设置':'Browser simulation · Sample data · No account connection or system changes',keyboard:zh?'支持点击、悬停和方向键；Esc 关闭子菜单。此页演示英文与简体中文，桌面程序支持五种语言。':'Click, hover, or use arrow keys; Esc closes a submenu. This demo supports English and Simplified Chinese; the desktop app supports five languages.',idle:zh?'选择任意菜单项查看效果。':'Choose a menu item to explore.',action:zh?'这是菜单演示，未执行实际操作。':'This is a menu demo; no real action was performed.',refreshed:zh?'示例已刷新；没有读取真实账户数据。':'Sample refreshed; no real account data was read.',changed:zh?'仅更新本页演示设置。':'Changed only this demo.',close:zh?'关闭':'Close',about:zh?'这是 v'+version+' 的菜单交互演示。实际程序为 Windows 本地托盘工具。':'This is an interactive menu demo for v'+version+'. The real application is a native Windows tray utility.',update:zh?'实际程序会在默认浏览器中打开最新 Release。':'The desktop app opens the latest Release in your default browser.',release:zh?'查看最新 Release':'View latest Release',simulation:zh?'菜单演示':'Menu demo',switchLang:zh?'English':'简体中文'};
  const urlTheme=new URLSearchParams(location.search).get('theme');
  const systemDark=matchMedia('(prefers-color-scheme: dark)');
  const initialTheme=['light','dark'].includes(urlTheme)?urlTheme:'system';
  const defaults={source:'WebView2',interval:15,style:'both',theme:initialTheme,language:locale,startup:false,lastSource:'WebView2',updated:'12:30'};
  let state={...defaults},opened=null;
  const esc=v=>String(v).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const t=k=>labels[k];
  const sep='<div class="separator" role="separator"></div>';
  const themeName=v=>t({system:'ThemeFollowWindows',light:'ThemeLight',dark:'ThemeDark'}[v]);
  const styleName=v=>t({five:'IconStyleFiveHourOnly',weekly:'IconStyleWeeklyOnly',both:'IconStyleBoth',side:'IconStyleSideBySide'}[v]);
  const minutes=v=>(v===1?t('MinutesSingularFormat'):t('MinutesPluralFormat')).replace('{0}',v);
  const names={source:t('UsageSourceMenu'),interval:t('RefreshInterval'),style:t('IconStyleMenu'),theme:t('Theme'),language:t('LanguageMenu'),tools:t('ToolsAndHelp')};
  const summary=k=>({source:state.source,interval:minutes(state.interval),style:styleName(state.style),theme:themeName(state.theme),language:zh?'简体中文':'English'})[k]||'';
  function row(label,action,value='',opts={}){
    const {checked=false,disabled=false,hint='',radio=false}=opts;
    return `<button type="button" class="row ${action==='open'&&opened===value?'open':''}" data-action="${action}" data-value="${esc(value)}" ${disabled?'disabled':''} ${action==='open'?`aria-expanded="${opened===value}" aria-controls="submenu"`:''} ${radio?`aria-pressed="${checked}"`:''}><span class="mark" aria-hidden="true">${checked?'✓':''}</span><span class="text">${esc(label)}</span>${hint?`<span class="summary">${esc(hint)}</span>`:''}${action==='open'?'<span class="arrow" aria-hidden="true">›</span>':''}</button>`;
  }
  root.innerHTML=`<header class="page-header"><span class="brand">Codex Usage Tray Lite</span><nav><a id="other-language" href="${zh?'en.html':'zh-CN.html'}">${text.switchLang}</a><a href="https://github.com/zjwww/Codex-Usage-Tray-Lite${zh?'/blob/main/README.zh-CN.md':''}">GitHub</a></nav></header><h1 class="page-title">${text.title}</h1><p class="intro">${text.intro}</p><div class="theme-controls"><span class="label">${text.theme}</span>${['system','light','dark'].map(v=>`<button type="button" class="theme-control" data-action="appearance" data-value="${v}">${esc(themeName(v))}</button>`).join('')}<button type="button" class="reset" data-action="reset">${text.reset}</button></div><section class="capture" aria-label="${text.simulation}"><div class="capture-heading"><span>Codex Usage Tray Lite</span><span class="meta">v${version} · ${zh?'简体中文':'English'} · <span id="theme-label"></span></span></div><div class="stage"><div class="menu main-menu" aria-label="${text.simulation}"></div><div class="branch" hidden><div id="submenu" class="menu"></div></div></div><p class="capture-note">${text.note}</p></section><p class="feedback" aria-live="polite">${text.idle}</p><footer class="footer">${text.keyboard}</footer><dialog aria-labelledby="dialog-title"><h2 id="dialog-title"></h2><p id="dialog-text"></p><p id="dialog-extra"></p><form method="dialog"><button>${text.close}</button></form></dialog>`;
  const main=root.querySelector('.main-menu'),branch=root.querySelector('.branch'),submenu=root.querySelector('#submenu');
  function applyTheme(){const dark=state.theme==='dark'||state.theme==='system'&&systemDark.matches;document.documentElement.dataset.theme=dark?'dark':'light';root.querySelector('#theme-label').textContent=t(dark?'ThemeDark':'ThemeLight');root.querySelectorAll('.theme-control').forEach(b=>b.setAttribute('aria-pressed',String(b.dataset.value===state.theme)));}
  function align(){if(!opened)return;const button=main.querySelector(`[data-value="${opened}"]`);const y=button.getBoundingClientRect().top-main.getBoundingClientRect().top;branch.style.paddingTop=Math.max(0,Math.min(y,main.offsetHeight-submenu.offsetHeight))+'px';}
  function render(){
    applyTheme();
    main.innerHTML=`<div class="row status account"><span class="text">demo@example.com</span><span class="plan">Plus</span></div><div class="row status">5-Hour 71% (15:30)</div><div class="row status">Weekly 81% (12:30 9/29)</div><div class="row status secondary">${esc(t('UsageResetsCountFormat').replace('{0}','2'))}</div><div class="row status secondary">${esc(t('LastUpdatedFormat').replace('{0}',state.updated).replace('{1}',state.lastSource))}</div>${sep}${row(t('RefreshNow'),'refresh')}${row(t(state.source==='WebView2'?'OpenLogin':'CodexCliLoginHelp'),'action','login')}${sep}${['source','interval'].map(k=>row(names[k],'open',k,{hint:summary(k)})).join('')}${sep}${['style','theme','language'].map(k=>row(names[k],'open',k,{hint:summary(k)})).join('')}${row(t('StartAtLogin'),'startup','',{checked:state.startup,radio:true})}${sep}${row(names.tools,'open','tools')}${sep}${row(t('Exit'),'action','exit')}`;
    branch.hidden=!opened;
    if(!opened){submenu.innerHTML='';return;}
    submenu.setAttribute('aria-label',names[opened]);
    const option=(label,v,disabled=false)=>row(label,'choose',v,{checked:String(state[opened])===String(v),radio:true,disabled});
    if(opened==='source')submenu.innerHTML=['WebView2','Codex CLI'].map(v=>option(v,v)).join('')+(state.source==='WebView2'?sep+row(t('ProxySettings'),'action','proxy')+sep+row(t('ClearSession'),'action','clear'):'');
    if(opened==='interval')submenu.innerHTML=[1,2,5,10,15,30,60].map(v=>option(minutes(v),v)).join('');
    if(opened==='style')submenu.innerHTML=['five','weekly','both','side'].map(v=>option(styleName(v),v)).join('');
    if(opened==='theme')submenu.innerHTML=['system','light','dark'].map(v=>option(themeName(v),v)).join('');
    if(opened==='language')submenu.innerHTML=[['English','en'],['简体中文','zh-CN'],['繁體中文','zh-TW'],['日本語','ja'],['한국어','ko']].map(([label,v])=>option(label,v,!['en','zh-CN'].includes(v))).join('');
    if(opened==='tools')submenu.innerHTML=row(t('OpenLogs'),'action','logs')+row(t('OpenConfig'),'action','config')+sep+row(t('Help'),'action','help')+row(t('Update'),'action','update')+row(t('About'),'action','about',{hint:'v'+version});
    align();
  }
  function announce(s){root.querySelector('.feedback').textContent=s;}
  function showDialog(action,label){const d=root.querySelector('dialog');root.querySelector('#dialog-title').textContent=label;root.querySelector('#dialog-text').textContent=action==='about'?text.about:action==='update'?text.update:text.action;root.querySelector('#dialog-extra').innerHTML=action==='update'?`<a href="https://github.com/zjwww/Codex-Usage-Tray-Lite/releases/latest" target="_blank" rel="noopener noreferrer">${text.release}</a>`:'';d.showModal();}
  root.addEventListener('click',e=>{const b=e.target.closest('button[data-action]');if(!b||b.disabled)return;const {action,value}=b.dataset;let focusQuery='';
    if(action==='open'){opened=value;focusQuery=`.main-menu [data-value="${value}"]`;}
    else if(action==='choose'){if(opened==='language'&&value!==locale){location.href=(value==='en'?'en.html':'zh-CN.html')+'?theme='+state.theme;return;}state[opened]=opened==='interval'?Number(value):value;focusQuery=`#submenu [data-value="${value}"]`;announce(text.changed);}
    else if(action==='appearance'){state.theme=value;focusQuery=`.theme-control[data-value="${value}"]`;}
    else if(action==='startup'){state.startup=!state.startup;focusQuery='[data-action="startup"]';announce(text.changed);}
    else if(action==='refresh'){state.updated='12:31';state.lastSource=state.source;announce(text.refreshed);focusQuery='[data-action="refresh"]';}
    else if(action==='reset'){state={...defaults};opened=null;announce(text.idle);focusQuery='[data-action="reset"]';}
    else {showDialog(value,b.querySelector('.text').textContent);return;}
    render();if(focusQuery)root.querySelector(focusQuery)?.focus({preventScroll:true});
  });
  root.addEventListener('pointerover',e=>{if(e.pointerType!=='mouse')return;const b=e.target.closest('.main-menu [data-action="open"]');if(b&&opened!==b.dataset.value){opened=b.dataset.value;render();}});
  root.addEventListener('keydown',e=>{if(root.querySelector('dialog').open)return;const b=e.target.closest('.menu button');if(!b)return;const container=b.closest('.menu');const buttons=[...container.querySelectorAll('button:not(:disabled)')];let next=null;
    if(['ArrowDown','ArrowUp','Home','End'].includes(e.key)){const i=buttons.indexOf(b);next=e.key==='Home'?buttons[0]:e.key==='End'?buttons.at(-1):buttons[(i+(e.key==='ArrowDown'?1:-1)+buttons.length)%buttons.length];}
    else if(e.key==='ArrowRight'&&b.dataset.action==='open'){opened=b.dataset.value;render();next=submenu.querySelector('button:not(:disabled)');}
    else if((e.key==='ArrowLeft'||e.key==='Escape')&&opened){const old=opened;opened=null;render();next=main.querySelector(`[data-value="${old}"]`);}
    else return;e.preventDefault();next?.focus();
  });
  systemDark.addEventListener('change',applyTheme);window.addEventListener('resize',align);render();
})();
