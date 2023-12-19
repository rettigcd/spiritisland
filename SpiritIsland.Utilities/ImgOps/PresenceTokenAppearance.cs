namespace SpiritIsland;

/// <summary> Describes what a presence token looks like. </summary>
class PresenceTokenAppearance {

	public PresenceTokenAppearance( HSL hsl ) {
		Hsl = hsl;
	}

	public PresenceTokenAppearance(float hue, float saturation, float lightness=.5f ) {
		Hsl = new HSL(hue, saturation, lightness);
	}

	public BitmapAdjustment? Adjustment => Hsl is null ? null : new PixelAdjustment( new HslColorAdjuster(Hsl).GetNewColor );

	public HSL? Hsl { get; }

}