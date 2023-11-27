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
		DrawRangeSource( graphics, power, cells[1] );
		DrawTarget( graphics, power.TargetFilter, cells[2] );
	}

	static void DrawSpeed( Graphics graphics, IFlexibleSpeedActionFactory power, Rectangle cell ) {
		using Image speedImg = ResourceImages.Singleton.GetImage( power.DisplaySpeed == Phase.Slow ? Img.Icon_Slow : Img.Icon_Fast );
		graphics.DrawImageFitHeight( speedImg, cell.InflateBy( (int)(-cell.Height * .1f) ) );
	}

	#region Range (Center)

	static void DrawRangeSource( Graphics graphics, IFlexibleSpeedActionFactory power, Rectangle cell ) {
		string rangeText = power.RangeText;

		if(rangeText == "-") {
			// draw dash
			using Bitmap img = ResourceImages.Singleton.GetResourceImage( "icons.No_Range.png" );
			graphics.DrawImageFitBoth( img, cell.InflateBy( -cell.Width / 4, 0 ) );
			return;
		}

		string[] parts = rangeText.Split( ':' );
		int width = cell.Width * 2 / 5;
		Rectangle drawRect = new Rectangle( cell.Left + (cell.Width - width) / 2, cell.Top, width, cell.Height );

		if(parts.Length == 1) {
			DrawRange( graphics, cell, text:rangeText, drawRect ); // do last since Parts==2 adjusts it
			return;
		}

		if(parts.Length == 2) {
			DrawRange( graphics, cell, text:parts[0], drawRect.OffsetBy( width / 2, 0 ) ); // do last since Parts==2 adjusts it
			DrawSourceCriteria( graphics, cell, parts[1], drawRect.InflateBy( -cell.Height / 10 ).OffsetBy( -width/2, 0 ) );
		}


	}

	private static void DrawSourceCriteria( Graphics graphics, Rectangle cell, string sourceCriteria, Rectangle imgRect ) {
		if(sourceCriteria == "ss") {
			// Sacred site
			using(Bitmap ssImg = ResourceImages.Singleton.GetImage( Img.Icon_Sacredsite ))
				graphics.DrawImageFitBoth( ssImg, imgRect );
			return;
		}

		// Draw Terrain 1st/background
		DrawSourceTerrain( graphics, sourceCriteria, imgRect.OffsetBy( -cell.Height/4, 0 ) ); // .SplitHorizontally( 2 )[0]
		// Draw Presence 2nd on top of Terrain
		using( Image presenceImg = ResourceImages.Singleton.GetImage( Img.Icon_Presence ))
			graphics.DrawImageFitBoth( presenceImg, imgRect.OffsetBy( 0, cell.Height / 4 ).InflateBy( -cell.Height / 7 ) );
	}

	static void DrawSourceTerrain( Graphics graphics, string sourceCriteria, Rectangle terrainRect ) {
		string[] parts = sourceCriteria.Split( ',' );
		Rectangle[] rects = terrainRect.SplitHorizontally(parts.Length);
		for(int i = 0; i < rects.Length; ++i) {
			using(Image terrainImg = ResourceImages.Singleton.GetImage( TargetToImg( parts[i] ) ))
				graphics.DrawImageFitHeight( terrainImg, rects[i] );
		}
		//Img terrain = TargetToImg( sourceCriteria );
		//using(Image terrainImg = ResourceImages.Singleton.GetImage( terrain ))
		//	graphics.DrawImageFitHeight( terrainImg, terrainRect );
	}

	static void DrawRange( Graphics graphics, Rectangle cell, string text, Rectangle rangeRect ) {
		Rectangle[] rects = rangeRect.SplitVerticallyAt( .75f );
		using Font regularFont = new Font( FontFamily.GenericSansSerif, cell.Height * .7f, FontStyle.Regular, GraphicsUnit.Pixel );
		graphics.DrawStringCenter( text, regularFont, Brushes.Black, rects[0] );
		rects[1].Offset( 0, -cell.Height / 10 ); // get arrow off of the bottom bounds
		using Bitmap arrow = ResourceImages.Singleton.GetImage( Img.RangeArrow );
		graphics.DrawImageFitBoth( arrow, rects[1] );
	}

	#endregion

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
		Target.Jungle      => Img.Icon_Jungle,
		Target.Sands       => Img.Icon_Sands,
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
		Target.Invaders    => Img.Icon_Invaders,
		Target.Strife      => Img.Icon_Strife,
		"Incarna"          => Img.Icon_Incarna,
		_                  => Img.None
	};

}
