/* Render and verify the public, dependency-free browser simulation. */
const path=require('path'),fs=require('fs'),assert=require('assert');
const {pathToFileURL}=require('url');
const {chromium}=require(process.env.CUTL_PLAYWRIGHT_MODULE || 'C:/Users/TESTPC/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const repo=path.resolve(__dirname,'..');
const output=path.join(repo,'artifacts/validation/readme-menu-demo-v0.2.26');
fs.mkdirSync(output,{recursive:true});
(async()=>{
 const browser=await chromium.launch({headless:true,executablePath:process.env.CUTL_BROWSER_PATH || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'});
 const checks=[];
 for(const lang of ['en','zh-CN'])for(const theme of ['light','dark']){
  const page=await browser.newPage({viewport:{width:880,height:1100},deviceScaleFactor:1.5});
  const errors=[],remote=[];page.on('pageerror',e=>errors.push(e.message));page.on('request',r=>{if(/^https?:/.test(r.url()))remote.push(r.url());});
  const url=pathToFileURL(path.join(repo,'docs/site',lang+'.html')).href+'?theme='+theme;
  await page.goto(url);await page.evaluate(()=>document.fonts.ready);
  assert.equal(await page.locator('.main-menu > .row').count(),15);
  const frames=[],prefix=lang.toLowerCase()+'-'+theme;
  async function capture(name,duration){await page.mouse.move(5,5);await page.evaluate(()=>document.activeElement?.blur());const filename=prefix+'-'+String(frames.length).padStart(2,'0')+'-'+name+'.png';await page.locator('.capture').screenshot({path:path.join(output,filename)});frames.push({file:filename,duration});}
  async function open(name){await page.locator('.main-menu [data-value="'+name+'"]').hover();assert.equal(await page.locator('.branch').isVisible(),true);}
  async function choose(value){await page.locator('#submenu [data-value="'+value+'"]').click();}
  await capture('main',1500);
  await open('source');await capture('source',1800);
  await open('interval');await capture('interval',1700);await choose('5');await capture('five-minutes',1300);
  await open('style');await capture('styles',1700);await choose('side');await capture('side-by-side',1300);
  await open('tools');assert((await page.locator('#submenu').innerText()).includes('v0.2.26'));await capture('tools',2500);
  await open('theme');await capture('theme',1700);
  fs.writeFileSync(path.join(output,prefix+'.json'),JSON.stringify(frames,null,2));
  await page.locator('.main-menu [data-value="tools"]').hover();await page.locator('#submenu [data-value="about"]').click();assert(await page.locator('dialog').isVisible());await page.keyboard.press('Escape');
  await open('source');await choose('Codex CLI');assert.equal(await page.locator('#submenu button').count(),2);assert((await page.locator('.main-menu').innerText()).includes('Codex CLI'));assert((await page.locator('.main-menu').innerText()).includes('12:30 (WebView2)'));await page.locator('[data-action="refresh"]').click();assert((await page.locator('.main-menu').innerText()).includes('12:31 (Codex CLI)'));
  await page.locator('.main-menu [data-value="interval"]').focus();await page.keyboard.press('ArrowRight');assert.equal(await page.evaluate(()=>document.activeElement.closest('.menu').id),'submenu');await page.keyboard.press('ArrowDown');await page.keyboard.press('Enter');assert((await page.locator('.main-menu [data-value="interval"]').innerText()).includes('2'));await page.keyboard.press('Escape');assert.equal(await page.locator('.branch').isVisible(),false);
  const opposite=theme==='light'?'dark':'light';await page.locator('.theme-control[data-value="'+opposite+'"]').click();assert.equal(await page.locator('html').getAttribute('data-theme'),opposite);
  await page.locator('[data-action="reset"]').click();await open('language');assert.equal(await page.locator('#submenu button:disabled').count(),3);await page.locator('#submenu [data-value="'+(lang==='en'?'zh-CN':'en')+'"]').click();assert.equal(await page.locator('html').getAttribute('lang'),lang==='en'?'zh-CN':'en');
  await page.goto(url);
  for(const width of [880,736,390,320]){await page.setViewportSize({width,height:1300});await open('tools');assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false,'overflow '+lang+' '+width);const clipped=await page.locator('.menu').evaluateAll(nodes=>nodes.some(n=>n.scrollWidth>n.clientWidth+2));assert.equal(clipped,false,'menu clipped '+width);}
  await page.screenshot({path:path.join(output,prefix+'-mobile.png'),fullPage:true});
  assert.deepEqual(errors,[]);assert.deepEqual(remote,[]);
  checks.push({locale:lang,theme,frames:frames.length,remoteRequests:remote.length,jsErrors:errors.length,checks:'15 main rows; submenu interaction; version; source-specific visibility; successful-source label; refresh simulation; keyboard; theme; language; 880/736/390/320px layout'});
  await page.close();
 }
 await browser.close();fs.writeFileSync(path.join(output,'browser-validation.json'),JSON.stringify(checks,null,2));console.log(JSON.stringify(checks,null,2));
})().catch(e=>{console.error(e);process.exit(1)});
