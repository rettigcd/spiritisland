using System.Drawing;

namespace SpiritIsland;

/// <summary>
/// Maps a color.  Configured with an HSL value. 
/// H: Returned as H of new color
/// S: Returned as S of new color
/// L: Used like overlay to push high L values up and low L values down.
/// </summary>
public class HslColorAdjuster : ColorAdjuster {

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

	#region private
	readonly HSL _desiredHsl;
	readonly float _lScaler;
	readonly float _lOffset;
	#endregion

}

