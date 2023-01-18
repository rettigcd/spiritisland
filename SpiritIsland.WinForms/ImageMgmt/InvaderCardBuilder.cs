using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SpiritIsland.WinForms;

class InvaderCardBuilder {
	static public Bitmap BuildInvaderCard( InvaderCard card ) {

		Bitmap bitmap = new Bitmap( 200, 320 );
		Graphics graphics = Graphics.FromImage( bitmap );
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

		// Draw perimeter and inner texture
		if(card.Filter is SingleTerrainFilter singleTerrain) {
			Rectangle topRect = new Rectangle( 30, 45, 200 - 2 * 30, 160 - 60 );
			Rectangle botRect = new Rectangle( 30, 160 + 15, 200 - 2 * 30, 160 - 60 );

			// texture
			using Brush terrainBrush = UseTerrainBrush( singleTerrain.Terrain );
			float tension = .15f;
			graphics.FillClosedCurve( terrainBrush, perimeter, FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

			// Abreviation Text in the middle
			using Font bigFont = UseInvaderFont( 60f );
			graphics.DrawStringCenter( singleTerrain.Terrain.ToString()[..1], bigFont, backgroundBrush, topRect );

			// Escalation
			if(card.HasEscalation) {
				var ellipseRect = botRect.InflateBy( -45, -20 );
				graphics.FillEllipse( backgroundBrush, ellipseRect );
				using Bitmap escalation = ResourceImages.Singleton.GetInvaderCard( "escalation.png" );
				graphics.DrawImageFitHeight( escalation, ellipseRect.InflateBy( -5 ) );
			}

		} else if(card.Filter is DoubleTerrainFilter doubleTerrain) {
			Rectangle topRect = new Rectangle( 30, 30 + 15, 200 - 2 * 30, 160 - 60 );
			Rectangle botRect = new Rectangle( 30, 160 + 25, 200 - 2 * 30, 160 - 60 );

			float tension = .15f;
			int countPerSide = perimeter.Length / 2;

			// texture - 1
			using Brush brush1 = UseTerrainBrush( doubleTerrain.Terrain1 );
			graphics.FillClosedCurve( brush1, perimeter.Take( countPerSide ).ToArray(), FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );
			// texture - 2
			using Brush brush2 = UseTerrainBrush( doubleTerrain.Terrain2 );
			graphics.FillClosedCurve( brush2, perimeter.Skip( countPerSide ).Take( countPerSide ).ToArray(), FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

			using Font bigFont = UseInvaderFont( 60f );
			// Text
			graphics.DrawStringCenter( doubleTerrain.Terrain1.ToString()[..1], bigFont, backgroundBrush, topRect );
			graphics.DrawStringCenter( doubleTerrain.Terrain2.ToString()[..1], bigFont, backgroundBrush, botRect );

		} else {

			// must be coastal
			Rectangle topRect = new Rectangle( 30, 30 + 30, 200 - 2 * 30, 160 - 60 );
			Rectangle botRect = new Rectangle( 30, 160 + 20 + 15, 200 - 2 * 30, 160 - 60 );

			// texture
			using Brush terrainBrush = UseTerrainBrush( Terrain.Ocean );
			float tension = .15f;
			graphics.FillClosedCurve( terrainBrush, perimeter, FillMode.Alternate, tension );
			graphics.DrawClosedCurve( perimeterPen, perimeter, tension, FillMode.Alternate );

			// Abreviation Text in the middle
			using Font bigFont = UseInvaderFont( 25f );
			graphics.DrawStringCenter( "Coastal", bigFont, backgroundBrush, topRect );
			graphics.DrawStringCenter( "Lands", bigFont, backgroundBrush, botRect );
		}
		Font bottomFont = UseInvaderFont( 20f );
		graphics.DrawStringCenter(
			card.InvaderStage switch { 1 => "I", 2 => "II", 3 => "III", _ => "" },
			bottomFont, Brushes.Brown,
			new RectangleF( 0, 285, 200, 30 )
		);
		return bitmap;
	}
	static Brush UseTerrainBrush( Terrain terrain ) => ResourceImages.Singleton.UseTerrainBrush( terrain );
	static Font UseInvaderFont( float fontHeight ) => ResourceImages.Singleton.UseInvaderFont( fontHeight );
}
