using System.Drawing;

namespace SpiritIsland; 
static public class SimpleMods {

	static public void Ghostify( Bitmap image ) {
		static Color MakePartiallyTransparent( Color x ) => Color.FromArgb( Math.Min( (byte)92, x.A ), x );
		new PixelAdjustment( MakePartiallyTransparent ).Adjust( image );
	}

	static public void Grayify( Bitmap image ) {
		static Color LowerContrast( Color x ) => Color.FromArgb( x.A, x.R / 4 + 96, x.G / 4 + 96, x.B / 4 + 96 );
		new PixelAdjustment( LowerContrast ).Adjust( image );
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

		//static Color Weight( Color a, int aW, Color b, int bW ) => Color.FromArgb(
		//	(a.A * aW + b.A * bW) / (aW + bW),
		//	(a.R * aW + b.R * bW) / (aW + bW),
		//	(a.G * aW + b.G * bW) / (aW + bW),
		//	(a.B * aW + b.B * bW) / (aW + bW)
		//);

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

	public static void Contrast( Bitmap shape, float strength ) {
		// Get the histogram
		int[] origHist = new int[101];
		for(int x = 0; x < shape.Width; ++x)
			for(int y = 0; y < shape.Height; ++y) {
				Color rgb = shape.GetPixel( x, y );
				HSL hsl = HSL.FromRgb( rgb );
				int l = (int)(hsl.L * 100 + .5f);
				origHist[l]++;
			}

		// Find the Luminosity Map
		int total = shape.Width * shape.Height;
		float[] newLs = new float[101];
		int previous = 0;
		int lowEnd = 0;
		for(int origL = 0; origL <= 100; ++origL) {
			previous += origHist[origL];
			int highEnd = previous * 100 / total;
			int flatL = (highEnd + lowEnd) / 2;
			newLs[origL] = (flatL * strength + origL * (1f - strength)) * .01f; // I think this is poor-mans strength-calc. Not sure what the offical way to do this is.
																				// next
			lowEnd = highEnd;
		}

		// Apply the Luminosity map
		for(int x = 0; x < shape.Width; ++x)
			for(int y = 0; y < shape.Height; ++y) {
				Color origRgb = shape.GetPixel( x, y );
				HSL origHsl = HSL.FromRgb( origRgb );
				int l = (int)(origHsl.L * 100 + .5f);
				HSL newHsl = new HSL( origHsl.H, origHsl.S, newLs[l] );
				shape.SetPixel( x, y, Color.FromArgb( origRgb.A, newHsl.ToRgb() ) );
			}

	}

}
