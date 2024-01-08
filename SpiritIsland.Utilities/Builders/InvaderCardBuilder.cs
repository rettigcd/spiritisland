using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpiritIsland;

interface InvaderCardResources {
	Brush UseTerrainBrush( Terrain terrain );
	Font UseInvaderFont( float fontHeight );
	Bitmap InvaderCardImage( string backOrEscalation );
}

class InvaderCardBuilder {

	static public Bitmap BuildInvaderCard( InvaderCard card, InvaderCardResources resources ) {

		Bitmap bitmap = new Bitmap( 200, 320 );
		Graphics graphics = Graphics.FromImage( bitmap );
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
		var perimeter = new PointF[] {
			// Top Half
			new PointF( 20    , 160 ),
			new PointF( 20-10 , 90 ),
			new PointF( 20    , 20 ), // top left
			new PointF( 100   , 25 ),
			new PointF( 180   , 20 ), // top right
			new PointF( 180+10, 90 ),
			new PointF( 180   , 160 ),
			// Bottom Half
			new PointF( 180   , 160 ),
			new PointF( 180+10, 230 ),
			new PointF( 180   , 290 ), // bottom right
			new PointF( 100   , 285 ),
			new PointF( 20    , 290 ), // bottom left
			new PointF( 20-10 , 230 ),
			new PointF( 20    , 160 ),
		};

		// Background
		var backgroundBrush = Brushes.Bisque;//  Brushes.SaddleBrown; // Brushes.BurlyWood; // or maybe Bisque
		graphics.FillRoundedRectangle( backgroundBrush, new Rectangle( 0, 0, 200, 320 ), 20 );

		Pen perimeterPen = new Pen( Color.Black, 3f );
		using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };

		// Draw perimeter and inner texture
		if(card.Filter is SingleTerrainFilter singleTerrain) {
			Rectangle topRect = new Rectangle( 30, 45, 200 - 2 * 30, 160 - 60 );
			Rectangle botRect = new Rectangle( 30, 160 + 15, 200 - 2 * 30, 160 - 60 );

			// texture
			using Brush terrainBrush = resources.UseTerrainBrush( singleTerrain.Terrain );
			float tension = .15f;
			graphics.FillClosedCurve( terrainBrush, perimeter, FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

			// Abreviation Text in the middle
			using Font bigFont = resources.UseInvaderFont( 60f );
			graphics.DrawString( singleTerrain.Terrain.ToString()[..1], bigFont, backgroundBrush, topRect, alignCenter );

			// Escalation
			if(card.HasEscalation) {
				var ellipseRect = botRect.InflateBy( -45, -20 );
				graphics.FillEllipse( backgroundBrush, ellipseRect );
				using Bitmap escalation = resources.InvaderCardImage( "escalation.png" );
				graphics.DrawImageFitHeight( escalation, ellipseRect.InflateBy( -5 ) );
			}

		} else if(card.Filter is DoubleTerrainFilter doubleTerrain) {
			Rectangle topRect = new Rectangle( 30, 30 + 15, 200 - 2 * 30, 160 - 60 );
			Rectangle botRect = new Rectangle( 30, 160 + 25, 200 - 2 * 30, 160 - 60 );

			float tension = .15f;
			int countPerSide = perimeter.Length / 2;

			// texture - 1
			using Brush brush1 = resources.UseTerrainBrush( doubleTerrain.Terrain1 );
			graphics.FillClosedCurve( brush1, perimeter.Take( countPerSide ).ToArray(), FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );
			// texture - 2
			using Brush brush2 = resources.UseTerrainBrush( doubleTerrain.Terrain2 );
			graphics.FillClosedCurve( brush2, perimeter.Skip( countPerSide ).Take( countPerSide ).ToArray(), FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

			using Font bigFont = resources.UseInvaderFont( 60f );
			// Text
			graphics.DrawString( doubleTerrain.Terrain1.ToString()[..1], bigFont, backgroundBrush, topRect, alignCenter );
			graphics.DrawString( doubleTerrain.Terrain2.ToString()[..1], bigFont, backgroundBrush, botRect, alignCenter );

		} else {

			// must be coastal / salt deposits
			Rectangle topRect = new Rectangle( 30, 30 + 30, 200 - 2 * 30, 160 - 60 );
			Rectangle botRect = new Rectangle( 30, 160 + 20 + 15, 200 - 2 * 30, 160 - 60 );

			if(card.Filter.Text == CoastalFilter.Name ) {
				// texture
				using Brush terrainBrush = resources.UseTerrainBrush( Terrain.Ocean );
				float tension = .15f;
				graphics.FillClosedCurve( terrainBrush, perimeter, FillMode.Alternate, tension );
				graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

				// Abreviation Text in the middle
				using Font bigFont = resources.UseInvaderFont( 25f );
				graphics.DrawString( "Coastal", bigFont, backgroundBrush, topRect, alignCenter );
				graphics.DrawString( "Lands", bigFont, backgroundBrush, botRect, alignCenter );
			}

			if(card.Filter.Text.Contains("Salt")) {
				// texture
				using Image image = ResourceImages.Singleton.AdversaryFlag( "Habsburg Mining Expedition" );
				using Brush terrainBrush = new TextureBrush( image );

				float tension = .15f;
				graphics.FillClosedCurve( terrainBrush, perimeter, FillMode.Alternate, tension );
				graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

				// Abreviation Text in the middle
				using Font bigFont = resources.UseInvaderFont( 24f );
				graphics.DrawString( "Salt", bigFont, backgroundBrush, topRect, alignCenter );
				graphics.DrawString( "Deposits", bigFont, backgroundBrush, botRect, alignCenter );
			}

		}
		Font bottomFont = resources.UseInvaderFont( 20f );
		graphics.DrawString(
			card.InvaderStage switch { 1 => "I", 2 => "II", 3 => "III", _ => "" },
			bottomFont, Brushes.Brown,
			new RectangleF( 0, 285, 200, 30 ), alignCenter
		);
		return bitmap;
	}

	static public Bitmap BuildInvaderCardBack( int stage, InvaderCardResources resources ) {
		Rectangle cardRect = new Rectangle( 0, 0, 200, 320 );
		Bitmap bitmap = new Bitmap( 200, 320 );
		Graphics graphics = Graphics.FromImage( bitmap );
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

		// Outside
		using(SolidBrush brush = new SolidBrush( Color.LightSteelBlue ))
			graphics.FillRoundedRectangle( brush, cardRect, (int)(cardRect.Width * .15f) );

		var smallerRect = cardRect.InflateBy( -15 );

		// Image
		using Bitmap img = resources.InvaderCardImage( "back.jpg" );
		using TextureBrush textureBrush = new TextureBrush( img );
		graphics.FillRoundedRectangle(textureBrush, smallerRect, 20 );

		using Font invaderStageFont = resources.UseInvaderFont( 100f );
		using StringFormat alignCenterBoth = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
		graphics.DrawString( stage.ToString(), invaderStageFont, Brushes.DarkRed, smallerRect, alignCenterBoth );

		return bitmap;
	}

}
