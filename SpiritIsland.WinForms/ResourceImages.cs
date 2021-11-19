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

		public readonly PrivateFontCollection Fonts;

		public Bitmap GetPresenceIcon( string presenceColor ) => GetResourceImage( $"presence.{presenceColor}.png" );

		#region Tokens

		
		public Bitmap GetToken( string fileName )       => GetResourceImage( $"tokens.{fileName}.png" );
		public Bitmap GetToken( Element element )       => GetResourceImage( $"tokens.Simple_{element.ToString().ToLower()}.png" );

		#endregion Tokens

		public Bitmap GetTargetFilterIcon( string filterEnum ) {
			string iconFilename = filterEnum switch {
				Target.Dahan => "Dahanicon",
				Target.JungleOrWetland => "Junglewetland",
				Target.DahanOrInvaders => "DahanOrInvaders",
				Target.Coastal => "Coastal",
				Target.PresenceOrWilds => "wildsorpresence",
				Target.NoBlight => "Noblight",
				Target.BeastOrJungle => "JungleOrBeast",
				Target.Ocean => "Ocean",
				Target.MountainOrPresence => "mountainorpresence",
				_ => null, // Inland, Any
			};
			return iconFilename != null ? GetIcon( iconFilename ) : null;
		}


		public Bitmap GetIcon( string fileName )         => GetResourceImage( $"icons.{fileName}.png" );

		public Image LoadIconBySimpleName( string token ) {
			string filename = token switch {
				"dahan"    => "Dahanicon",
				"city"     => "Cityicon",
				"town"     => "Townicon",
				"explorer" => "Explorericon",
				"blight"   => "Blighticon",
				"beast"    => "Beasticon",
				"fear"     => "Fearicon",
				"wilds"    => "Wildsicon",
				"fast"     => "Fasticon",
				"presence" => "Presenceicon",
				"slow"     => "Slowicon",
				"disease"  => "Diseaseicon",
				"strife"   => "Strifeicon",
				"badlands" => "Badlands",
				_          => "Simple_" + token // elements
			};
			return GetIcon(filename);
		}

		public Bitmap GetInvaderCard( string filename ) => GetResourceImage( $"invaders.{filename}.jpg" );

		#region private

		Bitmap GetResourceImage( string filename ) {
			var imgStream = assembly.GetManifestResourceStream( "SpiritIsland.WinForms.images."+filename );
			return new Bitmap( imgStream );
		}

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

		readonly Assembly assembly;

		#endregion
	}

}
