using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	sealed public class FearCardImageManager : IDisposable {

		public FearCardImageManager() {
		}

		public Image GetImage( DisplayFearCard card ) {

			string name = card.Name;

			string ns = card.FearOptions.GetType().Namespace;

			if(!images.ContainsKey( name )) {
				string edition = ns.Contains( "Basegame" ) ? "basegame" : ns.Contains( "BranchAndClaw" ) ? "bac" : ns;
				string name2 = GetFileName( name, edition );
				Image image = Image.FromFile( name2 );
				images.Add( name, image );
			}
			return images[name];
		}

		private static string GetFileName( string name, string edition ) {
			string filename = name.Replace( ' ', '_' ).Replace( "'", "" ).ToLower();
			string name2 = $".\\images\\{edition}\\fear\\{filename}.jpg";
			return name2;
		}

		public void Dispose() {
			foreach(var image in images.Values)
				image.Dispose();
			images.Clear();
		}

		readonly Dictionary<string, Image> images = new();
	}

}
