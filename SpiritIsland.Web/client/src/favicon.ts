import { useEffect } from 'preact/hooks';
import { badgeUrl } from './images';

const NONE = 'data:,'; // what index.html starts with: no icon

/** The tab's icon is the spirit's badge while a spirit is chosen or being played, and nothing otherwise. */
export function useFavicon(spirit: string | null | undefined) {
  useEffect(() => {
    const link = document.querySelector<HTMLLinkElement>('link[rel="icon"]');
    if (!link) return;
    link.type = spirit ? 'image/png' : '';
    link.href = spirit ? badgeUrl(spirit) : NONE;
    return () => { link.type = ''; link.href = NONE; };
  }, [spirit]);
}
