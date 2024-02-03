using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms; 

/// <summary> Caches PowerCard images in memory. </summary>
sealed public class CardImageManager : IDisposable {

	public ResourceMgr<Bitmap> GetMgr( PowerCard card ){
		return new ResourceMgr<Bitmap>(GetImage(card),false); // don't dispose
	}

	public Bitmap GetImage( PowerCard card ) {

		if(!images.TryGetValue( card, out Bitmap image )) {
			image = ResourceImages.Singleton.GetPowerCard( card ).Result; // !!!
			images.Add( card, image );
		}
		return image;
	}

	public void Dispose() {
		foreach(var image in images.Values)
			image.Dispose();
		images.Clear();
	}

	readonly Dictionary<PowerCard, Bitmap> images = [];
}
