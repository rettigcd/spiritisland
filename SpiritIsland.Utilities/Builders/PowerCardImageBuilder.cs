using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text.RegularExpressions;

namespace SpiritIsland;

/// <summary>
/// The resources necessary to build a power card image.
/// </summary>
public interface PowerCardResources {
	Bitmap GetImg( Img img );
	Task<Image> GetPowerCardImage( PowerCard card );
	Bitmap CardTexture( string texture );
	Font UseGameFont( float fontHeight );
	Bitmap GetPhaseCost( Phase phase );
}

public partial class PowerCardImageBuilder {

	static public async Task<Image> Build( PowerCard card, PowerCardResources resources ) {

		var bounds = new Rectangle( 0, 0, 300, 420 );
		var innerRect = bounds.InflateBy( -10 ); // 280x400
		var left = new Rectangle( innerRect.X, innerRect.Y, 50, innerRect.Height ); // 50x400
		var right = new Rectangle( left.Right, innerRect.Y, innerRect.Width - left.Width, innerRect.Height ); // 230x400

		var (title, (imgRect, (headerRect, (textArea, (artistFooter, _))))) 
			= right.SplitVerticallyByHeights( 42, 150, 25, 172, 11 );

		var img = new Bitmap( bounds.Width, bounds.Height );
		using Graphics graphics = Graphics.FromImage( img );
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

		// Perimeter - DK Brown
		using SolidBrush borderBrush = new SolidBrush( Color.FromArgb( 69, 42, 21 ) );
		graphics.FillPath( borderBrush, bounds.RoundCorners( 20 ) );
		// Texture background
		using Image background = resources.CardTexture( "power_weave.png" );
		using TextureBrush backgroundBrush = new TextureBrush( background );
		graphics.FillPath( backgroundBrush, innerRect.RoundCorners( 20 ) );

		// Elements
		for(int i = 0; i < ElementList.AllElements.Length; ++i) {
			Element el = ElementList.AllElements[i];
			if(0 < card.Elements[el]) {
				using Bitmap elImage = resources.GetImg( el.GetTokenImg() );
				graphics.DrawImage( elImage, left.X + 6, 68 + 41 * i, 36, 36 );
			}
		}

		// Title
		using(StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
		using(Font titleFont = new Font( "Arial Narrow", 16f, FontStyle.Bold, GraphicsUnit.Pixel ))
			graphics.DrawString( card.Name.ToUpper(), titleFont, Brushes.White, title, center );

		// Image

		using(Image artwork = await resources.GetPowerCardImage( card ))
			graphics.DrawImage( artwork, imgRect );

		// Header
		PowerHeaderDrawer.AttributeValuesRow( card ).Paint( graphics, headerRect );

		// Instructions
		PaintInstructionArea( card.Instructions, textArea, graphics, Brushes.Cornsilk );

		// Artist
		using(Bitmap paletteImg = resources.GetImg(Img.ArtistPalette)) {
			Rectangle paletteRect = artistFooter.FitHeight(paletteImg.Size,Align.Near);
			graphics.DrawImage(paletteImg, paletteRect);
			artistFooter.Offset(paletteRect.Width+2,0);
		}
		using(Font artistFont = new Font( "Arial Narrow", artistFooter.Height, FontStyle.Bold, GraphicsUnit.Pixel ))
			graphics.DrawString( card.Artist, artistFont, Brushes.Cornsilk, artistFooter );

		// Blue perimeter line
		using Pen borderPen = new Pen( Color.DeepSkyBlue, 1f );
		graphics.DrawPath( borderPen, innerRect.RoundCorners( 20 ) );

		// Cost
		Rectangle costRect = new Rectangle( innerRect.X - 7, innerRect.Y - 7, 60, 60 );

		using(Bitmap costImg = resources.GetPhaseCost(card.DisplaySpeed))
			graphics.DrawImage( costImg, costRect );

		costRect.Offset( 0, 3 );
		using(StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
		using(Font energyFont = resources.UseGameFont( 28f ))
			graphics.DrawString( card.Cost.ToString(), energyFont, Brushes.White, costRect, center );

		return img;
	}

	static List<string> SplitSentences( string paragraph ) {

		// Every -or- is preceeded by a '.' so this puts -or- on their own line.
		const string origOr = "-or- ";
		const string newOr = "-or-. ";
		paragraph = paragraph.Replace( origOr, newOr );

		var sentences = new List<string>();
		int start = 0;
		int end;
		while((end = NextEnd( paragraph, start )) != 0) {
			// capture
			if(end < paragraph.Length && paragraph[end] == ' ')
				++end;

			string phrase = paragraph[start..end];
			sentences.Add( phrase == newOr ? origOr : phrase );
			// next
			start = end;
		}
		if(start < paragraph.Length)
			sentences.Add( paragraph[start..] );

		return sentences;

		static int NextEnd( string paragraph, int start ) {
			return paragraph.IndexOf( ". ", start ) + 1;
		}
	}

	#region private methods

	static void PaintInstructionArea( string instructions, Rectangle textArea, Graphics graphics, Brush normalBackground ) {

		// background
		graphics.FillRectangle( normalBackground, textArea );

		// give it an upper and lower margin
		textArea.Inflate( 0, -10 );

		Match split = IfYouHaveRegex().Match( instructions );

		if(split.Success) {
			string topText = instructions[..split.Index];
			string splitText = split.Groups[1].Value.Replace( ",", " " );//.Replace( " ", "" );
			string bottomText = instructions[(split.Index + split.Length)..];
			Paint2Parts( textArea, graphics, SplitSentences( topText ), splitText, SplitSentences( bottomText ) );
		} else
			Paint2Parts( textArea, graphics, SplitSentences( instructions ), null, [] );
	}

	static void Paint2Parts( Rectangle textBounds, Graphics graphics, List<string> topParts, string? splitText, List<string> botParts ) {

		var config = new ConfigWrappingLayout {
			EmSize = 13,
			ElementDimension = 15,
			IconDimension = 20,
			HorizontalAlignment = Align.Center
		};
		int lineHeight = 18;
		int widthReduction = 0;

		WrappingLayout layout = AppendToLayout( textBounds, graphics, splitText, config, lineHeight, topParts, botParts, widthReduction );
		// Combine sentences until it fits
		while(textBounds.Height < layout.Size.Height && (1 < topParts.Count || 1 < botParts.Count)) {
			var reduceMe = topParts.Count < botParts.Count ? botParts : topParts;
			reduceMe[^2] += reduceMe[^1];
			reduceMe.RemoveAt(reduceMe.Count-1);
			layout = AppendToLayout( textBounds, graphics, splitText, config, lineHeight, topParts, botParts, widthReduction );
		}
		// FONT SIZE - Reduce font until it fits
		while(textBounds.Height < layout.Size.Height) {
			--config.EmSize;
			if(config.EmSize < 8) throw new Exception("something is wrong, we shouldn't have to reduce text smaller than 8px");
			layout = AppendToLayout( textBounds, graphics, splitText, config, lineHeight, topParts, botParts, widthReduction );
		}

		// No we have something that works

		// !!! Try breaking on commas(',') that don't increase line count

		// MARGINS - Give it margins but no more than 4 lines
		const int widthReductionStep = 5;
		WrappingLayout narrowLayout = AppendToLayout( textBounds, graphics, splitText, config, lineHeight, topParts, botParts, widthReduction + widthReductionStep );
		while((layout.LineCount<4 || narrowLayout.LineCount == layout.LineCount) && widthReduction < 50) {
			layout = narrowLayout;
			widthReduction += widthReductionStep;
			narrowLayout = AppendToLayout( textBounds, graphics, splitText, config, lineHeight, topParts, botParts, widthReduction + widthReductionStep );
		}

		// LINE HEIGHT - Spread out out lines  vertically (up to 1.5x original size)
		if(1 < layout.LineCount) { 
			const int verticalMargin = 15;
			int extraRowHeight = Math.Min(lineHeight/2, (textBounds.Height-layout.Size.Height- verticalMargin) /(layout.LineCount-1));
			if( 0 < extraRowHeight) {
				lineHeight += extraRowHeight;
				layout = AppendToLayout( textBounds, graphics, splitText, config, lineHeight, topParts, botParts, widthReduction );
			}
		}

		layout.Adjust( textBounds.X, textBounds.Y );
		Point centeringAdj = layout.CenterDrawingSpace( textBounds.Size );

		if(splitText != null) {
			int botLineCount = AppendToLayout( textBounds, graphics, null, config, lineHeight, botParts, [], widthReduction ).LineCount;
			int bottomHeight = (botLineCount + 1) * lineHeight +centeringAdj.Y;
			Rectangle botRect = new Rectangle( textBounds.X, textBounds.Bottom-bottomHeight, textBounds.Width, bottomHeight );
			graphics.FillRectangle( Brushes.Bisque, botRect );
		}

		layout.Paint( graphics );
	}

	static WrappingLayout AppendToLayout( Rectangle textBounds, Graphics graphics, string? splitText, ConfigWrappingLayout config, int rowHeight, List<string> topParts, List<string> botParts, int widthReduction ) {
		Size rowSize = new Size( textBounds.Width - widthReduction, rowHeight );
		WrappingLayout layout = new WrappingLayout( config, rowSize, graphics );

		// top
		foreach(string sentence in topParts)
			layout.AppendLine( sentence, FontStyle.Regular );
		// bottom
		if(splitText != null) {
			layout.Append( splitText, FontStyle.Regular ); layout.LineComplete();
			foreach(string sentence in botParts)
				layout.AppendLine( sentence, FontStyle.Regular );
		}

		layout.FinalizeBounds();
		return layout;
	}

	[GeneratedRegex( @"-If you have- ([^:]+):" )]
	static private partial Regex IfYouHaveRegex();

	#endregion

}
