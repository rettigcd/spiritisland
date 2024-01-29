namespace SpiritIsland;

/// <summary> Describes what a presence token looks like. </summary>
class TokenAppearance {

	public TokenAppearance( HSL hsl ) {
		Hsl = hsl;
	}

	public TokenAppearance(float hue, float saturation, float lightness=.5f ) {
		Hsl = new HSL(hue, saturation, lightness);
	}

	public TokenAppearance BC(float brightness,float contrast ) {
		Brightness = brightness;
		Contrast = contrast;
		return this;
	}

	public TokenAppearance Offset( int x, int y ) {
		PatternOffset = new Point(x,y);
		return this;
	}


	public BitmapAdjustment HslAdjustment => new PixelAdjustment( new HslColorAdjuster(Hsl).GetNewColor );

	public Point PatternOffset = new Point(100,100);
	public float Brightness = 0f;
	public float Contrast = .6f;


	public HSL Hsl { get; }

}