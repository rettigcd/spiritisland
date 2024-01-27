using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpiritIsland;

public interface InvaderCardResources {
	Brush UseTerrainBrush( Terrain terrain );
	Font UseInvaderFont( float fontHeight );
	Font UseGameFont( float fontHeight );
	Bitmap InvaderCardImage( string backOrEscalation );
}

public class InvaderCardBuilder {

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

		// Draw perimeter and inner texture
		if(card.Filter is SingleTerrainFilter singleTerrain)
			DrawSingleTerrain( card, resources, graphics, perimeter, backgroundBrush, singleTerrain );
		else if(card.Filter is DoubleTerrainFilter doubleTerrain)
			DrawDoubleTerrain( resources, graphics, perimeter, backgroundBrush, doubleTerrain );
		else {
			Draw_Special( card, resources, graphics, perimeter, backgroundBrush );

		}
		using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };
		Font bottomFont = resources.UseInvaderFont( 20f );
		graphics.DrawString(
			card.InvaderStage switch { 1 => "I", 2 => "II", 3 => "III", _ => "" },
			bottomFont, Brushes.Brown,
			new RectangleF( 0, 285, 200, 30 ), alignCenter
		);
		return bitmap;
	}

	static void Draw_Special( InvaderCard card, InvaderCardResources resources, Graphics graphics, PointF[] perimeter, Brush backgroundBrush ) {

		// must be coastal / salt deposits
		Rectangle topRect = new Rectangle( 30, 30 + 30, 200 - 2 * 30, 160 - 60 );
		Rectangle botRect = new Rectangle( 30, 160 + 20 + 15, 200 - 2 * 30, 160 - 60 );

		if(card.Filter.Text == CoastalFilter.Name)
			DrawCoastal( resources, graphics, perimeter, backgroundBrush, topRect, botRect );

		if(card.Filter.Text.Contains( "Salt" ))
			DrawSaltDeposits( resources, graphics, perimeter, backgroundBrush, topRect, botRect );

	}

	static void DrawSaltDeposits( InvaderCardResources resources, Graphics graphics, PointF[] perimeter, Brush backgroundBrush, Rectangle topRect, Rectangle botRect ) {

		// texture
		float tension = .15f;
		using( Bitmap cracks = ResourceImages.Singleton.CardTexture( "cracks.jpg" ) )
			using( Brush terrainBrush = new TextureBrush( cracks ) )
				graphics.FillClosedCurve( terrainBrush, perimeter, FillMode.Alternate, tension );

		using( Pen perimeterPen = new Pen( Color.Black, 3f ) )
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

		using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };
		using(Font bigFont = resources.UseInvaderFont( 26f )) {
			graphics.DrawString( "Salt", bigFont, backgroundBrush, topRect, alignCenter );
			graphics.DrawString( "Deposits", bigFont, backgroundBrush, botRect, alignCenter );
		}

		using Font descFont = resources.UseGameFont( 26f );
		int shiftDown = 40;
		topRect.Offset( 0, shiftDown );
		graphics.DrawString( "Explore/Build:\r\nNon-Mining", descFont, Brushes.Black, topRect, alignCenter );
		botRect.Offset( 0, shiftDown );
		graphics.DrawString( "Ravage: Mining", descFont, Brushes.Black, botRect, alignCenter );
	}

	static void DrawCoastal( InvaderCardResources resources, Graphics graphics, PointF[] perimeter, Brush backgroundBrush, Rectangle topRect, Rectangle botRect ) {

		// texture
		float tension = .15f;
		using( var terrainBrush = resources.UseTerrainBrush( Terrain.Ocean ) )
			graphics.FillClosedCurve( terrainBrush, perimeter, FillMode.Alternate, tension );
		using( Pen perimeterPen = new Pen( Color.Black, 3f ) )
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );
		using var bigFont = resources.UseInvaderFont( 25f );
		using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };
		graphics.DrawString( "Coastal", bigFont, backgroundBrush, topRect, alignCenter );
		graphics.DrawString( "Lands", bigFont, backgroundBrush, botRect, alignCenter );
	}

	static void DrawSingleTerrain( InvaderCard card, InvaderCardResources resources, Graphics graphics, PointF[] perimeter, Brush backgroundBrush, SingleTerrainFilter singleTerrain ) {
		Rectangle topRect = new Rectangle( 30, 45, 200 - 2 * 30, 160 - 60 );
		Rectangle botRect = new Rectangle( 30, 160 + 15, 200 - 2 * 30, 160 - 60 );

		Pen perimeterPen = new Pen( Color.Black, 3f );
		using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };

		// texture
		using Brush terrainBrush = resources.UseTerrainBrush( singleTerrain.Terrain );
		float tension = .15f;
		graphics.FillClosedCurve( terrainBrush, perimeter, FillMode.Alternate, tension );
		graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

		// Abreviation Text in the middle
		using Font bigFont = resources.UseInvaderFont( 60f );
		graphics.DrawString( singleTerrain.Terrain.ToString()[..1], bigFont, backgroundBrush, topRect, alignCenter );

		// Escalation
		if(card.TriggersEscalation) {
			var ellipseRect = botRect.InflateBy( -45, -20 );
			graphics.FillEllipse( backgroundBrush, ellipseRect );
			using Bitmap escalation = resources.InvaderCardImage( "escalation.png" );
			graphics.DrawImageFitHeight( escalation, ellipseRect.InflateBy( -5 ) );
		}
	}

	static void DrawDoubleTerrain( InvaderCardResources resources, Graphics graphics, PointF[] perimeter, Brush backgroundBrush, DoubleTerrainFilter doubleTerrain ) {

		Rectangle topRect = new Rectangle( 30, 30 + 15, 200 - 2 * 30, 160 - 60 );
		Rectangle botRect = new Rectangle( 30, 160 + 25, 200 - 2 * 30, 160 - 60 );

		Pen perimeterPen = new Pen( Color.Black, 3f );
		using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };

		float tension = .15f;
		int countPerSide = perimeter.Length / 2;
		using Brush brush1 = resources.UseTerrainBrush( doubleTerrain.Terrain1 );
		graphics.FillClosedCurve( brush1, perimeter.Take( countPerSide ).ToArray(), FillMode.Alternate, tension );
		graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );
		using Brush brush2 = resources.UseTerrainBrush( doubleTerrain.Terrain2 );
		graphics.FillClosedCurve( brush2, perimeter.Skip( countPerSide ).Take( countPerSide ).ToArray(), FillMode.Alternate, tension );
		graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );
		using Font bigFont = resources.UseInvaderFont( 60f );
		// Text
		graphics.DrawString( doubleTerrain.Terrain1.ToString()[..1], bigFont, backgroundBrush, topRect, alignCenter );
		graphics.DrawString( doubleTerrain.Terrain2.ToString()[..1], bigFont, backgroundBrush, botRect, alignCenter );
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
