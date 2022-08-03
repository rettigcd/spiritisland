using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	sealed public class FearCardImageManager : IDisposable {

		public FearCardImageManager() {
		}

		public Image GetImage( ActivatedFearCard card ) {

			string name = card.Name;

			if(!images.ContainsKey( name )) {
				string ns = card.FearOptions.GetType().Namespace;
				string edition = ns switch {
					"SpiritIsland.Basegame" => "basegame",
					"SpiritIsland.BranchAndClaw" => "bac",
					"SpiritIsland.JaggedEarth" => "je",
					_ => throw new ArgumentException($"Namespace {ns} not mapped to a fear folder")
				};
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
