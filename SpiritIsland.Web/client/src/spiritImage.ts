/** The picture of a spirit (public/img/spirits, named like the spirit with underscores for spaces). A spirit with no picture just shows none. */
export const spiritImageUrl = (spiritName: string) => `/img/spirits/${spiritName.replace(/ /g, '_')}.jpg`;
