using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	sealed public class CardImageManager : IDisposable {

		public CardImageManager() {
		}

		public Image GetImage( PowerCard card ) {

			if(!images.ContainsKey( card )) {
				string filename = card.Name
					.Replace( ' ', '_' )
					.Replace( "'", "" )
					.Replace( "-", "" )
					.ToLower();
				Image image = Image.FromFile( $".\\images\\cards\\{filename}.jpg" );
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

	sealed public class FearCardImageManager : IDisposable {

		public FearCardImageManager() {
		}

		public Image GetImage( string name ) {

			if(!images.ContainsKey( name )) {
				string filename = name.Replace( ' ', '_' ).Replace( "'", "" ).ToLower();
				Image image = Image.FromFile( $".\\images\\fear\\{filename}.jpg" );
				images.Add( name, image );
			}
			return images[name];
		}

		public void Dispose() {
			foreach(var image in images.Values)
				image.Dispose();
			images.Clear();
		}

		readonly Dictionary<string, Image> images = new();
	}

}
