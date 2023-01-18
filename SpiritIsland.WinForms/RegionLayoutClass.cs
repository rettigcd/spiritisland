using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

class RegionLayoutClass {

	const float SPIRIT_WIDTH = .35f; // % of screen width to use for spirit
	const float INFOBAR_HEIGHT = .1f; // % of height to use for info-bar

	Rectangle _infoRect;
	Rectangle _mainRect;
	readonly Rectangle _rect;

	public float GameLabelFontHeight => _height * .05f;
	int _width => _rect.Width;
	int _height => _rect.Height;

	#region Constructor

	public RegionLayoutClass( Rectangle rect ) {
		_rect = rect;

		const int MARGIN = 10;
		const float AdfersaryFlagWidths = 1.5f;
		const float InvaderCards = 4.5f;
		const float BlightWidths = 3f;
		const float FearWidths = 6f;

		(_infoRect,(_mainRect,_)) = rect.SplitVerticallyByWeight( 0, INFOBAR_HEIGHT, 1f - INFOBAR_HEIGHT ); // 0 margin because items in infoBar get their own margin.
		( IslandRect, (SpiritRect, _)) = _mainRect.SplitHorizontallyByWeight( MARGIN, 1 - SPIRIT_WIDTH, SPIRIT_WIDTH );
		(AdversaryFlagRect, (InvaderCardRect, (BlightRect, (FearPoolRect, _)))) = _infoRect
			.InflateBy( -MARGIN )
			.SplitHorizontallyRelativeToHeight( MARGIN, Align.Late, AdfersaryFlagWidths, InvaderCards, BlightWidths, FearWidths );
		(OptionRect,_) = IslandRect.SplitHorizontallyByWeight(0,.1f,.9f);

		(_,(CardRectPopup,_)) = _mainRect.SplitVerticallyByWeight(0, .5f, .5f);
	}

	#endregion

	public Rectangle SpiritRect        { get; }
	public Rectangle IslandRect        { get; }
	public Rectangle AdversaryFlagRect { get; }
	public Rectangle FearPoolRect      { get; }
	public Rectangle BlightRect        { get; }
	public Rectangle InvaderCardRect   { get; }
	public Rectangle OptionRect { get; }

	#region pop-ups

	public Rectangle PopupFearRect { get {
		Rectangle bounds = new Rectangle( 0, 0, (int)(_width * .65f), _height );
		// Active Fear Layout
		int fearHeight = (int)(bounds.Height * .8f);
		int fearWidth = fearHeight * 2 / 3;
		return new Rectangle( bounds.Width - fearWidth - (int)(bounds.Height * .1f), (bounds.Height - fearHeight) / 2, fearWidth, fearHeight );

	} }

	public Rectangle CardRectPopup { get; }

	public Rectangle ElementPopUpBounds( int count ) {
		// calculate layout based on count
		int maxHeight = (int)(_height * .16f);
		int maxWidth = (int)(_width * .75f);
		int desiredMmargin = 20;

		var maxBounds = new Rectangle( (_width - maxWidth) / 2, (_height - maxHeight) / 2, maxWidth, maxHeight ); // max size allowed to use
		var desiredSize = new Size( count * (maxHeight - desiredMmargin) + desiredMmargin, maxHeight ); // share we want.
		return maxBounds.FitBoth( desiredSize );
	}

	public Rectangle MinorMajorDeckSelectionPopup{ get { 
		int boundsHeight = _height / 3; // cards take up 1/3 of window vertically
		int boundsWidth = boundsHeight * 16 / 10;
		return new Rectangle( 0 + (_width - boundsWidth) / 2, (_height - boundsHeight)/2, boundsWidth, boundsHeight );
	} }

	#endregion

	public void DrawRects( Graphics graphics ) {
		var rects = new Dictionary<string, Rectangle>() {
			["Fear Pool"]      = FearPoolRect,
			["Adversary Flag"] = AdversaryFlagRect,
			["Blight"]         = BlightRect,
			["Invader Cards"]  = InvaderCardRect,
			["Options"]        = OptionRect,
			["Island"]         = IslandRect,
			["Spirit"]         = SpiritRect,
			// Debug
//			["InfoBar"]			= _infoRect,
//			["Main"]            = _mainRect,
			// popups
			["Element Bounds"] = ElementPopUpBounds( 10 ),
			["Popup Fear"]     = PopupFearRect,
			["Cards"]          = CardRectPopup,
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
