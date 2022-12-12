namespace SpiritIsland;

/// <summary> Describes what a presence token looks like. </summary>
public class PresenceTokenAppearance {

	public PresenceTokenAppearance(string name) { 
		BaseImage = name;
		AdjustHsl = false;
	}

	public PresenceTokenAppearance(float hue, float saturation, string baseImage = "red" ) {
		AdjustHsl = true;
		Hue = hue;
		Saturation = saturation;
		BaseImage = baseImage;
	}

	public readonly string BaseImage;

	public bool AdjustHsl;
	public readonly float Hue;
	public readonly float Saturation;
	public float Pull = 0;  // pulls the Lightness value up or down

	public void Adjust( HSL hsl ) {
		hsl.H = Hue;
		hsl.S = Saturation;

		// -1  pulls everything down to black
		// -.5
		//  0  no change
		// +1  pulls everything up to white
		if( Pull < 0 )
			hsl.L += hsl.L * Pull;
		else if( 0 < Pull )
			hsl.L += (1-hsl.L) * Pull;
	}
}
