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
	public void CanConnect_TwoBoxes(string poly1, string poly2) {
		// B-D-F
		// | | |
		// A-C-E
		Polygons.JoinAdjacentPolgons( poly1.Split( ',' ), poly2.Split( ',' ) )
			.Join(",").ShouldBe( "C,A,B,D,F,E" ); //
	}

}
