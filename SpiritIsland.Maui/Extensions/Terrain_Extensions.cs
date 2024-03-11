//using Android.OS;
namespace SpiritIsland.Maui;

static class Terrain_Extensions {
	static public Color GetColor( this Terrain terrain ) {
		return terrain switch {
			Terrain.Ocean => Colors.DarkBlue,
			Terrain.Mountain => Colors.DarkGray,
			Terrain.Wetland => Colors.LightBlue,
			Terrain.Sands => Colors.Goldenrod,
			Terrain.Jungle => Colors.DarkGreen,
			Terrain.None => Colors.SlateGray,
			_ => Colors.Pink, // error..
		};
	}
	static public Img ToIcon( this Terrain terrain ) {
		return terrain switch {
			Terrain.Ocean    => Img.Icon_Ocean,
			Terrain.Mountain => Img.Icon_Mountain,
			Terrain.Wetland  => Img.Icon_Wetland,
			Terrain.Sands    => Img.Icon_Sands,
			Terrain.Jungle   => Img.Icon_Jungle,
			_                => Img.None,
		};
	}
}