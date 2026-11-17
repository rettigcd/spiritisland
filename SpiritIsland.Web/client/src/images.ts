import presenceSpirits from './presenceImages.json';
// Which picture to draw for a token. The server sends the engine's own image name (TokenView.image); these are the pictures
// copied into public/img (see scripts/copy-images.mjs). A token with no picture here is drawn as a labelled circle instead.

const files: Record<string, string> = {
  City: 'tokens/city.png',
  Town: 'tokens/town.png',
  Explorer: 'tokens/explorer.png',
  Dahan: 'tokens/dahan.png',
  Dahan_Solid: 'tokens/dahan.png', // Dahan made unchangeable by Stubborn Solidity
  Blight: 'tokens/blight.png',
  Beast: 'tokens/beast.png',
  Wilds: 'tokens/wilds.png',
  Disease: 'tokens/disease.png',
  Strife: 'tokens/strife.png',
  Badlands: 'tokens/badlands.png',
  Vitality: 'tokens/vitality.png',
  Quake: 'tokens/quake.png',
  Defend: 'tokens/defend1orange.png',
  Isolate: 'tokens/isolateorange.png',
};

// Incarna pictures are named by spirit, with a "+" for the empowered side. The engine's names are "<Spirit>_Incarna" and
// "<Spirit>_Incarna_Empowered"; Thunderspeaker's picture is called "T" rather than "Ts".
const incarnaFile: Record<string, string> = { Ts: 'T' };
const incarna = /^(.+)_Incarna(_Empowered)?$/;

export const STRIFE_IMAGE = '/img/tokens/strife.png';
export const PRESENCE_IMAGE = '/img/presence/red.png';

export const isPresence = (image: string) => image.startsWith('Presence:');

/**
 * The spirits that have a presence picture of their own, drawn like the desktop apps do (the spirit's artwork laid over the
 * presence shape; see SpiritIsland.Web.PresenceImages, which writes the pictures and this list). The others get the red one, turned.
 */
const ownPresence = new Set<string>(presenceSpirits);

/** The spirit's own presence picture, or null if it has none. */
export function presenceImageUrl(image: string): string | null {
  const spirit = image.slice('Presence:'.length);
  return ownPresence.has(spirit) ? `/img/presence/${spirit.replace(/ /g, '_')}.png` : null;
}

/** The picture for a token, or null when there is none (draw a circle). A spirit's presence is not here: see {@link presenceHue}. */
export function tokenImageUrl(image: string): string | null {
  const file = files[image];
  if (file) return `/img/${file}`;
  const match = incarna.exec(image);
  if (match) return `/img/incarna/${incarnaFile[match[1]!] ?? match[1]}${match[2] ? '+' : ''}.png`;
  return null;
}

/**
 * Presence is one red picture for every spirit, turned to a colour of its own so the spirits can be told apart. The hue is
 * worked out from the spirit's name, so a spirit has the same colour everywhere and in every game.
 */
export function presenceHue(image: string): number {
  let h = 0;
  for (const ch of image.slice('Presence:'.length)) h = (h * 31 + ch.charCodeAt(0)) >>> 0;
  return h % 360;
}
