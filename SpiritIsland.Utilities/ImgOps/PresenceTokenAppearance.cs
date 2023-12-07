namespace SpiritIsland;

/// <summary> Describes what a presence token looks like. </summary>
public class PresenceTokenAppearance {

	public PresenceTokenAppearance( HSL hsl ) {
		Hsl = hsl;
		PatternImage = null;
	}

	public PresenceTokenAppearance(float hue, float saturation, float lightness=.5f ) {
		Hsl = new HSL(hue, saturation, lightness);
		PatternImage = null;
	}

	public PresenceTokenAppearance(string spiritName ) {
		PatternImage = spiritName;
	}

	public BitmapAdjustment? Adjustment => Hsl is null ? null : new PixelAdjustment( new HslColorAdjuster(Hsl).GetNewColor );

	public HSL? Hsl { get; }
	public string BaseImage => "red";
	public string? PatternImage;

}