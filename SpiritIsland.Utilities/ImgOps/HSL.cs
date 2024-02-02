namespace SpiritIsland;

/// <summary>
/// Represents color as Hue,Saturation,Lightness
/// </summary>
public class HSL {

	#region constructors

	public HSL(float h, float s, float l ) {
		H = h;
		S = s;
		L = l;
		// int versions
		_h = (int)(h + .5f);
		_s = (int)(s*100f + .5f);
		_l = (int)(l*100f + .5f);
	}
	public HSL( int hueDegrees, int saturationPercentage, int lightnessPercentage ) {
		// float
		H = hueDegrees;
		S = saturationPercentage * .01f;
		L = lightnessPercentage * .01f;
		// int versions
		_h = hueDegrees;
		_s = saturationPercentage;
		_l = lightnessPercentage;
	}
	#endregion

	/// <summary> Hue </summary>
	public float H;
	/// <summary> Saturation </summary>
	public float S;
	/// <summary> Lightness </summary>
	public float L;

	#region private

	readonly int _h;
	readonly int _s;
	readonly int _l;

	#endregion

	public override string ToString() => $"{_h} {_s} {_l}"; // $"{H:0.##} {S:0.##} {L:0.##}";

	#region To/From RGB

	public Color ToRgb() {
		// https://www.had2know.org/technology/hsl-rgb-color-converter.html

		float d = S * (1 - Math.Abs( 2 * L - 1 ));
		float m = L - 0.5f * d;
		float x = d * (1 - Math.Abs( (H / 60) % 2f - 1 ));

		static Color Result( float r, float g, float b ) => System.Drawing.Color.FromArgb( (int)(r * 255), (int)(g * 255), (int)(b * 255) );
		return H < 60 ? Result( m + d, m + x, m )
			: H < 120 ? Result( m + x, m + d, m )
			: H < 180 ? Result( m, m + d, m + x )
			: H < 240 ? Result( m, m + x, m + d )
			: H < 300 ? Result( m + x, m, m + d )
			: Result( m + d, m, m + x );
	}

	public static HSL FromRgb( Color color ) {
		// https://www.had2know.org/technology/hsl-rgb-color-converter.html
		float r = color.R / 255f;
		float g = color.G / 255f;
		float b = color.B / 255f;

		float max = Math.Max( Math.Max( r, g ), b );
		float min = Math.Min( Math.Min( r, g ), b );
		float d = max - min;

		float lightness = (max + min) / 2;
		float saturationDenominator = (1 - Math.Abs( 2 * lightness - 1 ));
		float saturation = saturationDenominator == 0 ? 0f : lightness == 0f ? 0f : d / saturationDenominator;

		static float hueWhenGreenIsGreaterThanBlue( float r, float g, float b ) {
			double num = (r - 0.5 * (g + b));
			double den = Math.Sqrt( r*r + g*g + b*b - r*g - r*b - g*b );
			double ratio = num / den; 
			if(ratio < -1 ) ratio = -1; else if( 1 < ratio ) ratio = 1; // guard against rounding issues
			return (float) (Math.Acos( ratio ) * 180 / Math.PI);
		}

		float hue = (r == b && b == g) ? 0f
			: b <= g ? hueWhenGreenIsGreaterThanBlue( r, g, b )
			: 360 - hueWhenGreenIsGreaterThanBlue( r, g, b );
		return new HSL( hue, saturation, lightness );
	}

	#endregion
}

