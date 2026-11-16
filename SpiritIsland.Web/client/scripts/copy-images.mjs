// Copies the token pictures the client draws from the desktop apps' image folder into public/img, which Vite serves and
// copies into wwwroot. The copies are committed, so the client builds without the rest of the repository; run this
// (npm run copy-images) only to refresh them or to add a picture.
//
// The names are the engine's own: SpiritIsland.Utilities/ImageMgmt/ResourceImages.cs maps each Img to one of these files.
import { copyFileSync, mkdirSync, readdirSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const source = join(here, '..', '..', '..', 'SpiritIsland.Utilities', 'images');
const target = join(here, '..', 'public', 'img');

const tokens = [
  'city', 'town', 'explorer', 'dahan', 'blight', 'beast', 'wilds', 'disease', 'strife', 'badlands', 'vitality', 'quake',
  'defend1orange', 'isolateorange',
];

function copy(folder, file) {
  mkdirSync(join(target, folder), { recursive: true });
  copyFileSync(join(source, folder, file), join(target, folder, file));
  console.log(`${folder}/${file}`);
}

for (const name of tokens) copy('tokens', `${name}.png`);
for (const file of readdirSync(join(source, 'incarna'))) if (file.endsWith('.png')) copy('incarna', file);
copy('presence', 'red.png');
