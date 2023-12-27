using System.Drawing;

namespace SpiritIsland;

/// <summary> Describes what a presence token looks like. </summary>
class PresenceTokenAppearance {

	public PresenceTokenAppearance( HSL hsl ) {
		Hsl = hsl;
	}

	public PresenceTokenAppearance(float hue, float saturation, float lightness=.5f ) {
		Hsl = new HSL(hue, saturation, lightness);
	}

	public BitmapAdjustment HslAdjustment => new PixelAdjustment( new HslColorAdjuster(Hsl).GetNewColor );

	public Point PatternOffset = new Point(100,100);
	public float Contrast = .5f;


	public HSL Hsl { get; }

}