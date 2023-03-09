using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

class RegionLayoutClass {

	public static RegionLayoutClass Overlapping(Rectangle rect ) {
		var x = new RegionLayoutClass( rect );

		const int MARGIN = 10;
		const float SPIRIT_WIDTH = .35f; // % of screen width to use for spirit
		const float STATUSBAR_HEIGHT = .1f; // % of height to use for info-bar

		Rectangle mainRect;
		(x.StatusRect, (mainRect, _)) = rect.SplitVerticallyByWeight( 0, STATUSBAR_HEIGHT, 1f - STATUSBAR_HEIGHT ); // 0 margin because items in infoBar get their own margin.
		(x.IslandRect, (x.SpiritRect, _)) = mainRect.SplitHorizontallyByWeight( MARGIN, 1 - SPIRIT_WIDTH, SPIRIT_WIDTH );


		(x.OptionRect, _) = x.IslandRect.InflateBy( -MARGIN ).SplitHorizontallyByWeight( 0, .1f, .9f );
		(_, (x.CardRect, _)) = mainRect.SplitVerticallyByWeight( 0, .5f, .5f );

		// Don't let the spirit rect stretch to the bottom of the screen, make it 1.1 times higher than width
		x.SpiritRect = x.SpiritRect.FitBoth( new Size( x.SpiritRect.Width, (int)(x.SpiritRect.Width * 1.1) ), Align.Far, Align.Near );
		return x;
	}

	public static RegionLayoutClass ForIslandFocused(Rectangle bounds ) {
		var x = new RegionLayoutClass( bounds );

		const int MARGIN = 10;
		const float SPIRIT_WIDTH = .35f; // % of screen width to use for spirit
		const float STATUSBAR_HEIGHT = .1f; // % of height to use for info-bar
		const float CARD_HEIGHT = .15f;

		Rectangle mainRect;
		(x.StatusRect, (mainRect, (x.CardRect,_))) = bounds.SplitVerticallyByWeight( 0, STATUSBAR_HEIGHT, 1f - STATUSBAR_HEIGHT-CARD_HEIGHT, CARD_HEIGHT ); // 0 margin because items in infoBar get their own margin.

		Rectangle spiritLimit;
		(x.IslandRect, (spiritLimit, _)) = mainRect.SplitHorizontallyByWeight( MARGIN, 1 - SPIRIT_WIDTH, SPIRIT_WIDTH );

		// Don't let the spirit rect stretch to the bottom of the screen, make it 1.1 times higher than width
		x.SpiritRect = spiritLimit.FitBoth( new Size( spiritLimit.Width, (int)(spiritLimit.Width * 1.1) ), Align.Far, Align.Near );

		(x.OptionRect, _) = x.IslandRect.InflateBy( -MARGIN ).SplitHorizontallyByWeight( 0, .1f, .9f );

		return x;
	}

	public static RegionLayoutClass ForCardFocused( Rectangle bounds ) {
		var x = new RegionLayoutClass( bounds );

		const int MARGIN = 10;
		const float SPIRIT_WIDTH = .35f; // % of screen width to use for spirit
		const float STATUSBAR_HEIGHT = .1f; // % of height to use for info-bar
		const float CARD_HEIGHT = .40f;

		Rectangle mainRect;
		(x.StatusRect, (mainRect, (x.CardRect,_))) = bounds.SplitVerticallyByWeight( 0, STATUSBAR_HEIGHT, 1f - STATUSBAR_HEIGHT - CARD_HEIGHT, CARD_HEIGHT ); // 0 margin because items in infoBar get their own margin.

		(x.IslandRect, (x.SpiritRect, _)) = mainRect.SplitHorizontallyByWeight( MARGIN, 1 - SPIRIT_WIDTH, SPIRIT_WIDTH );

		(x.OptionRect, _) = x.IslandRect.InflateBy( -MARGIN ).SplitHorizontallyByWeight( 0, .1f, .9f );

		// Don't let the spirit rect stretch to the bottom of the screen, make it 1.1 times higher than width
		x.SpiritRect = x.SpiritRect.FitBoth( new Size( x.SpiritRect.Width, (int)(x.SpiritRect.Width * 1.1) ), Align.Far, Align.Near );

		return x;
	}


	#region Constructor

	RegionLayoutClass( Rectangle bounds ) { _bounds = bounds; }

	#endregion

	readonly Rectangle _bounds;
	public Rectangle SpiritRect        { get; private set; }
	public Rectangle IslandRect        { get; private set; }
	public Rectangle StatusRect        { get; private set;}
	public Rectangle CardRect          { get; private set; }
	public Rectangle OptionRect        { get; private set; }

	#region pop-ups

	public Rectangle PopupFearRect { get {
		// Active Fear Layout
		int fearHeight = (int)(_bounds.Height * .5f);
		int fearWidth = fearHeight*2/3;
		return new Rectangle( _bounds.X + (_bounds.Width - fearWidth)/2, _bounds.Y + (_bounds.Height - fearHeight) / 2, fearWidth, fearHeight );

	} }

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

	public Rectangle MinorMajorDeckSelectionPopup{ get {
		int height = _bounds.Height;
		int boundsHeight = height / 3; // cards take up 1/3 of window vertically
		int boundsWidth = boundsHeight * 16 / 10;
		return new Rectangle( 0 + (_bounds.Width - boundsWidth) / 2, (height - boundsHeight)/2, boundsWidth, boundsHeight );
	} }

	#endregion

	public void DrawRects( Graphics graphics ) {
		var rects = new Dictionary<string, Rectangle>() {
			["Options"]        = OptionRect,
			["Island"]         = IslandRect,
			["Spirit"]         = SpiritRect,
			// Debug
			// popups
			["Element Bounds"] = ElementPopUpBounds( 10 ),
			["Popup Fear"]     = PopupFearRect,
			["Cards"]          = CardRect,
			["Minor/Major"]    = MinorMajorDeckSelectionPopup,
		};

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
