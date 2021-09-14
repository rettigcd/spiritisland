using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	sealed public class CardImageManager : IDisposable {

		public CardImageManager() {
		}

		public Image GetImage( PowerCard card ) {

			string ns = card.MethodType.Namespace;
			string edition = ns.Contains("Basegame") ? "basegame"
				:ns.Contains("BranchAndClaw") ? "bac"
				:ns;

			string cardType = card.PowerType switch {
				PowerType.Minor => "minor",
				PowerType.Major => "major",
				PowerType.Spirit => "spirit",
				_ => throw new Exception()
			};

			if(!images.ContainsKey( card )) {
				string filename = card.Name
					.Replace( ' ', '_' )
					.Replace( "'", "" )
					.Replace( "-", "" )
					.ToLower();
				Image image = Image.FromFile( $".\\images\\{edition}\\{cardType}\\{filename}.jpg" );
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
