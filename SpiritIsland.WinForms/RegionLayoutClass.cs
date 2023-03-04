using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

class RegionLayoutClass {

	const float SPIRIT_WIDTH = .35f; // % of screen width to use for spirit
	const float STATUSBAR_HEIGHT = .1f; // % of height to use for info-bar

	Rectangle _mainRect;
	readonly Rectangle _rect;

	public float GameLabelFontHeight => _height * .05f;
	int _width => _rect.Width;
	int _height => _rect.Height;

	#region Constructor

	public RegionLayoutClass( Rectangle rect ) {
		_rect = rect;

		const int MARGIN = 10;
		const float AdversaryFlagWidths = 1.5f;
		const float InvaderCards = 4.5f;
		const float BlightWidths = 3f;
		const float FearWidths = 6f;

		(StatusRect,(_mainRect,_)) = rect.SplitVerticallyByWeight( 0, STATUSBAR_HEIGHT, 1f - STATUSBAR_HEIGHT ); // 0 margin because items in infoBar get their own margin.
		( IslandRect, (SpiritRect, _)) = _mainRect.SplitHorizontallyByWeight( MARGIN, 1 - SPIRIT_WIDTH, SPIRIT_WIDTH );

		(PhaseRect,(AdversaryFlagRect, (InvaderCardRect, (BlightRect, (FearPoolRect, _))))) = StatusRect
			.InflateBy( -MARGIN )
			.SplitHorizontallyRelativeToHeight( MARGIN, Align.Far, 1.4f, AdversaryFlagWidths, InvaderCards, BlightWidths, FearWidths );

		(OptionRect,_) = IslandRect.InflateBy(-MARGIN).SplitHorizontallyByWeight(0,.1f,.9f);
		(_,(CardRect,_)) = _mainRect.SplitVerticallyByWeight(0, .5f, .5f);

		// Don't let the spirit rect stretch to the bottom of the screen, make it 1.1 times higher than width
		SpiritRect = SpiritRect.FitBoth(new Size(SpiritRect.Width,(int)(SpiritRect.Width*1.1)),Align.Far,Align.Near);
		PhaseRect = PhaseRect.InflateBy( (int)(PhaseRect.Height*.05) );
	}

	#endregion

	public Rectangle SpiritRect        { get; }
	public Rectangle IslandRect        { get; }
	public Rectangle StatusRect        { get; }
	public Rectangle CardRect          { get; }
	public Rectangle PhaseRect         { get; }
	public Rectangle AdversaryFlagRect { get; }
	public Rectangle FearPoolRect      { get; }
	public Rectangle BlightRect        { get; }
	public Rectangle InvaderCardRect   { get; }
	public Rectangle OptionRect        { get; }

	#region pop-ups

	public Rectangle PopupFearRect { get {
		// Active Fear Layout
		int fearHeight = (int)(_rect.Height * .5f);
		int fearWidth = fearHeight*2/3;
		return new Rectangle( _rect.X + (_rect.Width - fearWidth)/2, _rect.Y + (_rect.Height - fearHeight) / 2, fearWidth, fearHeight );

	} }

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
