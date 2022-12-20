using System.Drawing;

namespace SpiritIsland;

/// <summary> Describes what a presence token looks like. </summary>
public class PresenceTokenAppearance {

	public PresenceTokenAppearance(string name) { 
		BaseImage = name;
	}

	public PresenceTokenAppearance( HSL hsl, string baseImage = "red" ) {
		Hsl = hsl;
		BaseImage = baseImage;
	}

	public PresenceTokenAppearance(float hue, float saturation, float lightness=.5f, string baseImage = "red" ) {
		Hsl = new HSL(hue, saturation, lightness);
		BaseImage = baseImage;
	}

	public HslAdjustment Adjustment => Hsl is null ? null : new HslAdjustment(Hsl);

	public HSL Hsl { get; }
	public readonly string BaseImage;

}

public interface BitmapAdjustment {
	void Adjust( Bitmap bitmap );
}

public class HslAdjustment : BitmapAdjustment {
	readonly HSL _desiredHsl;
	readonly float _lScaler;
	readonly float _lOffset;
	public HslAdjustment( HSL desiredHsl ) {
		_desiredHsl = desiredHsl;

		float l2 = _desiredHsl.L * 2; // l2 range is 0..2
		if(l2 < 1) {
			// 0  pulls everything down to black
			_lScaler = l2;
			_lOffset = 0;
		} else if ( 1 < l2 ) {
			// 2  pulls everything up to white
			_lScaler = -l2;
			_lOffset = l2 - 1;
		} else {
			//  0  no change
			_lScaler = 1;
			_lOffset = 0;
		}
	}

	public void Adjust( Bitmap bitmap ) {
		for(int x = 0; x < bitmap.Width; ++x)
			for(int y = 0; y < bitmap.Height; ++y) {
				var p = bitmap.GetPixel( x, y );
				var hsl = HSL.FromRgb( p );
				AdjustPixelHsl( hsl );
				var newColor = Color.FromArgb( p.A, hsl.ToRgb() );
				bitmap.SetPixel( x, y, newColor );
			}
	}

	void AdjustPixelHsl( HSL hsl ) {
		hsl.H = _desiredHsl.H;
		hsl.S = _desiredHsl.S;
		// hsl.H = x*360f/orig.Width; // make a rainbow!
		// hsl.L += (1f - hsl.L) * .2f;
		hsl.L = AdjustLightness( hsl.L );
	}

	protected float AdjustLightness(float lightness) =>lightness * _lScaler + _lOffset;

}

