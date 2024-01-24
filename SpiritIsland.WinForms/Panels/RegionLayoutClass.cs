using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

public class RegionLayoutClass {

	const float STATUSBAR_HEIGHT = .1f; // % of height to use for info-bar

	public static RegionLayoutClass ForIslandFocused( Rectangle bounds, int deckSlots ) {
		var x = new RegionLayoutClass( bounds );

		const int MARGIN = 10;
		const float SPIRIT_WIDTH = .35f; // % of screen width to use for spirit

		const float CARD_HEIGHT = .15f;
		const float MAIN_HEIGHT = .75f;
		Rectangle mainRect;
		Rectangle cardRow;
		(x.StatusRect, (mainRect, (cardRow, _))) = bounds.SplitVerticallyByWeight( 0, STATUSBAR_HEIGHT, MAIN_HEIGHT, CARD_HEIGHT ); // 0 margin because items in infoBar get their own margin.

		Rectangle spiritLimit;
		(x.IslandRect, (spiritLimit, _)) = mainRect.SplitHorizontallyByWeight( MARGIN, 1 - SPIRIT_WIDTH, SPIRIT_WIDTH );
		(x.OptionRect, _) = x.IslandRect.InflateBy( -MARGIN ).SplitHorizontallyByWeight( 0, .1f, .9f );

		// Don't let the spirit rect stretch to the bottom of the screen, make it 1.1 times higher than width
		x.SpiritRect = spiritLimit.FitBoth( spiritLimit.Width, (int)(spiritLimit.Width * 1.1), Align.Far, Align.Near );

		// Spirit Panel Parts
		Rectangle growthRow;
		(growthRow, (x.PresenceTractRect, (x.InnateRect, (x.ElementRect, _)))) = x.SpiritRect
		.InflateBy( -MARGIN )
			.SplitVerticallyByWeight( MARGIN,
				200f, // Growth
				300f, // Presence Tracks
				480f, // Innates
				60f   // elements
			);
		int imageWidth = growthRow.Height * 3 / 2;
		(x.SpiritImageBounds, (x.GrowthRect, _)) = growthRow.SplitHorizontallyByWeight( MARGIN, imageWidth, growthRow.Width - imageWidth - MARGIN );

		// Cards
		float deckWidthWeight = 1f/deckSlots;
		float[] weights = new float[deckSlots]; for(int i=0;i<deckSlots;++i) weights[i] = deckWidthWeight;
		x.DeckRects = cardRow.SplitHorizontallyByWeight( 15, weights );

		return x;
	}

	public static RegionLayoutClass ForCardFocused( Rectangle bounds, int deckSlots, int deckIndex ) {

		const float BIG_CARD_HEIGHT = .4f;
		const float BOTTOM_CARD_SPACER = .07f;

		var layout = ForIslandFocused(bounds,deckSlots);

		// Focus Cards
		Rectangle bigCardRow;
		(_, (bigCardRow, _)) = bounds.SplitVerticallyByWeight( 0, 1f - BIG_CARD_HEIGHT - BOTTOM_CARD_SPACER, BIG_CARD_HEIGHT, BOTTOM_CARD_SPACER );
		(_, (layout.DeckRects[deckIndex], _)) = bigCardRow.SplitHorizontallyByWeight( 0, .1f, .8f, .1f );

		return layout;
	}

	public static RegionLayoutClass ForGrowthFocused( Rectangle bounds, int deckSlots ) {

		var layout = ForIslandFocused( bounds, deckSlots );

		// Focus: Growth
		Rectangle row;
		(row, _) = bounds.SplitVerticallyByWeight( 0, .25f, .75f );
		(_, ( layout.GrowthRect, _)) = row.SplitHorizontallyByWeight(0, .4f, .6f );

		return layout;
	}


	#region Constructor

	RegionLayoutClass( Rectangle bounds ) { _bounds = bounds; DeckRects = new Rectangle[4]; }

	#endregion

	readonly Rectangle _bounds;
	public Rectangle SpiritRect { get; private set; }
	public Rectangle IslandRect { get; private set; }
	public Rectangle StatusRect { get; private set; }
	public Rectangle OptionRect { get; private set; }
	public Rectangle CardRect { get; private set; }
	public Rectangle[] DeckRects { get; private set; }


	// Spirit Parts
	public Rectangle SpiritImageBounds;
	public Rectangle GrowthRect;
	public Rectangle PresenceTractRect;
	public Rectangle InnateRect;
	public Rectangle ElementRect;


	#region pop-ups

	public Rectangle AckPopupRect {
		get {
			// Active Fear Layout
			int fearHeight = (int)(_bounds.Height * .5f);
			int fearWidth = fearHeight * 2 / 3;
			return new Rectangle( _bounds.X + (_bounds.Width - fearWidth) / 2, _bounds.Y + (_bounds.Height - fearHeight) / 2, fearWidth, fearHeight );

		}
	}

	public Rectangle ElementPopUpBounds( int count ) {
		// calculate layout based on count
		int height = _bounds.Height;
		int width = _bounds.Width;
		int maxHeight = (int)(height * .16f);
		int maxWidth = (int)(width * .75f);
		int desiredMmargin = 20;

		var maxBounds = new Rectangle( (width - maxWidth) / 2, (height - maxHeight) / 2, maxWidth, maxHeight ); // max size allowed to use
		var desiredSize = new Size( count * (maxHeight - desiredMmargin) + desiredMmargin, maxHeight ); // share we want.
		return maxBounds.FitBoth( desiredSize );
	}

	public Rectangle MinorMajorDeckSelectionPopup {
		get {
			int height = _bounds.Height;
			int boundsHeight = height / 3; // cards take up 1/3 of window vertically
			int boundsWidth = boundsHeight * 16 / 10;
			return new Rectangle( 0 + (_bounds.Width - boundsWidth) / 2, (height - boundsHeight) / 2, boundsWidth, boundsHeight );
		}
	}

	#endregion

	public void DrawRects( Graphics graphics ) {
		var rects = new Dictionary<string, Rectangle>() {
			["Options"] = OptionRect,
			["Island"] = IslandRect,
			["Spirit"] = SpiritRect,
			["Hand"] = DeckRects[0],
			["Played"] = DeckRects[1],
			["Discarded"] = DeckRects[2],
			["Cards"] = CardRect,
			// Spirit
			["PresenceTrack"] = PresenceTractRect,
			["Growth"] = GrowthRect,
			["Innates"] = InnateRect,
			// Debug

			// popups
			["Element Bounds"] = ElementPopUpBounds( 10 ),
			["Popup Fear"] = AckPopupRect,
			["Minor/Major"] = MinorMajorDeckSelectionPopup,
		};

		if(3 < DeckRects.Length)
			rects["Draw"] = DeckRects[3];

		var colors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Purple, Color.Yellow };
		int i = 0;
		foreach(var (title, r) in rects.Select( p => (p.Key, p.Value) )) {
			Color color = colors[i++ % colors.Length];
			using SolidBrush brush = new SolidBrush( color );
			graphics.DrawString( title, SystemFonts.DefaultFont, brush, r.Location );
			using Pen pen = new Pen( color, 2f );
			graphics.DrawRectangle( pen, r );
		}
	}

}
