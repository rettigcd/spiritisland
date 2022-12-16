namespace SpiritIsland;

/// <summary> Describes what a presence token looks like. </summary>
public class PresenceTokenAppearance {

	public PresenceTokenAppearance(string name) { 
		BaseImage = name;
		AdjustHsl = false;
	}

	public PresenceTokenAppearance( HSL hsl, string baseImage = "red" ) {
		AdjustHsl = true;

		Hue = hsl.H;
		Saturation = hsl.S;
		Lightness = hsl.L;
		_pull = Lightness * 2 - 1;

		BaseImage = baseImage;
	}

	public PresenceTokenAppearance(float hue, float saturation, float lightness=.5f, string baseImage = "red" ) {
		AdjustHsl = true;
		Hue = hue;
		Saturation = saturation;
		Lightness = lightness;
		_pull = Lightness * 2 - 1;

		BaseImage = baseImage;
	}

	public readonly bool AdjustHsl;
	public readonly string BaseImage;
	public readonly float Hue;
	public readonly float Saturation;
	public readonly float Lightness;
	readonly float _pull;

	public void Adjust( HSL hsl ) {
		if( !AdjustHsl ) return;
		hsl.H = Hue;
		hsl.S = Saturation;

		// -1  pulls everything down to black
		// -.5
		//  0  no change
		// +1  pulls everything up to white
		if( _pull < 0 )
			hsl.L += hsl.L * _pull;
		else if( 0 < _pull )
			hsl.L += (1-hsl.L) * _pull;
	}
}
