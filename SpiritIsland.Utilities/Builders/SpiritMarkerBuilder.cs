using System.Drawing;

namespace SpiritIsland.WinForms;

class SpiritMarkerBuilder {

	static public Image BuildPresence( PresenceTokenAppearance presenceAppearance ) {
		Bitmap bitmap = ResourceImages.Singleton.GetPresenceImage( presenceAppearance.BaseImage );
		presenceAppearance.Adjustment?.Adjust( bitmap );
		return bitmap;
	}

	static public Image BuildMarker( Img img, BitmapAdjustment? adjustment ) {
		Bitmap bitmap = ResourceImages.Singleton.GetImage( img );
		adjustment?.Adjust( bitmap );
		return bitmap;
	}

}