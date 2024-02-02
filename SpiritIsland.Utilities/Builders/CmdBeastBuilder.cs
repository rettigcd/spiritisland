namespace SpiritIsland;

internal class CmdBeastBuilder {

	static public Bitmap ForStage( int stage ) {
		var bitmap = new Bitmap( 320, 200 );
		using Graphics graphics = Graphics.FromImage( bitmap );

		// rect: border
		new RectRect { Fill = "bab4a6", Stroke = "363636", }.RoundCorners( .1f )
			.FloatSelf()
			// rect: pink
			.Float( new RectRect { Fill = "fcc7b9" }.RoundCorners( .1f ), .03125f, .05f, .9375f, .9f )
			.Float( new ImgRect( Img.Icon_Beast ), 0f, .5f, .4f, .4f ) // Bottom Left
			.Float( new ImgRect( Img.Icon_Beast ), .83f, .33f, .1f, .1f ) // inline
																		  // Text
			.Float( new TextRect( "COMMAND BEASTS" ), 0f, .08f, 1f, .12f )
			.Float( new TextRect( GetStageStr( stage ) ), 0f, .2f, 1f, .12f )
			.Float( new TextRect( "Use during Fast phase to choose for each" ), .0f, .35f, 1f, .09f )
			.Float( new TextRect( "• 1 Damage." ), .3f, .5f, .6f, .09f )
			.Float( new TextRect( "• 1 Fear if Invaders are present." ), .3f, .62f, .6f, .09f )
			.Float( new TextRect( "• Push that." ), .3f, .75f, .6f, .09f )
			.Paint( graphics, new Rectangle( 0, 0, 320, 200 ) );

		return bitmap;
	}

	private static string GetStageStr( int stage ) {
		return stage switch {
			2 => "STAGE II",
			3 => "STAGE III",
			_ => throw new ArgumentException( "Only stage 2 or 3 is allowd", nameof( stage ) )
		};
	}
}
