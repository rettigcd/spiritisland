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
function shrink(png) {
  const scale = MAX_SIZE / Math.max(png.width, png.height);
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

function copy(folder, file) {
  mkdirSync(join(target, folder), { recursive: true });
  const png = PNG.sync.read(readFileSync(join(source, folder, file)));
  const small = shrink(png);
  if (small === png) copyFileSync(join(source, folder, file), join(target, folder, file));
  else writeFileSync(join(target, folder, file), PNG.sync.write(small, { colorType: 6 }));
  console.log(`${folder}/${file}  ${png.width}x${png.height} -> ${small.width}x${small.height}`);
}

for (const name of tokens) copy('tokens', `${name}.png`);
for (const file of readdirSync(join(source, 'incarna'))) if (file.endsWith('.png')) copy('incarna', file);
copy('presence', 'red.png');
