using System.Drawing;

namespace SpiritIsland; 
static public class SimpleMods {

	#region public custom: Ghostify, Grayify, Color overlay

	static public void Ghostify( Bitmap image ) {
		static Color MakePartiallyTransparent( Color x ) => Color.FromArgb( Math.Min( (byte)92, x.A ), x );
		new PixelAdjustment( MakePartiallyTransparent ).Adjust( image );
	}

	static public void Grayify( Bitmap image ) {
		const int Gray = 96;
		static Color MakeMostlyGray( Color x ) => Color.FromArgb( x.A, 
			x.R / 4 + Gray,
			x.G / 4 + Gray,
			x.B / 4 + Gray
		);
		new PixelAdjustment( MakeMostlyGray ).Adjust( image );
	}

	// Modifies the shape bitmap by Overlaying the pattern Color & (muted) Lightness
	static public void ColorOverlay( Bitmap shape, Bitmap pattern, Point patternOffset ) {
		for(int x = 0; x < shape.Width; ++x)
			for(int y = 0; y < shape.Height; ++y) {

				// Shape
				Color shapeRgb = shape.GetPixel( x, y );
				HSL shapeHsl = HSL.FromRgb( shapeRgb );

				// Pattern
				Color patternRgb = Average(
					pattern.GetPixel( patternOffset.X + x * 2, patternOffset.Y + y * 2 ),
					pattern.GetPixel( patternOffset.X + x * 2 + 1, patternOffset.Y + y * 2 ),
					pattern.GetPixel( patternOffset.X + x * 2, patternOffset.Y + y * 2 + 1 ),
					pattern.GetPixel( patternOffset.X + x * 2 + 1, patternOffset.Y + y * 2 + 1 )
				);
				HSL patternHsl = HSL.FromRgb( patternRgb );

				// H << Pattern
				// S << Pattern
				// L << Patter * Shape
				// Transparency << Shape
				const float shapeLumWeight = .75f;
				HSL colorOverlayHsl = new HSL(
					patternHsl.H,
					patternHsl.S,
					// (patternHsl.L+2f)/3 * shapeHsl.L
					patternHsl.L * (1f - shapeLumWeight) + (shapeHsl.L * shapeLumWeight)
				);
				Color colorOverlayRgb = Color.FromArgb( shapeRgb.A, colorOverlayHsl.ToRgb() );

				// shape.SetPixel(x,y,Weight(colorOverlayRgb,2,shapeRgb,1));
				shape.SetPixel( x, y, colorOverlayRgb );
			}

		static Color Average( params Color[] colors ) {
			int r = 0, g = 0, b = 0, a = 0;
			foreach(Color c in colors) {
				r += c.R;
				g += c.G;
				b += c.B;
				a += c.A;
			}
			return Color.FromArgb( a / colors.Length, r / colors.Length, g / colors.Length, b / colors.Length );
		};

	}

	#endregion public custom: Ghostify, Grayify, Color overlay

	/// <summary>
	/// Attempts to Flatten/Equalize the images Histograph
	/// </summary>
	static public void Contrast_FlattenHistogram( Bitmap bitmap, float strength ) {
		// Get the histogram
		var origHist = new CountDictionary<float>();
		for(int x = 0; x < bitmap.Width; ++x)
			for(int y = 0; y < bitmap.Height; ++y)
				origHist[ HSL.FromRgb( bitmap.GetPixel( x, y ) ).L ]++;

		// Build the Luminosity Map
		Dictionary<float, float> luminosityMap = new Dictionary<float, float>();
		int total = bitmap.Width * bitmap.Height;
		float totalInv = .5f / total; // .5 is pre multiplying value because we are going to average later
		int previous = 0; // counts
		float lowEnd = 0f;
		foreach(float origL in origHist.Keys.OrderBy(x=>x).ToArray()){
			previous += origHist[origL];
			float highEnd = previous * totalInv;
			float newL = highEnd + lowEnd; // Use value in the middle (values were prescaled to *.5 so no need to /2 here)
			luminosityMap[origL] = newL * strength + origL * (1f - strength); // I think this is poor-mans strength-calc. Not sure what the offical way to do this is.
			lowEnd = highEnd;
		}

		// Apply the Luminosity map
		for(int x = 0; x < bitmap.Width; ++x)
			for(int y = 0; y < bitmap.Height; ++y) {
				Color origRgb = bitmap.GetPixel( x, y );
				HSL origHsl = HSL.FromRgb( origRgb );
				HSL newHsl = new HSL( origHsl.H, origHsl.S, luminosityMap[origHsl.L] );
				bitmap.SetPixel( x, y, Color.FromArgb( origRgb.A, newHsl.ToRgb() ) );
			}

	}

