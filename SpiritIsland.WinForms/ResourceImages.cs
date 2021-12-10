using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SpiritIsland.WinForms {

	public class ResourceImages {

		static public readonly ResourceImages Singleton = new ResourceImages();

		ResourceImages() {
			assembly = System.Reflection.Assembly.GetExecutingAssembly();
			Fonts = new PrivateFontCollection();
			LoadFont( "leaguegothic-regular-webfont.ttf" );
		}

		#region Fonts

		public readonly PrivateFontCollection Fonts;

		void LoadFont(string file) {
			string resource = "SpiritIsland.WinForms." + file;
			using Stream fontStream = assembly.GetManifestResourceStream( resource );               
			
			// read the fond data into a buffer
			byte[] fontdata = new byte[fontStream.Length];
			fontStream.Read( fontdata, 0, (int)fontStream.Length );

			// copy the bytes to the unsafe memory block
			IntPtr data = Marshal.AllocCoTaskMem( (int)fontStream.Length );
			Marshal.Copy( fontdata, 0, data, (int)fontStream.Length );

			// pass the font to the font collection
			Fonts.AddMemoryFont( data, (int)fontStream.Length );
			// free up the unsafe memory
			Marshal.FreeCoTaskMem( data );
		}

		#endregion

		public Bitmap GetImage( Img img ) {
			return GetResourceImage( ToResource( img ) );
		}

		public Bitmap GetPresenceIcon( string presenceColor ) => GetResourceImage( $"presence.{presenceColor}.png" );

		// Strife / fear / invaders / dahan / blight / beast etc...
		public Bitmap Strife()   => GetResourceImage("tokens.strife.png");
		public Bitmap Fear()     => GetResourceImage("tokens.fear.png");
		public Bitmap FearGray() => GetResourceImage("tokens.fear_gray.png");
		public Bitmap FearCard() => GetResourceImage("tokens.fearcard.png");
		public Bitmap RedX()     => GetResourceImage( "icons.red-x.png" );
		public Bitmap Hourglass()     => GetResourceImage( "icons.hourglass.png" );
		public Bitmap TerrorLevel( int terrorLevel )     => GetResourceImage( $"icons.TerrorLevel{terrorLevel}.png" );

		// Growth / Terror Level / Destoryed Presence

		public Bitmap GetInvaderCard( string filename ) => GetResourceImage( $"invaders.{filename}.jpg" );

		#region private

		Bitmap GetResourceImage( string filename ) {
			var imgStream = assembly.GetManifestResourceStream( "SpiritIsland.WinForms.images."+filename );
			return new Bitmap( imgStream );
		}

		readonly Assembly assembly;

		#endregion

		static string ToResource( Img img ) => img switch {
			Img.Starlight_AssignElement      => "icons.AssignElement.png",
			Img.CardPlay                     => "icons.cardplay.png",
			Img.Reclaim1                     => "icons.reclaim 1.png",
			Img.ReclaimAll                   => "icons.ReclaimAll.png",
			Img.ReclaimHalf                  => "icons.Reclaim_Half.png",
			Img.Plus1CardPlay                => "icons.Cardplayplusone.png",
			Img.Push1dahan                   => "icons.Push1dahan.png",
			Img.GainCard                     => "icons.GainCard.png",
			Img.MovePresence                 => "icons.MovePresence.png",

			Img.Pushfromocean                => "icons.Pushfromocean.png",
			Img.Gathertoocean                => "icons.Gathertoocean.png",
			Img.Damage_2                     => "icons.Damage_2.png",
			Img.Oneenergyfire                => "icons.Oneenergyfire.png",
			Img.Land_Gather_Beasts           => "icons.Land_Gather_Beasts.png",
			Img.GainEnergyEqualToCardPlays   => "icons.GainEnergyEqualToCardPlays.png",

			Img.RangeArrow                   => "icons.Range.png",
			Img.MoveArrow                    => "icons.Moveicon.png",

			Img.Stone_Minor                  => "icons.minor.png",
			Img.ShiftingMemory_PrepareEl     => "icons.PrepareElement.png",
			Img.ShiftingMemory_Discard2Prep  => "icons.DiscardElementsForCardPlay.png",
			Img.Starlight_GrowthOption1      => "icons.so1.png",
			Img.Starlight_GrowthOption2      => "icons.so2.png",
			Img.Starlight_GrowthOption3      => "icons.so3.png",
			Img.Starlight_GrowthOption4      => "icons.so4.png",
			Img.FracturedDays_Gain2Time      => "icons.Gain2time.png",
			Img.FracturedDays_Gain1Timex2    => "icons.Gain1timex2.png",
			Img.FracturedDays_Gain1Timex3    => "icons.Gain1timex3.png",
			Img.FracturedDays_DrawDtnw       => "icons.Daysthatneverweregrowthicon.png",


			Img.Coin                         => "tokens.coin.png",

			Img.Token_Sun                    => "tokens.Simple_sun.png",
			Img.Token_Moon                   => "tokens.Simple_moon.png",
			Img.Token_Fire                   => "tokens.Simple_fire.png",
			Img.Token_Air                    => "tokens.Simple_air.png",
			Img.Token_Water                  => "tokens.Simple_water.png",
			Img.Token_Plant                  => "tokens.Simple_plant.png",
			Img.Token_Earth                  => "tokens.Simple_earth.png",
			Img.Token_Animal                 => "tokens.Simple_animal.png",
			Img.Token_Any                    => "tokens.Simple_any.png",

			Img.City3                        => "tokens.city.png",
			Img.City2                        => "tokens.city2.png",
			Img.City1                        => "tokens.city1.png",
			Img.Town2                        => "tokens.town.png",
			Img.Town1                        => "tokens.town1.png",
			Img.Explorer                     => "tokens.explorer.png",
			Img.Dahan2                       => "tokens.dahan.png",
			Img.Dahan1                       => "tokens.dahan1.png",
			Img.Defend                       => "tokens.defend1orange.png",
			Img.Blight                       => "tokens.blight.png",
			Img.Beast                        => "tokens.beast.png",
			Img.Wilds                        => "tokens.wilds.png",
			Img.Disease                      => "tokens.disease.png",
			Img.Badlands                     => "tokens.badlands.png",

			Img.Icon_Sun                    => "icons.Simple_sun.png",
			Img.Icon_Moon                   => "icons.Simple_moon.png",
			Img.Icon_Fire                   => "icons.Simple_fire.png",
			Img.Icon_Air                    => "icons.Simple_air.png",
			Img.Icon_Water                  => "icons.Simple_water.png",
			Img.Icon_Plant                  => "icons.Simple_plant.png",
			Img.Icon_Earth                  => "icons.Simple_earth.png",
			Img.Icon_Animal                 => "icons.Simple_animal.png",

			Img.Icon_Dahan                   => "icons.Dahanicon.png",
			Img.Icon_JungleOrWetland         => "icons.Junglewetland.png",
			Img.Icon_DahanOrInvaders         => "icons.DahanOrInvaders.png",
			Img.Icon_Coastal                 => "icons.Coastal.png",
			Img.Icon_PresenceOrWilds         => "icons.wildsorpresence.png",
			Img.Icon_NoBlight                => "icons.Noblight.png",
			Img.Icon_BeastOrJungle           => "icons.JungleOrBeast.png",
			Img.Icon_Ocean                   => "icons.Ocean.png",
			Img.Icon_MountainOrPresence      => "icons.mountainorpresence.png",
			Img.Icon_TownCityOrBlight        => "icons.TownCityOrBlight.png",
			Img.Icon_Blight                  => "icons.Blighticon.png",
			Img.Icon_Beast                   => "icons.Beasticon.png",
			Img.Icon_Fear                    => "icons.Fearicon.png",
			Img.Icon_Wilds                   => "icons.Wildsicon.png",
			Img.Icon_Fast                    => "icons.Fasticon.png",
			Img.Icon_Presence                => "icons.Presenceicon.png",
			Img.Icon_Slow                    => "icons.Slowicon.png",
			Img.Icon_Disease                 => "icons.Diseaseicon.png",
			Img.Icon_Strife                  => "icons.Strifeicon.png",
			Img.Icon_Badlands                => "icons.Badlands.png",
			Img.Icon_City                    => "icons.Cityicon.png",
			Img.Icon_Town                    => "icons.Townicon.png",
			Img.Icon_Explorer                => "icons.Explorericon.png",

			Img.Deck_Hand                    => "hand.png",
			Img.Deck_Played                  => "inplay.png",
			Img.Deck_Discarded               => "discard.png",
			Img.Deck_DaysThatNeverWere_Major => "major_inverted.png",
			Img.Deck_DaysThatNeverWere_Minor => "minor_inverted.png",

			Img.None                         => null,
			_                                => throw new System.ArgumentOutOfRangeException(nameof(img), img.ToString() ),
		};

	}

}
