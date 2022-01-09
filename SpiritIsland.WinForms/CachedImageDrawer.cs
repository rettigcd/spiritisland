using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	/// <summary>
	/// Pulls images from ResoueceImages/Files, and Caches them
	/// </summary>
	class CachedImageDrawer : IDisposable {

		readonly Dictionary<Img,Image> images = new Dictionary<Img,Image>();

		public CachedImageDrawer() {}

		public void Draw(Graphics graphics, Img img, Rectangle rect) {
			graphics.DrawImage( GetImage(img), rect );
		}

		public void DrawFitHeight( Graphics graphics, Img img, Rectangle rect ) {
			graphics.DrawImageFitHeight(  GetImage( img ), rect );
		}

		public Image GetImage( Img img ) {	 // !!! make this private
			if( images.ContainsKey( img ) )
				return images[ img ];
			var image = ResourceImages.Singleton.GetImage( img );
			images.Add( img, image );
			return image;
		}

		public void Dispose() {
			foreach(var img in images.Values)
				img.Dispose();
			images.Clear();
		}
	}

}
