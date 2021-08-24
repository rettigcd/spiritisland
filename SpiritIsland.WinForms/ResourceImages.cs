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

		public readonly PrivateFontCollection Fonts; // 			return new Font( fonts.Families[0], 22 );

		public Bitmap GetPresenceIcon( string presenceColor ) => GetResourceImage( $"presence.{presenceColor}.png" );

		public Bitmap GetTokenIcon( string cardText )             => GetResourceImage( $"tokens.{cardText}.png" );

		#region private

		Bitmap GetResourceImage( string s ) {
			var imgStream = assembly.GetManifestResourceStream( "SpiritIsland.WinForms.images."+s );
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
