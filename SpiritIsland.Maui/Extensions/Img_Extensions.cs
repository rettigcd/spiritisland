using System.Text;

namespace SpiritIsland.Maui;

static public class Img_Extensions {

	public static ImageSource ImgSource( this Img img ) {

		string filename = img switch {
			Img.Dahan    => "dahan.png",
			Img.Town     => "town.png",
			Img.City     => "city.png",
			Img.Explorer => "explorer.png",
			Img.Blight   => "blight.png",
			Img.Disease  => "disease.png",
			Img.Beast    => "beast.png",
			Img.Vitality => "vitality.png",
			Img.Deeps    => "deeps.png",
			Img.Badlands => "badlands.png",
			Img.Wilds    => "wilds.png",
			Img.Defend   => "defend.png",
			Img.Isolate  => "isolate.png",

			// Elements
			Img.Icon_Air     => "air.png",
			Img.Icon_Animal  => "animal.png",
			Img.Icon_Earth   => "earth.png",
			Img.Icon_Fire    => "fire.png",
			Img.Icon_Moon    => "moon.png",
			Img.Icon_Plant   => "plant.png",
			Img.Icon_Sun     => "sun.png",
			Img.Icon_Water   => "water.png",

			Img.Token_Air    => "air_token.png",
			Img.Token_Animal => "animal_token.png",
			Img.Token_Earth  => "earth_token.png",
			Img.Token_Fire   => "fire_token.png",
			Img.Token_Moon   => "moon_token.png",
			Img.Token_Plant  => "plant_token.png",
			Img.Token_Sun    => "sun_token.png",
			Img.Token_Water  => "water_token.png",

			// Terrain
			Img.Icon_Ocean   => "ter_ocean.png",
			Img.Icon_Mountain=> "ter_mountain.png",
			Img.Icon_Wetland => "ter_wetland.png",
			Img.Icon_Sands   => "ter_sands.png",
			Img.Icon_Jungle  => "ter_jungle.png",

			Img.Token_Any => "any_token.png",
			_ => "vitality.png" // ERROR - see if this ever happens.
		};
		return ImageCache.FromFile( filename );
	}

}
