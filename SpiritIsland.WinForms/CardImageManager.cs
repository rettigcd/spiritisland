using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	sealed public class CardImageManager : IDisposable {

		public CardImageManager() {
		}

		public Image GetImage( PowerCard card ) {

			if(!images.ContainsKey( card )) {
				Image image = ResourceImages.GetPowerCard( card ).Result; // !!!
				images.Add( card, image );
			}
			var x = images[card];
			return x;
		}

		public void Dispose() {
			foreach(var image in images.Values)
				image.Dispose();
			images.Clear();
		}

		readonly Dictionary<PowerCard, Image> images = new();
	}

}
