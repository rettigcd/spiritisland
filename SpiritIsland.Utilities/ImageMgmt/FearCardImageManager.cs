using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace SpiritIsland.WinForms; 

sealed public class FearCardImageManager {

	public static Image GetImage( IFearCard card ) {
		var bounds = new Rectangle(0,0,300,420);
		var innerBounds = bounds.InflateBy( -14 );
		var (titleArea,(canvasArea,_)) = innerBounds.SplitVerticallyAt(.11f);
		var (_,(fearBox1,(fearBox2,(fearBox3,_)))) = innerBounds.SplitVerticallyByWeight(0, .09f, .30f, .30f, .31f );
		
		// fear 1
		var (secRow1,(textArea1,_)) = fearBox1.SplitVerticallyByWeight(0, 4, 5 );
		var (secRow2,(textArea2,_)) = fearBox2.SplitVerticallyByWeight( 0, 4, 5 );
		var (secRow3,(textArea3,_)) = fearBox3.SplitVerticallyByWeight( 0, 4, 5 );


		var img = new Bitmap(bounds.Width,bounds.Height);
		using Graphics graphics = Graphics.FromImage( img );
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

		// Card background
		const int cornerRadius = 18;

		// Perimeter
		using(Bitmap cracks = ResourceImages.Singleton.Texture( "cracks.jpg" ))
			using(TextureBrush outerBrush = new TextureBrush( cracks ))
				graphics.FillPath( outerBrush, bounds.RoundCorners( 20 ) );

		// Bottom / Parchment
		var innerPath = innerBounds.RoundCorners( cornerRadius );
		using(Bitmap parchment = ResourceImages.Singleton.Texture( "parchment.jpg" ))
			using(TextureBrush parchmentBrush = new TextureBrush( parchment ))
				graphics.FillPath( parchmentBrush, innerPath );

		// Brown top - flowing
		using(Bitmap parchment = ResourceImages.Singleton.Texture( "flowing.jpg" ))
			using(TextureBrush parchmentBrush = new TextureBrush( parchment ))
				graphics.FillPath( parchmentBrush, titleArea.RoundCorners( cornerRadius, true, true, false, false ) );

		using(var brownPen = new Pen(Color.FromArgb(108,68,18), 2f ))
			graphics.DrawPath( brownPen, innerPath );

		// Card Title
		using(var titleFont = ResourceImages.Singleton.UseGameFont( 22f ))
		using(StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
			graphics.DrawString( card.Text.ToUpper(), titleFont, Brushes.White, titleArea, alignCenter );

		// Smoke
		var secTitleRects = new Rectangle[] { secRow1, secRow2, secRow3 };
		var colors = new HSL[] { new HSL( 47, 100, 40 ), new HSL( 13, 100, 40 ), new HSL( 0, 100, 30 ) };
		for(int i = 0; i < 3; ++i) {
			Rectangle rect = secTitleRects[i];
			var adjuster = new PixelAdjustment( new HslColorAdjuster( colors[i] ).GetNewColor );
			// Smoke
			using Bitmap smoke = ResourceImages.Singleton.Texture( "smoke.png" );
			adjuster.Adjust( (Bitmap)smoke );
			graphics.DrawImage( smoke, rect );
			// Terror Level icon
			using Bitmap terrorIcon = ResourceImages.Singleton.TerrorLevel( i + 1 );
			if(i==0) adjuster.Adjust( (Bitmap)terrorIcon ); // adjust the first icon, it is too light. others are ok.
			graphics.DrawImageFitHeight( terrorIcon, rect );
		}

		// Text 1..3
		Rectangle[] textAreas = new[] { textArea1, textArea2, textArea3 };
		for(int i = 0; i < 3; ++i)
			PaintTerrorLevelDetails( graphics, textAreas[i], card.GetDescription( i + 1 ) );

		return img;
	}

	static void PaintTerrorLevelDetails( Graphics graphics, Rectangle outterArea, string description ) {
		var config = new ConfigWrappingLayout {
			EmSize = 13,
			ElementDimension = 31,
			IconDimension = 20,
			HorizontalAlignment = Align.Center
		};

		int rowHeight = 18;
		Rectangle textBounds = outterArea.InflateBy( -10, 0 );

		// Try - reduced Width
		int widthReduction = 100;
		WrappingLayout layout;

		// as long as it takes up 3 lines, incrmentally make it wider
		do {

			// generate
			Size rowSize = new Size( textBounds.Width - widthReduction, rowHeight );
			layout = new WrappingLayout( config, rowSize, graphics );
			layout.Append( description, FontStyle.Regular );
			layout.FinalizeBounds();

			widthReduction -= 20;
		} while(0 < widthReduction && 2 < layout.LineCount);

		layout.Adjust( textBounds.X, textBounds.Y );
		layout.CenterDrawingSpace( textBounds.Size );

		layout.Paint( graphics );
	}
}
