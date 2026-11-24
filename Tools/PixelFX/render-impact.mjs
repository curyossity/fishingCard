// Free, local authoring pipeline. Run from the repository root with Node 22.
import { mkdirSync, writeFileSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { renderFrames, contactSheet, spriteSheet, save } from '../../.local-tools/pixelfx/tools/lib/fxlib.mjs';
import * as mod from './card-impact.mjs';
const root = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const preview = resolve(root, process.argv[2] ?? 'Docs/Validation/WaterImpact/Art');
mkdirSync(preview, {recursive:true});
for (const [tier,power] of [['Small',.2],['Medium',.58],['Large',1]]) {
  const fx = {id:'deepwater/card-impact', mod, meta:mod.meta};
  const rendered = renderFrames(fx, {actor:false, options:{power}, seed:19});
  save(resolve(preview,tier+'.png'),contactSheet(fx,rendered,{max:12,zoom:2}));
  const sheet = spriteSheet(fx,rendered,tier);
  const dest = resolve(root,'Assets/FishingUIAssets/Effects/WaterImpact');
  save(resolve(dest,tier+'.png'),sheet.png);
  writeFileSync(resolve(preview,tier+'.json'),JSON.stringify(sheet.json,null,2));
  // Stable metadata is consumed by the Unity authoring tool; frame rectangles are not hand-transcribed.
  writeFileSync(resolve(root,'Tools/PixelFX/'+tier+'.json'),JSON.stringify(sheet.json,null,2));
  console.log(tier,rendered.n+' frames',sheet.json.meta.size);
  const checkFile=resolve(root,'.local-tools/pixelfx/src/fx/deepwater/card-impact-'+tier.toLowerCase()+'.js');
  mkdirSync(dirname(checkFile),{recursive:true});
  writeFileSync(checkFile,"import {meta as base,draw} from '../../../../../Tools/PixelFX/card-impact.mjs'; export {draw}; export const meta={...base,options:{...base.options,power:"+power+"}};\n");
}
// Expose the maintained module to PixelFX's existing checker without copying the recipe into its ignored checkout.
const wrapper=resolve(root,'.local-tools/pixelfx/src/fx/deepwater/card-impact.js');
mkdirSync(dirname(wrapper),{recursive:true});
writeFileSync(wrapper,"export * from '../../../../../Tools/PixelFX/card-impact.mjs';\n");
