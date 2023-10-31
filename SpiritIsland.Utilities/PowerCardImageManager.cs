using SpiritIsland.WinForms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text.RegularExpressions;

namespace SpiritIsland.Utilities.ImageMgmt;

public class PowerCardImageManager {

	public static async Task<Image> GetImage( PowerCard card ) {
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
		using Image background = Image.FromFile( $".\\images\\texture1.png" );
		using TextureBrush backgroundBrush = new TextureBrush( background );
		graphics.FillPath( backgroundBrush, innerRect.RoundCorners( 20 ) );

		// Elements
		for(int i = 0; i < ElementList.AllElements.Length; ++i) {
			Element el = ElementList.AllElements[i];
			if(0 < card.Elements[el]) {
				using Bitmap elImage = ResourceImages.Singleton.GetImage( el );
				graphics.DrawImage( elImage, left.X + 6, 68 + 41 * i, 36, 36 );
			}
		}

		// Title
		using(StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
		using(Font titleFont = new Font( "Arial Narrow", 16f, FontStyle.Bold, GraphicsUnit.Pixel ))
			graphics.DrawString( card.Name.ToUpper(), titleFont, Brushes.White, title, center );

		// Image

		using(Image artwork = await ResourceImages.Singleton.CardCardImage( card ))
			graphics.DrawImage( artwork, imgRect );

		// Header
		AttributeValues( graphics, headerRect.SplitHorizontally( 3 ), card );

		// Instructions
		PaintInstructionArea( card.Instructions, textArea, graphics );

		// Artist
		// graphics.DrawRectangle( Pens.White, artistFooter  );

		// Blue perimeter line
		using Pen borderPen = new Pen( Color.DeepSkyBlue, 1f );
		graphics.DrawPath( borderPen, innerRect.RoundCorners( 20 ) );

		// Cost
		Rectangle costRect = new Rectangle( innerRect.X - 7, innerRect.Y - 7, 60, 60 );
		string speedStr = card.DisplaySpeed == Phase.Fast ? "Fast" : "Slow";
		using(Bitmap costImg = ResourceImages.Singleton.GetResourceImage( $"tokens.Cost{speedStr}.png" ))
			graphics.DrawImage( costImg, costRect );
		costRect.Offset( 0, 3 );
		using(StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
		using(Font energyFont = ResourceImages.Singleton.UseGameFont( 28f ))
			graphics.DrawString( card.Cost.ToString(), energyFont, Brushes.White, costRect, center );

		return img;
	}

	static void PaintInstructionArea( string instructions, Rectangle textArea, Graphics graphics ) {

		// background
		graphics.FillRectangle( Brushes.Cornsilk, textArea );

		Match split = new Regex( @"-If you have- ([^:]+):" ).Match( instructions );

		if(split.Success) {
			string topText = instructions[..split.Index];
			string splitText = split.Groups[1].Value.Replace( ",", "" ).Replace( " ", "" );
			string bottomText = instructions[(split.Index + split.Length)..];
			Paint2Parts( textArea, graphics, SplitSentences( topText ), splitText, SplitSentences( bottomText ) );
		} else
			Paint2Parts( textArea, graphics, SplitSentences( instructions ), null, new List<string>() );
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
			reduceMe[reduceMe.Count-2] += reduceMe[reduceMe.Count - 1];
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
			int botLineCount = AppendToLayout( textBounds, graphics, null, config, lineHeight, botParts, new List<string>(), widthReduction ).LineCount;
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

	public static List<string> SplitSentences(string paragraph ) {

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

	static public void AttributeValues( Graphics graphics, Rectangle[] cells, IFlexibleSpeedActionFactory power ) {

		foreach(var cell in cells)
			graphics.FillRectangle( Brushes.BlanchedAlmond, cell );

		// Divider lines
		Rectangle c = cells[1];
		graphics.DrawLine( Pens.SaddleBrown, c.Left, c.Top, c.Left, c.Bottom );
		graphics.DrawLine( Pens.SaddleBrown, c.Right, c.Top, c.Right, c.Bottom );

		// Content 
		DrawSpeed( graphics, power, cells[0] );
		DrawRange( graphics, power, cells[1] );
		DrawTarget( graphics, power, cells[2] );
	}

	static void DrawTarget( Graphics graphics, IFlexibleSpeedActionFactory power, Rectangle cell ) {
		DrawTarget(graphics, power.TargetFilter, cell );
	}

	static void DrawTarget( Graphics graphics, string text, Rectangle cell, Align align = Align.Center ) {

		int orIndex = text.IndexOf(" Or ");
		if(orIndex != -1) {
			// Split into 2 parts and do each
			Rectangle[] cellParts = cell.SplitHorizontally(2);
			DrawTarget( graphics, text[..orIndex], cellParts[0], Align.Far );
			DrawTarget( graphics, text[(orIndex+4)..], cellParts[1], Align.Near );
			return;
		}

		bool isNot = text.StartsWith("No");
		if(isNot) text = text.Split(' ')[1]; // just 2nd word

		Img simpleIcon = TargetToImg(text);
		Rectangle imgRect = cell.InflateBy( -cell.Height / 10 );
		if(simpleIcon != Img.None) {

			// Simple Icon
			using Bitmap icon = ResourceImages.Singleton.GetImage( simpleIcon );
			graphics.DrawImage( icon, imgRect.FitHeight( icon.Size, align ) );

		} else {

			// Text
			switch(text) { // !!! coordinate this with GrowthPainter

				case AnySpiritAttribute.TargetFilterText: {
						Rectangle[] cellParts = cell.SplitHorizontally(2);
						DrawTarget( graphics, "Any", cellParts[0], Align.Far );
						DrawTarget( graphics, "Spirit", cellParts[1], Align.Near );
					}
					break;
                case AnotherSpiritAttribute.TargetFilterText: {
						Rectangle[] cellParts = cell.SplitHorizontallyByWeight( 0, .65f, .25f );
						DrawTarget( graphics, "Another", cellParts[0].InflateBy(0,-cell.Height/8), Align.Near );
						DrawTarget( graphics, "Spirit", cell, Align.Far );
					}
					break;

				case Target.TwoBeasts:
				case Target.BlightAndInvaders:
				case Target.TwoBeastPlusInvaders:

				case Target.Inland:
				case Target.Coastal:
				case Target.Invaders:
				case Target.Any:
				default: {
						using StringFormat centerBoth = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
						using Font boldFont = new Font( FontFamily.GenericSansSerif, cell.Height / 2, FontStyle.Bold, GraphicsUnit.Pixel );
						graphics.DrawString( text.ToUpper(), boldFont, Brushes.Black, cell, centerBoth );
					}
					break;
			}

		}

		if(isNot)
			using(Bitmap icon = ResourceImages.Singleton.GetNoSymbol())
				graphics.DrawImage( icon, imgRect.FitHeight( icon.Size, align ) );

	}

	static Img TargetToImg(string text) => text switch { 
		"Spirit"           => Img.Icon_Spirit,
		Target.Jungle      => Img.Icon_Jungle,
		Target.Sand        => Img.Icon_Sand,
		Target.Mountain    => Img.Icon_Mountain,
		Target.Wetland     => Img.Icon_Wetland,
		Target.Ocean       => Img.Icon_Ocean,
		Target.Blight      => Img.Icon_Blight,
		Target.Dahan       => Img.Icon_Dahan,
		Target.City        => Img.Icon_City,
		Target.Town        => Img.Icon_Town,
		Target.Disease     => Img.Icon_Disease,
		Target.Wilds       => Img.Icon_Wilds,
		Target.Beast       => Img.Icon_Beast,
		Target.Presence    => Img.Icon_Presence,
		Target.EndlessDark => Img.Icon_EndlessDark,
		Target.Quake       => Img.Icon_Quake,
		"Incarna"          => Img.Icon_Incarna,
		_                  => Img.None
	};

	static void DrawSpeed( Graphics graphics, IFlexibleSpeedActionFactory power, Rectangle cell ) {
		using Image speedImg = ResourceImages.Singleton.GetImage( power.DisplaySpeed == Phase.Slow ? Img.Icon_Slow : Img.Icon_Fast );
		graphics.DrawImageFitHeight( speedImg, cell.InflateBy( (int)(-cell.Height * .1f) ) );
	}

	static void DrawRange( Graphics graphics, IFlexibleSpeedActionFactory power, Rectangle cell ) {
		string rangeText = power.RangeText;

		if(rangeText == "-") {
			// draw dash
			using Bitmap img = ResourceImages.Singleton.GetResourceImage( "icons.No_Range.png" );
			graphics.DrawImageFitBoth( img, cell.InflateBy( -cell.Width / 4, 0 ) );
		} else {
			string[] parts = rangeText.Split( ':' );
			int width = cell.Width*2/5;
			Rectangle rangeRect = new Rectangle(cell.Left+(cell.Width-width)/2,cell.Top,width,cell.Height);

			if(parts.Length == 2) {

				// draw ss / terrain
				rangeRect.Offset(width/2,0);
				Rectangle imgRect = rangeRect.InflateBy( -cell.Height / 10 ); 
				imgRect.Offset(-width,0);

				if(parts[1]=="ss")
					using(Bitmap ssImg = ResourceImages.Singleton.GetImage(Img.Icon_Sacredsite))
						graphics.DrawImageFitBoth( ssImg, imgRect/*.InflateBy(-cell.Height/8)*/ );
				else {
					var terrainSplit = imgRect.SplitHorizontally( 2 );
					Img terrain = TargetToImg( parts[1] ); //  ParseTerrain( parts[1] );
					using(Image terrainImg = ResourceImages.Singleton.GetImage(terrain))
						graphics.DrawImageFitHeight( terrainImg, terrainSplit[0] );
					imgRect.Offset( 0, cell.Height/4 );
					using Image presenceImg = ResourceImages.Singleton.GetImage( Img.Icon_Presence );
					graphics.DrawImageFitBoth( presenceImg, imgRect.InflateBy( -cell.Height / 7 ) );
				}

			}

			// draw range
			Rectangle[] rects = rangeRect.SplitVerticallyAt(.75f);
			using Font regularFont = new Font( FontFamily.GenericSansSerif, cell.Height*.7f, FontStyle.Regular, GraphicsUnit.Pixel );
			graphics.DrawStringCenter( parts[0], regularFont, Brushes.Black, rects[0] );
			rects[1].Offset(0,-cell.Height/10); // get arrow off of the bottom bounds
			using Bitmap arrow = ResourceImages.Singleton.GetImage( Img.RangeArrow );
			graphics.DrawImageFitBoth( arrow, rects[1] );

		}
	}

}
