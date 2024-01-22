using System.Drawing;

namespace SpiritIsland;

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
		using Image speedImg = ResourceImages.Singleton.GetImg( power.DisplaySpeed == Phase.Slow ? Img.Icon_Slow : Img.Icon_Fast );
		graphics.DrawImageFitHeight( speedImg, cell.InflateBy( (int)(-cell.Height * .1f) ) );
	}

	#region Range (Center)

	static void DrawRangeSource( Graphics graphics, IFlexibleSpeedActionFactory power, Rectangle cell ) {
		string rangeText = power.RangeText;

		if(rangeText == "-") {
			// draw dash
			using Bitmap img = ResourceImages.Singleton.GetImg(Img.NoRange);
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
			using Bitmap ssImg = ResourceImages.Singleton.GetImg( Img.Icon_Sacredsite );
			graphics.DrawImageFitBoth( ssImg, imgRect );
			return;
		}

		// Draw Terrain 1st/background
		DrawSourceTerrain( graphics, sourceCriteria, imgRect.OffsetBy( -cell.Height/4, 0 ) ); // .SplitHorizontally( 2 )[0]
		// Draw Presence 2nd on top of Terrain
		using Image presenceImg = ResourceImages.Singleton.GetImg( Img.Icon_Presence );
		graphics.DrawImageFitBoth( presenceImg, imgRect.OffsetBy( 0, cell.Height / 4 ).InflateBy( -cell.Height / 7 ) );
	}

	static void DrawSourceTerrain( Graphics graphics, string sourceCriteria, Rectangle terrainRect ) {
		string[] parts = sourceCriteria.Split( ',' );
		Rectangle[] rects = terrainRect.SplitHorizontally(parts.Length);
		for(int i = 0; i < rects.Length; ++i)
			using(Image terrainImg = ResourceImages.Singleton.GetImg( TargetToImg( parts[i] ) ))
				graphics.DrawImageFitHeight( terrainImg, rects[i] );
	}

	static void DrawRange( Graphics graphics, Rectangle cell, string text, Rectangle rangeRect ) {
		Rectangle[] rects = rangeRect.SplitVerticallyAt( .75f );
		using Font regularFont = new Font( FontFamily.GenericSansSerif, cell.Height * .7f, FontStyle.Regular, GraphicsUnit.Pixel );
		graphics.DrawStringCenter( text, regularFont, Brushes.Black, rects[0] );
		rects[1].Offset( 0, -cell.Height / 10 ); // get arrow off of the bottom bounds
		using Bitmap arrow = ResourceImages.Singleton.GetImg( Img.RangeArrow );
		graphics.DrawImageFitBoth( arrow, rects[1] );
	}

	#endregion

	static void DrawTarget( Graphics graphics, string text, Rectangle cell, Align align = Align.Center ) {

		if(text[0] == '2') {
			int index = text[1] == ' ' ? 2 : 1;
			string sub = text[index..];
			Draw2SideBySide( graphics, cell, sub, sub );
			return;
		}

		int orIndex = text.IndexOf( '/' );
		if(orIndex != -1) {
			Draw2SideBySide( graphics, cell, text[..orIndex], text[(orIndex + 1)..] );
			return;
		}

		int andIndex = text.IndexOf( '+' );
		if(andIndex != -1) {
			Draw2SideBySide( graphics, cell, text[..andIndex], text[(andIndex + 1)..] );
			return;
		}

		Draw1TargetImage( graphics, text, cell, align );

	}

	static void Draw1TargetImage( Graphics graphics, string text, Rectangle cell, Align align ) {
		bool isNot = text.StartsWith( "No" );
		if(isNot) text = text.Split( ' ' )[1]; // just 2nd word

		Img simpleIcon = TargetToImg( text );
		Rectangle imgRect = cell.InflateBy( -cell.Height / 10 );
		if(simpleIcon != Img.None) {

			// Simple Icon
			using Bitmap icon = ResourceImages.Singleton.GetImg( simpleIcon );
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

				case Filter.Inland:
				case Filter.Coastal:
				case Filter.Any:
				default: {
						using StringFormat centerBoth = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
						using Font boldFont = new Font( FontFamily.GenericSansSerif, cell.Height / 2, FontStyle.Bold, GraphicsUnit.Pixel );
						graphics.DrawString( text.ToUpper(), boldFont, Brushes.Black, cell, centerBoth );
					}
					break;
			}

		}

		if(isNot)
			using(Bitmap icon = ResourceImages.Singleton.GetImg(Img.NoX))
				graphics.DrawImage( icon, imgRect.FitHeight( icon.Size, align ) );
	}

	private static void Draw2SideBySide( Graphics graphics, Rectangle cell, string s1, string s2 ) {
		// Split into 2 parts and do each
		Rectangle[] cellParts = cell.SplitHorizontally( 2 );
		DrawTarget( graphics, s1, cellParts[0], Align.Far );
		DrawTarget( graphics, s2, cellParts[1], Align.Near );
	}

	static Img TargetToImg( string text ) => text switch {
		"Spirit" => Img.Icon_Spirit,
		Filter.Jungle      => Img.Icon_Jungle,
		Filter.Sands       => Img.Icon_Sands,
		Filter.Mountain    => Img.Icon_Mountain,
		Filter.Wetland     => Img.Icon_Wetland,
		Filter.Ocean       => Img.Icon_Ocean,
		Filter.Blight      => Img.Icon_Blight,
		Filter.Dahan       => Img.Icon_Dahan,
		Filter.City        => Img.Icon_City,
		Filter.Town        => Img.Icon_Town,
		Filter.Disease     => Img.Icon_Disease,
		Filter.Wilds       => Img.Icon_Wilds,
		Filter.Beast       => Img.Icon_Beast,
		Filter.Presence    => Img.Icon_Presence,
		Filter.EndlessDark => Img.Icon_EndlessDark,
		Filter.Quake       => Img.Icon_Quake,
		Filter.Invaders    => Img.Icon_Invaders,
		Filter.Strife      => Img.Icon_Strife,
		"Incarna"          => Img.Icon_Incarna,
		_                  => Img.None
	};

}
