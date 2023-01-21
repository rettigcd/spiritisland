namespace SpiritIsland.Tests;
public class Polygon_Tests {

	[Theory]
	[InlineData( "A,B,D,C", "C,D,F,E" )]
	[InlineData( "A,B,D,C", "D,F,E,C" )]
	[InlineData( "A,B,D,C", "F,E,C,D" )]
	[InlineData( "A,B,D,C", "E,C,D,F" )]
	[InlineData( "B,D,C,A", "C,D,F,E" )]
	[InlineData( "B,D,C,A", "D,F,E,C" )]
	[InlineData( "B,D,C,A", "F,E,C,D" )]
	[InlineData( "B,D,C,A", "E,C,D,F" )]
	[InlineData( "D,C,A,B", "C,D,F,E" )]
	[InlineData( "D,C,A,B", "D,F,E,C" )]
	[InlineData( "D,C,A,B", "F,E,C,D" )]
	[InlineData( "D,C,A,B", "E,C,D,F" )]
	[InlineData( "C,A,B,D", "C,D,F,E" )]
	[InlineData( "C,A,B,D", "D,F,E,C" )]
	[InlineData( "C,A,B,D", "F,E,C,D" )]
	[InlineData( "C,A,B,D", "E,C,D,F" )]
	public void TwoBoxes(string poly1, string poly2) {
		// B-D-F
		// | | |
		// A-C-E
		Polygons.JoinAdjacentPolgons( poly1.Split( ',' ), poly2.Split( ',' ) )
			.Join(",").ShouldBe( "C,A,B,D,F,E" ); //
	}

	[Theory]
	[InlineData( 0, 0, 0, "0 0 0" )]            // black
	[InlineData( 128, 128, 128, "0 0 50" )]    // gray
	[InlineData( 255, 255, 255, "0 0 100" )]      // white
	[InlineData( 255, 0, 0, "0 100 50" )]        // red
	[InlineData( 0, 255, 0, "120 100 50" )]      // green
	[InlineData( 0, 0, 255, "240 100 50" )]      // blue
	[InlineData( 255, 255, 0, "60 100 50" )]     // yellow
	[InlineData( 0, 255, 255, "180 100 50" )]    // cyan
	[InlineData( 255, 0, 255, "300 100 50" )]    // magenta
	[InlineData( 0, 1, 1, "180 100 0" )]          // almost black, had rounding error that blew up Hue
	public void Rgb2Hsl_test(int red, int green, int blue, string expected) {
		var orig = System.Drawing.Color.FromArgb( red, green, blue );

		HSL hsl = HSL.FromRgb( orig );
		hsl.ToString().ShouldBe( expected );

		var result = hsl.ToRgb(  );

		result.ToString().ShouldBe( orig.ToString() );
		result.R.ShouldBe(orig.R);
		result.G.ShouldBe( orig.G );
		result.B.ShouldBe( orig.B );
	}

}
