namespace SpiritIsland.Tests;

public class Transform_Tests {

	[Fact]
	public void Translate() {
		// final = Transform.Translate( 1, 0 ).Apply( orig )
		var final = new RowVector( 5f, 10f ) * RowVector.Translate( 1f, 2f );
		final.ShouldBe(6f,12f);
	}

	[Theory]
	[InlineData( 2f, 3f, 5f, 7f, 10f, 21f )]
	public void Scale( float initX, float initY, float scaleX, float scaleY, float finalX, float finalY ) {
		var final = new RowVector( initX, initY ) * RowVector.Scale( scaleX, scaleY );
		final.ShouldBe( finalX, finalY );
	}

	[Theory]
	[InlineData( 1f, 0f, 0f, 1f )]
	[InlineData( 0f, 1f, -1f, 0f )]
	[InlineData( -1f, 0f, 0f, -1f )]
	[InlineData( 0f, -1f, 1f, 0f )]
	[InlineData( 1f, 1f, -1f, 1f )]
	public void Rotate90(float initX, float initY, float finalX, float finalY ) {
		// This tests the Sin components since rotating cos(90) = 0
		var final = new RowVector( initX, initY ) * RowVector.RotateDegrees( 90 );
		final.ShouldBe( finalX,finalY);
	}

	[Theory]
	[InlineData( 1f, 0f )]
	[InlineData( -1f, 0f )]
	[InlineData( 0f, 1f )]
	[InlineData( 0f, -1f )]
	public void Rotate180( float initX, float initY ) {
		// This tests the Cos components since rotating sin(180) = 0
		var final = new RowVector( initX, initY ) * RowVector.RotateDegrees( 180 );
		final.ShouldBe( -initX, -initY );
	}

	[Fact]
	public void TranslateThenScale() {
		var transform = RowVector.Translate( 2, 3 ) * RowVector.Scale( 5f, 7f );
		(new RowVector( 0, 0 ) * transform).ShouldBe( 10f, 21f );
	}

	[Fact]
	public void ScaleThenTranslate() {
		var transform = RowVector.Scale( 5f, 7f ) 
			* RowVector.Translate( -30, 50 );
		(new RowVector( 2, 3 ) * transform).ShouldBe( -20f, 71f );
	}

	[Fact]
	public void TranslateThenRotate() {
		// Vector is on the right, so transformation on the right runs first
		var transform = RowVector.Translate( 1, 2 ) 
			* RowVector.RotateDegrees( 90 );
		var final = new RowVector( 10, 20 ) * transform;
		final.ShouldBe( -22f, 11f );
	}

	[Fact]
	public void RotateThenTranslate() {
		// Vector is on the right, so transformation on the right runs first
		var transform = RowVector.RotateDegrees( 90 ) * RowVector.Translate( 1, 2 );
		var final = new RowVector( 10, 20 ) * transform;
		// Rotate (10,20) => (-20,10)
		// Translate(1,2) => (-19,12
		final.ShouldBe( -19f, 12f );
	}

}

public static class FVector2Extensions {
	public static void ShouldBe( this RowVector final, float x, float y ) {
		const double tollerence = .0001;
		final.X.ShouldBe( x, tollerence );
		final.Y.ShouldBe( y, tollerence );
	}
	public static void ShouldBe( this System.Drawing.PointF final, float x, float y ) {
		const double tollerence = .0001;
		final.X.ShouldBe( x, tollerence );
		final.Y.ShouldBe( y, tollerence );
	}
}


