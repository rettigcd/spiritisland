using System.Drawing;
using System.Reflection;

namespace SpiritIsland.WinForms {
	public class ResourceImages {
		public readonly Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
		public Bitmap GetPresenceIcon( string presenceColor ) {
			System.IO.Stream presenceStream = assembly.GetManifestResourceStream( $"SpiritIsland.WinForms.images.presence.{presenceColor}.png" );
			return new Bitmap( presenceStream );
		}

		public Bitmap GetEnergyIcon( string energyText ) {
			var imgStream = assembly.GetManifestResourceStream( $"SpiritIsland.WinForms.images.tokens.{energyText}.png" );
			return new Bitmap( imgStream );
		}
		public Bitmap GetCardplayIcon( string cardText ) {
			var imgStream = assembly.GetManifestResourceStream( $"SpiritIsland.WinForms.images.tokens.{cardText}.png" );
			return new Bitmap( imgStream );
		}
	}

}
