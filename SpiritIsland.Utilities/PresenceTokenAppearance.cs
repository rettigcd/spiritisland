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

	public BitmapAdjustment? Adjustment => Hsl is null ? null : new PixelAdjustment( new HslColorAdjuster(Hsl).GetNewColor );

	public HSL? Hsl { get; }
	public readonly string BaseImage;

}

public interface BitmapAdjustment {
	void Adjust( Bitmap bitmap );
}

public interface ColorAdjuster {
	Color GetNewColor( Color p );
}

public class HslColorAdjuster : ColorAdjuster {
	readonly HSL _desiredHsl;
	readonly float _lScaler;
	readonly float _lOffset;
	public HslColorAdjuster(HSL desiredHsl ) {
		_desiredHsl = desiredHsl;

		float l2 = _desiredHsl.L * 2; // l2 range is 0..2
		if(l2 < 1) {
			// 0  pulls everything down to black
			_lScaler = l2;
			_lOffset = 0;
		} else if(1 < l2) {
			// 2  pulls everything up to white
			_lScaler = 2 - l2;
			_lOffset = l2 - 1;
		} else {
			//  0  no change
			_lScaler = 1;
			_lOffset = 0;
		}
	}
	public Color GetNewColor( Color p )
		=> Color.FromArgb( p.A, AdjustHsl( HSL.FromRgb( p ) ).ToRgb() );

	HSL AdjustHsl( HSL hsl ) => new HSL( _desiredHsl.H, _desiredHsl.S, AdjustLightness( hsl.L ) );

	// hsl.H = x*360f/orig.Width; // make a rainbow!
	// hsl.L += (1f - hsl.L) * .2f;
	protected float AdjustLightness( float lightness ) => lightness * _lScaler + _lOffset;
}

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

