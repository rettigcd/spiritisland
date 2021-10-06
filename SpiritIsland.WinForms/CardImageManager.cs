using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	sealed public class CardImageManager : IDisposable {

		public CardImageManager() {
		}

		public Image GetImage( PowerCard card ) {

			if(!images.ContainsKey( card )) {
				string filename = card.GetImageFilename();
				Image image = Image.FromFile( filename );
				images.Add( card, image );
			}
			return images[card];
		}

		public void Dispose() {
			foreach(var image in images.Values)
				image.Dispose();
			images.Clear();
		}

		readonly Dictionary<PowerCard, Image> images = new();
	}

}
