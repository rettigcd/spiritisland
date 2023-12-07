using System.Drawing;

namespace SpiritIsland.WinForms;

class SpiritMarkerBuilder {

	static public Image BuildPresence( PresenceTokenAppearance presenceAppearance ) {
		Bitmap bitmap = ResourceImages.Singleton.GetPresenceImage( presenceAppearance.BaseImage );

		presenceAppearance.Adjustment?.Adjust( bitmap );

		if(presenceAppearance.PatternImage != null) {
			using Bitmap pattern = ResourceImages.Singleton.LoadSpiritImage( presenceAppearance.PatternImage );
			ColorOverlay(bitmap, pattern);

			//using Bitmap pattern = ResourceImages.Singleton.LoadSpiritImage( spirit.Text );
			//ColorOverlay(bitmap, pattern);


		}

		return bitmap;
	}

	static public Image BuildMarker( Img img, BitmapAdjustment? adjustment ) {
		Bitmap bitmap = ResourceImages.Singleton.GetImage( img );
		adjustment?.Adjust( bitmap );
		return bitmap;
	}

	// Modifies the shape bitmap by Overlaying the pattern Color & (muted) Lightness
	static void ColorOverlay( Bitmap shape, Bitmap pattern ) {
		for(int x = 0; x < shape.Width; ++x)
			for(int y = 0; y < shape.Height; ++y) {

				// Shape
				Color shapeRgb = shape.GetPixel( x, y );
				HSL shapeHsl = HSL.FromRgb(shapeRgb);

				// Pattern
//				Color patternRgb = pattern.GetPixel( x+100, y+100 );
				Color patternRgb = Average( 
					pattern.GetPixel( 100+x*2,   100+y*2 ), 
					pattern.GetPixel( 100+x*2+1, 100+y*2 ),
					pattern.GetPixel( 100+x*2,   100+y*2+1 ), 
					pattern.GetPixel( 100+x*2+1, 100+y*2+1 )
				);

				HSL patternHsl = HSL.FromRgb(patternRgb);
				float mutedPatternL = (patternHsl.L+2f)/3;

				HSL colorOverlayHsl = new HSL( patternHsl.H, patternHsl.S, mutedPatternL * shapeHsl.L );
				Color colorOverlayRgb = Color.FromArgb( shapeRgb.A, colorOverlayHsl.ToRgb() );

				shape.SetPixel(x,y,Weight(colorOverlayRgb,2,shapeRgb,1));
			}

		static Color Weight(Color a, int aW,Color b, int bW) => Color.FromArgb(
			(a.A*aW+b.A*bW)/(aW+bW),
			(a.R*aW+b.R*bW)/(aW+bW),
			(a.G*aW+b.G*bW)/(aW+bW),
			(a.B*aW+b.B*bW)/(aW+bW)
		);

		static Color Average(params Color[] colors) {
			int r=0,g=0,b=0,a=0;
			foreach(Color c in colors) {
				r+=c.R;
				g+=c.G;
				b+=c.B;
				a+=c.A;
			}
			return Color.FromArgb(a/colors.Length,r/colors.Length,g/colors.Length,b/colors.Length);
		};

	}


}