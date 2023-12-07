using System.Drawing;

namespace SpiritIsland;

/// <summary>
/// Applies a location-independed Modification. 
/// Mod is based only on the starting color of the source pixel.
/// </summary>
public class PixelAdjustment : BitmapAdjustment {
	readonly Func<Color,Color> _adjust;
	public PixelAdjustment( Func<Color, Color> adjust ) {
		_adjust = adjust;
	}

	public void Adjust( Bitmap bitmap ) {
		for(int x = 0; x < bitmap.Width; ++x)
			for(int y = 0; y < bitmap.Height; ++y)
				bitmap.SetPixel( x, y, _adjust( bitmap.GetPixel( x, y ) ) );
	}
}