	/// <param name="bitmap"></param>
	/// <param name="contrastStrength">Normal range: -1..1 => (1/3)..(3)  Max range: -2..2</param>
	static public void Contrast_FrancisGLoch( Bitmap bitmap, float contrastStrength ) {
		if(contrastStrength == 0f) return;
		var adj = MakeLuminosityAdjustor( ContrastMap_FrancisGLoch( contrastStrength ) );
		new PixelAdjustment( adj ).Adjust(bitmap);
	}

	static public void BrightnessContrast_Photoshop( Bitmap bitmap, float brightness, float contrast ) {
		Func<float, float> brightnessMap = BrightnessMap_Photoshop( brightness );
		Func<float, float> contrastMap = ContrastMap_Photoshop( contrast );
		Func<Color, Color> colorMap = MakeLuminosityAdjustor( x => brightnessMap( contrastMap( x ) ) );
		new PixelAdjustment( colorMap )
			.Adjust( bitmap );
	}

	#region private Contrast / Brightness maps

	/// <param name="normalizedUserSetting">-1 .. 1</param>
	/// <returns>A scaling value from 0 to ∞</returns>
	/// <remarks>
	/// https://www.dfstudios.co.uk/articles/programming/image-programming-algorithms/image-processing-algorithms-part-5-contrast-adjustment/
	/// </remarks>
	/// <example>
	/// F(-1) => 0,	values go to 0, F(0) => 1, F(+1) => ∞
	static Func<float, float> ContrastMap_FrancisGLoch( float userSelectedStrength ) {
		float Calc_FrancisGLochStretchFactor( float x ) => x <= -1 ? 0 : 1 <= x ? 256f : (1 + x) / (1 - x);
		float scale = Calc_FrancisGLochStretchFactor( userSelectedStrength * .5f ); // 

		return ( float l ) => {
			float stretched = (l - .5f) * scale + .5f;
			return stretched <= 0f ? 0f : 1f <= stretched ? 1f : stretched;
		};
	}

	/// <summary>
	/// Dino Dini's Normalized tunable Sigmoid func
	/// </summary>
	/// <param name="k">
	/// Extreme: -1..1  
	/// Better values: -.5 to .5
	/// Positive values of K decrease the slope at origin (less contrast, less brightness)
	/// Negative vlaues of K increate the slope at the origin (more contrast, more brightness)
	/// </param>
	/// <returns>
	/// function that is ready to go for adjusting Brightness in Luminosity range (0..1)
	/// or can be domain/range shift for adjusting Contrast 
	/// </returns>
	/// <remarks>  https://dhemery.github.io/DHE-Modules/technical/sigmoid/ </remarks>
	static Func<float,float> MakeSigmoid( float k ) =>
		k == 0 ? x => x
		: ( x ) => x * (1 - k) / (k - 2 * k * Math.Abs( x ) + 1); // the SigMoid function
	
	/// <param name="userSelectedStrength">1=> increase brightness a lot, -1 decrease brightness a lot</param>
	static Func<float,float> BrightnessMap_Photoshop( float userSelectedStrength )
		// - => Sigmoid works backwards from expected
		// .3 => approximate reduction observed in photoshop and protects users from function blowing up near real 1 and -1
		=> MakeSigmoid( userSelectedStrength * -.3f );

	/// <param name="userSelectedStrength">1=> increase contrast a lot, -1 decrease contrast a lot, 0=>no change</param>
	static Func<float, float> ContrastMap_Photoshop( float userSelectedStrength ) {
		// - => Sigmoid works backwards from expected
		// .9 => approximate reduction observed in photoshop and protects users from function blowing up near real 1 and -1
		var contrastSigmoid = MakeSigmoid( userSelectedStrength * -.9f );
		return (x) => (contrastSigmoid(x*2-1)+1)*.5f; // shift Domain (input) from 0..1 -1..1 and shift Range (output) from -1..1 to 0..1
	}

	#endregion private Contrast / Brightness maps

	static Func<Color,Color> MakeLuminosityAdjustor( Func<float,float> luminosityMap ) {
		return orig => {
			HSL origHsl = HSL.FromRgb( orig );
			return Color.FromArgb( orig.A, new HSL( origHsl.H, origHsl.S, luminosityMap( origHsl.L ) ).ToRgb() );
		};
	}



}