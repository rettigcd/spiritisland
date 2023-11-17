using SpiritIsland.WinForms;
using System.Drawing;

namespace SpiritIsland.Utilities.ImageMgmt;

/// <summary>
/// Draws: Speed, Range, Target head attributes on a given Graphics object.
/// </summary>
public static class PowerHeaderDrawer {

	static public void DrawAttributeValues( Graphics graphics, Rectangle[] cells, IFlexibleSpeedActionFactory power ) {

		foreach(var cell in cells)
			graphics.FillRectangle( Brushes.BlanchedAlmond, cell );

		// Divider lines
		Rectangle c = cells[1];
		graphics.DrawLine( Pens.SaddleBrown, c.Left, c.Top, c.Left, c.Bottom );
		graphics.DrawLine( Pens.SaddleBrown, c.Right, c.Top, c.Right, c.Bottom );

		// Content 
		DrawSpeed( graphics, power, cells[0] );
		DrawRange( graphics, power, cells[1] );
		DrawTarget( graphics, power.TargetFilter, cells[2] );
	}

	#region private

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
			int width = cell.Width * 2 / 5;
			Rectangle rangeRect = new Rectangle( cell.Left + (cell.Width - width) / 2, cell.Top, width, cell.Height );

			if(parts.Length == 2) {

				// draw ss / terrain
				rangeRect.Offset( width / 2, 0 );
				Rectangle imgRect = rangeRect.InflateBy( -cell.Height / 10 );
				imgRect.Offset( -width, 0 );

				if(parts[1] == "ss")
					using(Bitmap ssImg = ResourceImages.Singleton.GetImage( Img.Icon_Sacredsite ))
						graphics.DrawImageFitBoth( ssImg, imgRect/*.InflateBy(-cell.Height/8)*/ );
				else {
					var terrainSplit = imgRect.SplitHorizontally( 2 );
					Img terrain = TargetToImg( parts[1] ); //  ParseTerrain( parts[1] );
					using(Image terrainImg = ResourceImages.Singleton.GetImage( terrain ))
						graphics.DrawImageFitHeight( terrainImg, terrainSplit[0] );
					imgRect.Offset( 0, cell.Height / 4 );
					using Image presenceImg = ResourceImages.Singleton.GetImage( Img.Icon_Presence );
					graphics.DrawImageFitBoth( presenceImg, imgRect.InflateBy( -cell.Height / 7 ) );
				}

			}

			// draw range
			Rectangle[] rects = rangeRect.SplitVerticallyAt( .75f );
			using Font regularFont = new Font( FontFamily.GenericSansSerif, cell.Height * .7f, FontStyle.Regular, GraphicsUnit.Pixel );
			graphics.DrawStringCenter( parts[0], regularFont, Brushes.Black, rects[0] );
			rects[1].Offset( 0, -cell.Height / 10 ); // get arrow off of the bottom bounds
			using Bitmap arrow = ResourceImages.Singleton.GetImage( Img.RangeArrow );
			graphics.DrawImageFitBoth( arrow, rects[1] );

		}
	}

	static void DrawTarget( Graphics graphics, string text, Rectangle cell, Align align = Align.Center ) {

		int orIndex = text.IndexOf( " Or " );
		if(orIndex != -1) {
			// Split into 2 parts and do each
			Rectangle[] cellParts = cell.SplitHorizontally( 2 );
			DrawTarget( graphics, text[..orIndex], cellParts[0], Align.Far );
			DrawTarget( graphics, text[(orIndex + 4)..], cellParts[1], Align.Near );
			return;
		}

		bool isNot = text.StartsWith( "No" );
		if(isNot) text = text.Split( ' ' )[1]; // just 2nd word

		Img simpleIcon = TargetToImg( text );
		Rectangle imgRect = cell.InflateBy( -cell.Height / 10 );
		if(simpleIcon != Img.None) {

			// Simple Icon
			using Bitmap icon = ResourceImages.Singleton.GetImage( simpleIcon );
			graphics.DrawImage( icon, imgRect.FitBoth( icon.Size, align ) );

		} else {

			// Text
			switch(text) { // !!! coordinate this with GrowthPainter

				case AnySpiritAttribute.TargetFilterText: {
						Rectangle[] cellParts = cell.SplitHorizontally( 2 );
						DrawTarget( graphics, "Any", cellParts[0], Align.Far );
						DrawTarget( graphics, "Spirit", cellParts[1], Align.Near );
					}
					break;
				case AnotherSpiritAttribute.TargetFilterText: {
						Rectangle[] cellParts = cell.SplitHorizontallyByWeight( 0, .65f, .25f );
						DrawTarget( graphics, "Another", cellParts[0].InflateBy( 0, -cell.Height / 8 ), Align.Near );
						DrawTarget( graphics, "Spirit", cell, Align.Far );
					}
					break;

				case Target.TwoBeasts:
				case Target.BlightAndInvaders:
				case Target.TwoBeastPlusInvaders:

				case Target.Inland:
				case Target.Coastal:
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

	static Img TargetToImg( string text ) => text switch {
		"Spirit" => Img.Icon_Spirit,
		Target.Jungle => Img.Icon_Jungle,
		Target.Sands => Img.Icon_Sands,
		Target.Mountain => Img.Icon_Mountain,
		Target.Wetland => Img.Icon_Wetland,
		Target.Ocean => Img.Icon_Ocean,
		Target.Blight => Img.Icon_Blight,
		Target.Dahan => Img.Icon_Dahan,
		Target.City => Img.Icon_City,
		Target.Town => Img.Icon_Town,
		Target.Disease => Img.Icon_Disease,
		Target.Wilds => Img.Icon_Wilds,
		Target.Beast => Img.Icon_Beast,
		Target.Presence => Img.Icon_Presence,
		Target.EndlessDark => Img.Icon_EndlessDark,
		Target.Quake => Img.Icon_Quake,
		Target.Invaders => Img.Icon_Invaders,
		"Incarna" => Img.Icon_Incarna,
		_ => Img.None
	};

	#endregion

}
