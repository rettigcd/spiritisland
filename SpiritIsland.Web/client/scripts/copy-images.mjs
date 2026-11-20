// Copies the token pictures the client draws from the desktop apps' image folder into public/img, which Vite serves and
// copies into wwwroot, making each small enough to draw cheaply. The copies are committed, so the client builds without the
// rest of the repository; run this (npm run copy-images) only to refresh them or to add a picture.
//
// The names are the engine's own: SpiritIsland.Utilities/ImageMgmt/ResourceImages.cs maps each Img to one of these files.
//
// The desktop pictures are up to 667 pixels wide, but a token is drawn about 50 pixels wide (twice that on a high-density
// screen). Drawing big pictures that small makes the browser resample every one of them whenever the board repaints, which
// during a drag is every frame and makes the GPU fall behind. So each picture is shrunk to at most MAX_SIZE pixels.
import { copyFileSync, mkdirSync, readFileSync, readdirSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { PNG } from 'pngjs';

const MAX_SIZE = 128;

const here = dirname(fileURLToPath(import.meta.url));
const source = join(here, '..', '..', '..', 'SpiritIsland.Utilities', 'images');
const target = join(here, '..', 'public', 'img');

const tokens = [
  'city', 'town', 'explorer', 'dahan', 'blight', 'beast', 'wilds', 'disease', 'strife', 'badlands', 'vitality', 'quake',
  'defend1orange', 'isolateorange',
];

/** Shrinks a picture so its longer side is at most MAX_SIZE, averaging the pixels that each new one covers (weighted by their opacity). */
function shrink(png, max = MAX_SIZE) {
  const scale = max / Math.max(png.width, png.height);
  if (scale >= 1) return png;
  const width = Math.max(1, Math.round(png.width * scale));
  const height = Math.max(1, Math.round(png.height * scale));
  const out = new PNG({ width, height });
  for (let y = 0; y < height; y++) {
    const y0 = Math.floor(y / scale), y1 = Math.max(y0 + 1, Math.min(png.height, Math.ceil((y + 1) / scale)));
    for (let x = 0; x < width; x++) {
      const x0 = Math.floor(x / scale), x1 = Math.max(x0 + 1, Math.min(png.width, Math.ceil((x + 1) / scale)));
      let r = 0, g = 0, b = 0, a = 0, n = 0;
      for (let sy = y0; sy < y1; sy++)
        for (let sx = x0; sx < x1; sx++) {
          const i = (sy * png.width + sx) * 4;
          const alpha = png.data[i + 3];
          r += png.data[i] * alpha; g += png.data[i + 1] * alpha; b += png.data[i + 2] * alpha; a += alpha; n++;
        }
      const o = (y * width + x) * 4;
      out.data[o] = a ? Math.round(r / a) : 0;
      out.data[o + 1] = a ? Math.round(g / a) : 0;
      out.data[o + 2] = a ? Math.round(b / a) : 0;
      out.data[o + 3] = Math.round(a / n);
    }
  }
  return out;
}

function copy(folder, file, max = MAX_SIZE, targetFolder = folder, targetFile = file) {
  mkdirSync(join(target, targetFolder), { recursive: true });
  const png = PNG.sync.read(readFileSync(join(source, folder, file)));
  const small = shrink(png, max);
  if (small === png) copyFileSync(join(source, folder, file), join(target, targetFolder, targetFile));
  else writeFileSync(join(target, targetFolder, targetFile), PNG.sync.write(small, { colorType: 6 }));
  console.log(`${folder}/${file}  ${png.width}x${png.height} -> ${small.width}x${small.height}`);
}

for (const name of tokens) copy('tokens', `${name}.png`);
for (const file of readdirSync(join(source, 'incarna'))) if (file.endsWith('.png')) copy('incarna', file);
copy('presence', 'red.png');

// Small icons, drawn in text (an element, a speed, a terrain), so a smaller size does: img/icons/<name>.png
const ICON_SIZE = 64;
for (const el of ['sun', 'moon', 'fire', 'air', 'water', 'plant', 'earth', 'animal'])
  copy('icons/Elements', `${el}.png`, ICON_SIZE, 'icons', `${el}.png`);
copy('icons', 'Fasticon.png', ICON_SIZE, 'icons', 'fast.png');
copy('icons', 'Slowicon.png', ICON_SIZE, 'icons', 'slow.png');
copy('icons', 'Coastal.png', ICON_SIZE, 'icons', 'coastal.png');
copy('tokens', 'coin.png', ICON_SIZE, 'icons', 'coin.png');
copy('tokens', 'CostFast.png', ICON_SIZE, 'icons', 'cost-fast.png');
copy('tokens', 'CostSlow.png', ICON_SIZE, 'icons', 'cost-slow.png');
for (const [name, file] of [['jungle', 'Jungle'], ['mountain', 'Mountain'], ['ocean', 'Ocean'], ['sands', 'Sands'], ['wetland', 'Wetlandland']])
  copy('icons/Terrain', `${file}.png`, ICON_SIZE, 'icons', `${name}.png`);
copy('invaders', 'escalation.png', ICON_SIZE, 'icons', 'escalation.png');

// The presence track slots' pictures (60x80, used as they are): img/tracks/<resource name>.png. The MAUI app names them by the slot's
// code ("2 cardplay" is z2_cardplay.png); see trackImageUrl in src/images.ts.
const trackSource = join(here, '..', '..', '..', 'SpiritIsland.Maui', 'Resources', 'Images', 'presenceTrack');
mkdirSync(join(target, 'tracks'), { recursive: true });
for (const file of readdirSync(trackSource))
  if (file.endsWith('.png')) {
    copyFileSync(join(trackSource, file), join(target, 'tracks', file));
    console.log(`tracks/${file}`);
  }
// The broken presence the MAUI spirit panel shows for presence that has been destroyed: img/icons/destroyed.png
copyFileSync(join(trackSource, '..', 'destroyed_presence.png'), join(target, 'icons', 'destroyed.png'));
console.log('icons/destroyed.png');
// ...and the few the MAUI app keeps in other folders (its image folders are flattened): reclaim, gain 1 energy and the bare elements.
const mauiImages = join(trackSource, '..');
for (const [folder, file] of [['growth', 'reclaim_1z'], ['growth', 'gain_1_energy'], ...['fire', 'moon', 'water', 'earth', 'air'].map(e => ['tokens/elements', e])]) {
  const png = PNG.sync.read(readFileSync(join(mauiImages, folder, `${file}.png`)));
  const small = shrink(png, 80);
  writeFileSync(join(target, 'tracks', `${file}.png`), PNG.sync.write(small, { colorType: 6 }));
  console.log(`tracks/${file}.png  ${png.width}x${png.height} -> ${small.width}x${small.height}`);
}

// EXPERIMENT: the terrain pictures, repeated as the background of each land: img/terrain/<name>.jpg
mkdirSync(join(target, 'terrain'), { recursive: true });
for (const name of ['ocean', 'jungle', 'wetlands', 'sand', 'mountains', 'none']) {
  copyFileSync(join(source, 'terrain', `${name}.jpg`), join(target, 'terrain', `${name}.jpg`));
  console.log(`terrain/${name}.jpg`);
}

// The spirits' round badges (200x200, shrunk): img/badges/<resource name>.png, from the MAUI app's bdg_<resource name>.png
mkdirSync(join(target, 'badges'), { recursive: true });
for (const file of readdirSync(join(trackSource, '..', 'badge')))
  if (file.startsWith('bdg_') && file.endsWith('.png')) {
    const png = PNG.sync.read(readFileSync(join(trackSource, '..', 'badge', file)));
    const small = shrink(png, 128);
    writeFileSync(join(target, 'badges', file.slice('bdg_'.length)), PNG.sync.write(small, { colorType: 6 }));
    console.log(`badges/${file.slice('bdg_'.length)}  ${png.width}x${png.height} -> ${small.width}x${small.height}`);
  }

// The spirits' pictures (JPEGs about 600 pixels wide, used as they are): img/spirits/<name>.jpg
mkdirSync(join(target, 'spirits'), { recursive: true });
for (const file of readdirSync(join(source, 'spirits')))
  if (file.endsWith('.jpg')) {
    copyFileSync(join(source, 'spirits', file), join(target, 'spirits', file));
    console.log(`spirits/${file}`);
  }
