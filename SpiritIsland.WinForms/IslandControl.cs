using SpiritIsland.Log;
using SpiritIsland.Select;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

/*
	== Painting / Options => Drawing & Hotspot  process ==
	1) NewDecisionArrived - parse options record which type we have
	2) Draw_Static part of island
	3) DecorateSpace => DrawRow => Record where Tokens cards, etc are located
	4) Other controls draw themselves and record location
	5) DrawHotspots => foreeach Option => Find location and draw it.
*/

public partial class IslandControl : Control {

	#region constructor / Init

	public IslandControl() {
		InitializeComponent();

		SetStyle(ControlStyles.AllPaintingInWmPaint 
			| ControlStyles.UserPaint 
			| ControlStyles.OptimizedDoubleBuffer 
			| ControlStyles.ResizeRedraw, true
		);
		LoadStandardTokenImages();
		_cardData = new CardUi();
		_cardData.AppearanceChanged += () => Invalidate();
		_cardData.CardClicked += (card) => OptionSelected?.Invoke( card );

	}

	public void Init( GameState gameState, PresenceTokenAppearance presenceAppearance, AdversaryConfig adversary ) {

		// Wind down old

		// Dispose old spirit tokens
		_presenceImg?.Dispose();
		_tokenImages[Token.Defend]?.Dispose();
		_tokenImages[Token.Isolate]?.Dispose();

		// Setup New
		_gameState = gameState;
		_spirit = gameState.Spirits.Single();
		_adversary = adversary;
		_cardData.SpiritCardInfo = new SpiritCardInfo( gameState.Spirits[0] ); // !!! 1 spirit only


		// Init new Presence, Defend, Isolate
		// !! we could cache these if we could serialize the Adjustment into a caching-key

		_presenceImg = ResourceImages.Singleton.GetPresenceImage( presenceAppearance.BaseImage );
		_tokenImages[Token.Defend] = ResourceImages.Singleton.GetImage( Img.Defend );
		_tokenImages[Token.Isolate] = ResourceImages.Singleton.GetImage( Img.Isolate );
		presenceAppearance.Adjustment?.Adjust( (Bitmap)_presenceImg );
		presenceAppearance.Adjustment?.Adjust( (Bitmap)_tokenImages[Token.Defend] );
		presenceAppearance.Adjustment?.Adjust( (Bitmap)_tokenImages[Token.Isolate] );

		_spiritPainter = new SpiritPainter( _spirit, presenceAppearance );

		Invalidate_GameLayout();
	}

	void AddButtonsToContainer() {
		_buttonContainer.Clear();
		foreach(InnatePower power in _spirit.InnatePowers) {
			_buttonContainer.Add( power, new InnateButton() );
			foreach(IDrawableInnateOption innatePowerOption in power.DrawableOptions)
				_buttonContainer.Add( innatePowerOption, new InnateOptionsBtn( _spirit, innatePowerOption ) );
		}
		foreach(Track energySlot in _spirit.Presence.Energy.Slots)
			_buttonContainer.Add( energySlot, new PresenceSlotButton( _spirit.Presence.Energy, energySlot, _presenceImg ) );
		foreach(Track cardSlot in _spirit.Presence.CardPlays.Slots)
			_buttonContainer.Add( cardSlot, new PresenceSlotButton( _spirit.Presence.CardPlays, cardSlot, _presenceImg ) );
		foreach(var action in _spirit.GrowthTrack.Options.SelectMany( optionGroup => optionGroup.GrowthActions ))
			_buttonContainer.Add( action, new GrowthButton() );
		foreach(var spaceState in _gameState.Spaces_Unfiltered)
			_buttonContainer.Add( spaceState.Space, new SpaceButton( GetPortPoint ,spaceState.Space, hotspotRadius ) );
	}

	#endregion constructor / Init

	#region Calc Layout

	// Window Dependent
	RegionLayoutClass RegionLayout => _regionLayout ??= new RegionLayoutClass( ClientRectangle );
	RegionLayoutClass _regionLayout; // depends ONLY on the window/client, NOT on the game
	public Rectangle OptionBounds => RegionLayout.OptionRect;

	// Game Dependent
	RectangleF BoardWorldRect => _boardWorldRect ??= CalcIslandWorldBounds();
	RectangleF? _boardWorldRect;

	//Dictionary<Space, ManageInternalPoints> InsidePoints => _insidePoints 
	//	??= _gameState.AllSpaces.ToDictionary( ss => ss.Space, ss => new ManageInternalPoints( ss ) );

	ManageInternalPoints InsidePoints( Space space ) {
		_insidePoints ??= _gameState.Spaces_Unfiltered.ToDictionary( ss => ss.Space, ss => new ManageInternalPoints( ss ) );

		// In case Weave-Together has occurred and the event hasn't propogated here yet.
		if(!_insidePoints.ContainsKey( space ))
			_insidePoints.Add( space, new ManageInternalPoints( _gameState.Tokens[space] ) );

		return _insidePoints[space];
	}
	Dictionary<Space, ManageInternalPoints> _insidePoints;

	// Game & Window Dependent
	Rectangle gw_boardScreenRect;
	PointMapper gw_mapper; // Maps from normallized space to Client space.
	Bitmap gw_cachedBackground;
	SpiritLayout gw_spiritLayout;
	int gw_IconWidth;

	RectangleF CalcIslandWorldBounds() {
		float left = float.MaxValue;
		float top = float.MaxValue;
		float right = float.MinValue;
		float bottom = float.MinValue;
		foreach(var board in _gameState.Island.Boards) {
			var e = board.OriginalLayout.CalcExtents();
			if(e.Left < left) left = e.Left;
			if(e.Top < top) top = e.Top;
			if(e.Right > right) right = e.Right;
			if(e.Bottom > bottom) bottom = e.Bottom;
		}
		const float CUSHION = .015f; // because the drawing algo is curvey and goes outside the bounds
		return new RectangleF( left-CUSHION, top - CUSHION, right - left+2*CUSHION, bottom - top+2*CUSHION );
	}

	AdversaryConfig _adversary;

	readonly VisibleButtonContainer _buttonContainer = new VisibleButtonContainer();

	static Matrix3D CalcWorldToScreenMatrix( RectangleF worldRect, Rectangle viewportRect ) {
		// calculate scaling Assuming height limited
		float scale = viewportRect.Height / worldRect.Height;

		var islandBitmapMatrix
			= RowVector.Translate( -worldRect.X, -worldRect.Y ) // translate to origin
			* RowVector.Scale( scale, -scale ) // flip-y and scale
			* RowVector.Translate( 0, viewportRect.Height ) // because 0,0 is at the bottom,left
			* RowVector.Translate( viewportRect.X, viewportRect.Y ); // translate to viewport origin
		return islandBitmapMatrix;
	}

	static Rectangle FitWidth( float x, float y, float width, Size sz ) {
		return new Rectangle( (int)x, (int)y, (int)width, (int)(width / sz.Width * sz.Height) );
	}

	#endregion Calc Layout

	protected override void OnPaint( PaintEventArgs pe ) {
		base.OnPaint( pe );

		_tokenLocations.Clear();
		_hotSpots.Clear(); // Clear this at beginning so any of the DrawX methods can add to it

		if(_gameState is null) return;

		if(gw_cachedBackground == null)
			GameLayout_Calculate();

		StopWatch.timeLog.Clear();

		using(new StopWatch( "Total" )) {

			pe.Graphics.DrawImage( gw_cachedBackground, gw_boardScreenRect );

			DrawGameRound( pe.Graphics );

			DrawPhase( pe.Graphics );

			// mostly static
			DrawAdversary( pe );
			DrawFearPool( pe.Graphics );
			DrawBlight( pe.Graphics );
			DrawInvaderCards( pe.Graphics ); // other than highlights, do this last since it contains the Fear Card that we want to be on top of everything.

			// Island / Spaces
			foreach(SpaceState space in _gameState.Spaces_Unfiltered)
				DecorateSpace( pe.Graphics, space );

			DrawHotSpots_SpaceToken( pe.Graphics );

			DrawArrows( pe.Graphics );

			DrawSpirit( pe.Graphics );

			_buttonContainer.Paint( pe.Graphics );

			DrawPowerCards( pe.Graphics );

			// Pop-ups - draw last, because they are pop-ups and should be on top.
			DrawDeckPopUp( pe.Graphics );
			DrawElementsPopUp( pe.Graphics );
			DrawFearPopUp( pe.Graphics );

			if(_debug)
				RegionLayout.DrawRects( pe.Graphics );
		}

		// non drawing - record Hot spots
		RecordSpiritHotspots();
		if(options_FearPopUp is not null) 
			_hotSpots.Add(options_FearPopUp, RegionLayout.PopupFearRect );
		else if(options_BlightPopUp is not null)
			_hotSpots.Add( options_BlightPopUp, RegionLayout.PopupFearRect );
	}

	public void GameState_NewLogEntry( ILogEntry obj ) {
		if(obj is Log.Phase phaseEvent) {
			_phaseImage?.Dispose();
			_phaseImage = phaseEvent.phase switch {
				Phase.Growth => ResourceImages.Singleton.GetImage( Img.Coin ),
				Phase.Fast   => ResourceImages.Singleton.GetImage( Img.Icon_Fast ),
				Phase.Slow   => ResourceImages.Singleton.GetImage( Img.Icon_Slow ),
				_ => null,
			};
			Invalidate();
		}
		if(obj is LayoutChanged)
			Invalidate_GameLayout();
	}
	Image _phaseImage; // updates on Log Events

	void DrawPhase( Graphics graphics ){
		if(_phaseImage != null)
			graphics.DrawImage( _phaseImage, RegionLayout.PhaseRect.FitBoth( _phaseImage.Size));
	}

	#region Draw Static Board

	void DrawBoardSpacesOnly( Graphics graphics, Board board ) {
		Pen perimeterPen = new Pen( SpacePerimeterColor, 5f );

		// !!! Bug - Layout needs updated when we weave stuff together.  Out of sync with # of spaces on board.
		BoardLayout normalizedBoardLayout = board.OriginalLayout;

		foreach(var space in board.Spaces_Unfiltered) {
			// Space space = board[i];
			using Brush brush = ResourceImages.Singleton.UseSpaceBrush( space );
			// SpaceLayout spaceLayout = normalizedBoardLayout.Spaces[i];
			SpaceLayout spaceLayout = space.Layout;
			PointF[] points = spaceLayout.Corners.Select( gw_mapper.Map ).ToArray();

			// Draw blocky
			//pe.Graphics.FillPolygon( brush, points );
			//pe.Graphics.DrawPolygon( perimeterPen, points );

			// Draw smoothy
			graphics.FillClosedCurve( brush, points, FillMode.Alternate, .25f );
			graphics.DrawClosedCurve( perimeterPen, points, .25f, FillMode.Alternate );

			// Draw Label
			PointF nameLocation = gw_mapper.Map( InsidePoints(space).NameLocation );
			graphics.DrawString( space.Text, SystemFonts.MessageBoxFont, SpaceLabelBrush, nameLocation );
		}

	}

	#endregion Draw Static Board

	void DrawGameRound( Graphics graphics ) {
		using Font font = UseGameFont( RegionLayout.GameLabelFontHeight );

		Brush brush = GameTextBrush_Default;
		string snippet = "Fight!";

		// If game is over, update
		if(_gameState.Result != null) {

			brush = _gameState.Result.Result == GameOverResult.Victory ? GameTextBrush_Victory : GameTextBrush_Defeat;
			snippet = _gameState.Result.Msg( LogLevel.Info );
		}
		graphics.DrawString( $"Round {_gameState.RoundNumber} - {snippet}", font, brush, 0, 0 );

	}

	#region Draw - Fear, Blight, Invader Cards

	void DrawAdversary( PaintEventArgs pe ) {
		if(_adversary != null) {
			using var flag = ResourceImages.Singleton.GetAdversaryFlag( _adversary.Name );
			pe.Graphics.DrawImage( flag, RegionLayout.AdversaryFlagRect );
		}
	}

	void DrawFearPool( Graphics graphics ) {
		var outterBounds = RegionLayout.FearPoolRect;

		int margin = Math.Max( 5, (int)(outterBounds.Height * .05f) );
		var bounds = outterBounds.InflateBy(-margin);

		using var cardImg = ResourceImages.Singleton.FearCardBack(); // Maybe load this with the control and not dispose of it every time we draw.

		// -1 slot width for #/# and 
		// -1 slot width for last fear token
		int slotWidth = bounds.Width / 6;

		bool limitedByHeight = bounds.Height * _fearTokenImage.Width < slotWidth * _fearTokenImage.Height;
		var tokenSize = limitedByHeight
			? new Size( bounds.Height * _fearTokenImage.Width / _fearTokenImage.Height, bounds.Height )
			: new Size( slotWidth, slotWidth * _fearTokenImage.Height / _fearTokenImage.Width ); // assume token is wider than tall.

		// Calc Terror Level bounds - slotWidth reserved but only tokenSize.Width used
		var terrorLevelBounds = new RectangleF( bounds.X, bounds.Y, tokenSize.Width, tokenSize.Height );

		// Calc Fear Pool bounds - skip 1 slotWidth, 
		int poolMax = this._gameState.Fear.PoolMax;
		float step = (bounds.Width - 4 * slotWidth) / (poolMax - 1);
		RectangleF CalcBounds( int i ) => new RectangleF( bounds.X + slotWidth + step * i, bounds.Y, tokenSize.Width, tokenSize.Height );

		// Calc Activated Fear Bounds
		int cardHeight = tokenSize.Height * 7 / 8;
		var activatedCardRect = new Rectangle( bounds.Right - slotWidth*2, bounds.Y, tokenSize.Width, cardHeight ).FitHeight( cardImg.Size );
		var futureCardRect    = new Rectangle( bounds.Right - slotWidth, bounds.Y, tokenSize.Width, cardHeight ).FitHeight( cardImg.Size );


		// Draw Terror Level
		using var terror = ResourceImages.Singleton.TerrorLevel( _gameState.Fear.TerrorLevel );
		graphics.DrawImage( terror, terrorLevelBounds );

		// Draw Fear Pool
		int fearCount = _gameState.Fear.EarnedFear;
		for(int i = fearCount; i < poolMax; ++i)
			graphics.DrawImage( _grayFear, CalcBounds( i ) );	// Gray underneath
		// draw fear tokens
		for(int i = 0; i < fearCount; ++i)
			graphics.DrawImage( _fearTokenImage, CalcBounds( i ) );	// Tokens

		// Activated Cards
		int activated = _gameState.Fear.ActivatedCards.Count;
		if(0 < activated) {
			// Draw Card
			var top = _gameState.Fear.ActivatedCards.Peek();
			if(top.Flipped) {
				using Image img = FearCardImageManager.GetImage( top );
				graphics.DrawImage( img, activatedCardRect );
			} else {
				graphics.DrawImage( cardImg, activatedCardRect );
			}
			graphics.DrawCountIfHigherThan( activatedCardRect, activated );
		} else {
			graphics.FillRectangle(EmptySlotBrush, activatedCardRect );
		}

		int future = _gameState.Fear.Deck.Count;
		if(0 < future) {
			var top = _gameState.Fear.Deck.Peek();
			if(top.Flipped) {
				using Image img = FearCardImageManager.GetImage( top );
				graphics.DrawImage( img, futureCardRect );
			} else {
				// Draw Card
				graphics.DrawImage( cardImg, futureCardRect );
			}
			graphics.DrawCountIfHigherThan( futureCardRect, future );
			futureCardRect.Location = new Point( futureCardRect.X, futureCardRect.Bottom);
			futureCardRect = futureCardRect.InflateBy(30,0);
			using var cardCountFont = UseGameFont( margin * 3);
			graphics.DrawStringCenter( string.Join(" / ",_gameState.Fear.CardsPerLevelRemaining), cardCountFont, CardLabelBrush, futureCardRect);
		}
	}

	void DrawBlight( Graphics graphics ) {
		var bounds = RegionLayout.BlightRect;

		int margin = Math.Max( 5, (int)(bounds.Height * .05f) );
		int slotWidth = bounds.Height;

		int count = _gameState.Tokens[BlightCard.Space].Blight.Count;
		int maxSpaces = 6;

		float step = (bounds.Width - 2 * margin - 2 * slotWidth) / (maxSpaces - 1);
		// -1 slot width for #/# and 
		// -1 slot width for last fear token
		float tokenWidth = slotWidth - 2 * margin;
		float tokenHeight = _fearTokenImage.Height * tokenWidth / _fearTokenImage.Width;
		RectangleF CalcBounds( int i ) => new RectangleF( bounds.X + slotWidth + margin + step * i, bounds.Y + margin, tokenWidth, tokenHeight );

		// draw fear tokens
		var img = this._tokenImages[Token.Blight];
		for(int i = 0; i < count; ++i)
			graphics.DrawImage( img, CalcBounds( i ) );

		using Image healthy = _gameState.BlightCard.CardFlipped 
			? ResourceImages.Singleton.GetBlightCard( _gameState.BlightCard )
			: ResourceImages.Singleton.GetHealthBlightCard();
		graphics.DrawImageFitHeight( healthy, bounds.FitHeight( healthy.Size, Align.Near ) );
	}

	void DrawInvaderCards( Graphics graphics ) {

		const int CARD_SEPARATOR = 10;
		var bounds = RegionLayout.InvaderCardRect;

		// Calculate Card Size based on # of slots
		float slots = _gameState.InvaderDeck.ActiveSlots.Count + 1.5f;
		float slotWidth = bounds.Width / slots;
		float cardHeight = bounds.Height * .8f;
		int textHeight = (int)(bounds.Height * .2f);

		bool isTooNarrow = slotWidth * 1.5f < cardHeight;
		Size cardSize = isTooNarrow
			? new Size( (int)slotWidth, (int)(slotWidth * 1.5f) )     // use narrow width to limit height
			: new Size( (int)(cardHeight / 1.5f), (int)cardHeight ); // plenty of width, use height to determine size

		// locate each of the cards
		var cardMetrics = new InvaderCardMetrics[_gameState.InvaderDeck.ActiveSlots.Count];

		for(int i = 0; i < cardMetrics.Length; ++i)
			cardMetrics[i] = new InvaderCardMetrics( _gameState.InvaderDeck.ActiveSlots[i],
				bounds.Left + CARD_SEPARATOR + (int)((i + 1.5f) * (cardSize.Width+CARD_SEPARATOR)), //left+i*xStep, 
				bounds.Top, // y, 
				cardSize.Width, 
				cardSize.Height, // width, height, 
				textHeight
			);

		// Draw
		using Font buildRavageFont = UseGameFont( textHeight ); // 
		foreach(InvaderCardMetrics cardMetric in cardMetrics)
			cardMetric.Draw( graphics, buildRavageFont );

		// # of cards in explore pile
		graphics.DrawCountIfHigherThan( cardMetrics.Last().Rect.First(), _gameState.InvaderDeck.UnrevealedCards.Count+1 );

		// Draw Discard
		var lastDiscard = _gameState.InvaderDeck.Discards.LastOrDefault();
		if(lastDiscard is not null) {
			// calc discard location
			var discardRect = new RectangleF(
				bounds.Left,
				bounds.Top + (cardSize.Height - cardSize.Width) * .5f,
				cardSize.Height,
				cardSize.Width
			);
			Point[] discardDestinationPoints = {
				new Point((int)discardRect.Left, (int)discardRect.Bottom),    // destination for upper-left point of original
				new Point((int)discardRect.Left, (int)discardRect.Top), // destination for upper-right point of original
				new Point((int)discardRect.Right,(int)discardRect.Bottom)      // destination for lower-left point of original
			};
			using Image discardImg = ResourceImages.Singleton.GetInvaderCard( lastDiscard );
			graphics.DrawImage( discardImg, discardDestinationPoints );
			// # of cards in explore pile
			graphics.DrawCountIfHigherThan( discardRect, _gameState.InvaderDeck.Discards.Count );
		}
	}

	#endregion Draw - Fear, Blight, Invader Cards

	#region Draw - Board Spaces & Tokens

	void DecorateSpace( Graphics graphics, SpaceState spaceState ) {

		InsidePoints(spaceState.Space).Init(spaceState);

		int iconWidth = gw_IconWidth;//  gw_boardScreenRect.Width / 20; // !!! scale tokens based on board/space size, NOT widow size (for 2 boards, tokens are too big)

		if(_debug)
			DrawTokenTargets( graphics, spaceState, iconWidth );

		if(spaceState.Space is MultiSpace ms)
			DrawMultiSpace( graphics, ms );

		DrawInvaderRow( graphics, spaceState, iconWidth );
		DrawRow( graphics, spaceState, iconWidth );
	}

	void DrawTokenTargets( Graphics graphics, SpaceState spaceState, float iconWidth ) {
		foreach(var (token,point) in InsidePoints( spaceState.Space ).Assignments()) {
			using Image img = ResourceImages.Singleton.GetGhostImage( token.Img );
			var clientPoint = gw_mapper.Map( new PointF( point.X, point.Y ) );
			Rectangle rect = FitWidth( clientPoint.X - iconWidth / 2, clientPoint.Y - iconWidth / 2, iconWidth, img.Size );
			graphics.DrawImage( img, rect );
		}
	}

	void DrawInvaderRow( Graphics graphics, SpaceState ss, float iconWidth ) {

		var orderedInvaders = ss.Keys
			.Where( k => k.Class.Category == TokenCategory.Invader )
			.Cast<HumanToken>()
			// Major ordering: (Type > Strife)
			.OrderByDescending( i => i.FullHealth )
			.ThenBy( x => x.StrifeCount )
			// Minor ordering: (remaining health)
			.ThenBy( i => i.RemainingHealth ); // show damaged first so when we apply damage, the damaged one replaces the old undamaged one.

		foreach(IToken token in orderedInvaders) {

			// New way
			PointF center = gw_mapper.Map( InsidePoints(ss.Space).GetPointFor( token ) );
			float x = center.X-iconWidth/2;
			float y = center.Y-iconWidth/2; //!! approximate - need Image to get actual Height to scale

			// Strife
			IToken imageToken;
			if(token is HumanToken si && 0 < si.StrifeCount) {
				imageToken = si.HavingStrife( 0 );

				Rectangle strifeRect = FitWidth( x, y, iconWidth, _strife.Size );
				graphics.DrawImage( _strife, strifeRect );
				if(si.StrifeCount > 1)
					graphics.DrawSuperscript( strifeRect, "x" + si.StrifeCount );
			} else {
				imageToken = token;
			}

			// record token location
			Image img = AccessTokenImage( imageToken );
			Rectangle rect = FitWidth( x, y, iconWidth, img.Size );
			_tokenLocations.Add( new SpaceToken( ss.Space, token ), rect );

			// Draw Token
			graphics.DrawImage( img, rect );
			// Count
			graphics.DrawCountIfHigherThan( rect, ss[token] );

		}

	}

	void DrawPowerCards( Graphics graphics ) {
		_cardData.Layout ??= new CardLayout( RegionLayout.CardRectPopup );
		_cardData.DrawParts( graphics );
	}

	void DrawRow( Graphics graphics, SpaceState spaceState, float iconWidth ) {

		var tokenTypes = new List<IToken> {
			Token.Defend, Token.Blight, // These don't show up in .OfAnyType if they are dynamic
			Token.Wilds, Token.Badlands, Token.Isolate
		}	.Union( spaceState.OfCategory( TokenCategory.Dahan ) )
			.Union( spaceState.OfClass( Token.Beast ) )
			.Union( spaceState.OfAnyClass( _spirit.Token, Token.Element, Token.OpenTheWays, Token.Beast, Token.Disease ) )
			.Cast<IToken>()
			.ToArray();

		foreach(var token in tokenTypes) {
			int count = spaceState[token];
			if(count == 0) continue;

			bool isPresence = token is SpiritPresenceToken;
			Image img = isPresence ? _presenceImg : AccessTokenImage( token );

			// calc rect
			float iconHeight = iconWidth / img.Width * img.Height;

			PointF pt = gw_mapper.Map( InsidePoints(spaceState.Space).GetPointFor(token) );
			Rectangle rect = new Rectangle( (int)(pt.X-iconWidth/2), (int)(pt.Y-iconHeight/2), (int)iconWidth, (int)iconHeight );

			// record token location
			_tokenLocations.Add( new SpaceToken( spaceState.Space, token ), rect );

			if(isPresence && _spirit.Presence.IsSacredSite( spaceState )) {
				const int inflationSize = 10;
				rect.Inflate( inflationSize, inflationSize );

				using var brush = new SolidBrush( Color.FromArgb( 100, SacredSiteColor ) );
				graphics.FillEllipse( brush, rect );
				rect.Inflate( -inflationSize, -inflationSize );
			}

			// Draw Tokens
			graphics.DrawImage( img, rect );
			graphics.DrawCountIfHigherThan( rect, count );
		}


	}

	#endregion 	Draw - Board Spaces & Tokens

	#region Draw - Pop Ups

	void DrawElementsPopUp(Graphics graphics ) {
		if(decision_Element is null) return;

		var elementOptions = decision_Element.ElementOptions;
		int count = elementOptions.Length;

		RectangleF bounds = RegionLayout.ElementPopUpBounds( count );

		// recalculate this incase bounds got squished
		float actualMargin = 1 < count ? (count * bounds.Height - bounds.Width) / (count - 1) : bounds.Height * .05f;

		// Background
		graphics.FillRectangle( PopupBackgroundBrush, bounds );
		ButtonBorderStyle bs = ButtonBorderStyle.Outset;
		int borderWidth = (int)(actualMargin / 2);
		ControlPaint.DrawBorder( graphics, bounds.ToInts(),
			PopupBorderColor, borderWidth, bs,
			PopupBorderColor, borderWidth, bs,
			PopupBorderColor, borderWidth, bs,
			PopupBorderColor, borderWidth, bs
		);

		// Draw Elements
		float contentSize = bounds.Height - actualMargin * 2;
		float x = bounds.X + actualMargin;
		float y = bounds.Y + actualMargin;
		foreach(var elementOption in elementOptions) {
			using var img = ResourceImages.Singleton.GetImage( elementOption.Item.GetTokenImg() );
			var rect = new RectangleF( x, y, contentSize, contentSize );
			graphics.DrawImage( img, rect );
			_hotSpots.Add( elementOption, rect );

			x += (contentSize + actualMargin);
		}

	}

	void DrawDeckPopUp(Graphics graphics ) {
		if(decision_DeckToDrawFrom is null) return;

		// calc layout
		Rectangle bounds = RegionLayout.MinorMajorDeckSelectionPopup;
		Rectangle innerDeckBounds = bounds.InflateBy( -bounds.Height / 20 );

		var cardWidth = innerDeckBounds.Height * 3 / 4;
		var minorRect = new Rectangle( innerDeckBounds.X, innerDeckBounds.Y, cardWidth, innerDeckBounds.Height );
		var majorRect = new Rectangle( innerDeckBounds.Right - cardWidth, innerDeckBounds.Y, cardWidth, innerDeckBounds.Height );

		// Hotspots
		// !! we are assuming minor is first...
		_hotSpots.Add( decision_DeckToDrawFrom.PowerTypes[0], minorRect );
		_hotSpots.Add( decision_DeckToDrawFrom.PowerTypes[1], majorRect );

		graphics.FillRectangle( PopupBackgroundBrush, bounds );
		using var minorImage = Image.FromFile( ".\\images\\minor.png" );
		using var majorImage = Image.FromFile( ".\\images\\major.png" );
		graphics.DrawImage( minorImage, minorRect );
		graphics.DrawImage( majorImage, majorRect );
	}

	void DrawFearPopUp( Graphics graphics ) {
		if(options_FearPopUp is not null) {
			using Image img = FearCardImageManager.GetImage( options_FearPopUp );
			graphics.DrawImage( img, RegionLayout.PopupFearRect );
		}
		if(options_BlightPopUp is not null) {
			using Image img = ResourceImages.Singleton.GetBlightCard( options_BlightPopUp );
			graphics.DrawImage( img, RegionLayout.PopupFearRect );
		}
	}

	#endregion Draw - Pop Ups

	#region Draw HotSpots - spaces / space tokens

	void DrawHotSpots_SpaceToken( Graphics graphics ) {
		if(_decision is not Select.TokenFromManySpaces spaceTokenDecision) return;

		// Draw SpaceToken option hotspots & record option location
		using Pen pen = new Pen( Color.Aquamarine, 5 );
		foreach(SpaceToken spaceToken in spaceTokenDecision.SpaceTokens)
			DrawSpaceTokenHotspot( graphics, pen, spaceToken, spaceToken );
	}

	void DrawArrows( Graphics graphics ) {
		if(_decision is not IHaveArrows quiver) return;
		using Pen pushArrowPen = UsingArrowPen;
		foreach(Arrow arrow in quiver.Arrows)
			graphics.DrawArrow( pushArrowPen, GetPortPoint( arrow.From, arrow.Token ), GetPortPoint( arrow.To, arrow.Token ) );
	}

	void DrawSpaceTokenHotspot(	// SpaceTokens and DeployedPresence
		Graphics graphics,
		Pen pen,
		IOption option,  // the actual option to record
		SpaceToken st   // the effective SpaceToken (location) of the option
	) {

		Rectangle rect;
		if( _tokenLocations.ContainsKey( st ) )
			// Real
			rect = _tokenLocations[st];
		else {
			// Virtual
			var p = gw_mapper.Map( InsidePoints(st.Space).GetPointFor( st.Token ) ).ToInts();
			rect = new Rectangle( p.X-hotspotRadius, p.Y-hotspotRadius, hotspotRadius*2, hotspotRadius*2 ); // !!! use token size, whatever that is.
		}

		rect.Inflate( 2, 2 );
		_hotSpots.Add( option, rect );
		graphics.DrawRectangle( pen, rect );

	}



	Point GetPortPoint( Space space, IToken visibileTokens ) {
		PointF worldCoord = visibileTokens != null
			? InsidePoints(space).GetPointFor( visibileTokens )
			: space.Layout.Center; // normal space 
		return gw_mapper.Map( worldCoord ).ToInts();
	}


	static Pen UsingArrowPen => new Pen( ArrowColor, 7 );


	#endregion

	void DrawSpirit( Graphics graphics ) {
		graphics.FillRectangle( SpiritPanelBackgroundBrush, RegionLayout.SpiritRect );
		_spiritPainter.Paint( graphics );
	}

	void DrawMultiSpace( Graphics graphics, MultiSpace multi ) {

		using var pen = new Pen( MultiSpacePerimeterColor, 3f );

		using var brush = UseMultiSpaceBrush( multi );

		var points = multi.Layout.Corners.Select( gw_mapper.Map ).ToArray();
		graphics.FillClosedCurve( brush, points, FillMode.Alternate, .25f );
		graphics.DrawClosedCurve( pen, points, .25f, FillMode.Alternate );
		// graphics.FillPolygon( brush, points );
		// graphics.DrawPolygon( pen, points );

	}

	static LinearGradientBrush UseMultiSpaceBrush( MultiSpace multi ) {
		var brush = new LinearGradientBrush( new Rectangle( 0, 0, 30, 30 ), Color.Transparent, Color.Transparent, 45F );

		var colors = multi.OrigSpaces
			.Select( x => Color.FromArgb( 92, SpaceColor( x ) ) )
			.ToArray();

	    var blend = new ColorBlend {
			Positions = new float[colors.Length * 2],
			Colors = new Color[colors.Length * 2]
		};
		float step = 1.0f / colors.Length;
		for(int i = 0; i < colors.Length; ++i) {
			blend.Positions[i * 2] = i * step;
			blend.Positions[i * 2 + 1] = (i + 1) * step;
			blend.Colors[i * 2] = blend.Colors[i * 2 + 1] = colors[i];
		}
		brush.InterpolationColors = blend;
		return brush;
	}

	void RecordSpiritHotspots() {
		// Growth Options/Actions
		foreach(var act in options_GrowthActions)
			if(gw_spiritLayout.growthLayout.HasAction( act )) // there might be delayed setup actions here that don't have a rect
				_hotSpots.Add( act, gw_spiritLayout.growthLayout[act] );
	}

	Image AccessTokenImage( IToken imageToken ) {
		if(!_tokenImages.ContainsKey( imageToken ))
			_tokenImages[imageToken] = ResourceImages.Singleton.GetTokenImage( imageToken );
		return _tokenImages[imageToken];
	}

	protected override void OnSizeChanged( EventArgs e ) {
		base.OnSizeChanged( e );

		// Refresh window-dependent stuff
		_regionLayout = null;

		// Cascade
		Invalidate_GameWindowLayout();
	}

	void GameLayout_Calculate() {

		// Depends on Gamestate And Window
		gw_boardScreenRect = RegionLayout.IslandRect.FitBoth( BoardWorldRect.Scale( 1000 ).ToInts().Size, Align.Center, Align.Near );
		gw_mapper = new PointMapper( CalcWorldToScreenMatrix( BoardWorldRect, gw_boardScreenRect ) );
		gw_IconWidth = (int)( gw_mapper.XScale * .075f );

		// !! could delay initializing Layout until we get here
		gw_spiritLayout = new SpiritLayout( _spirit, RegionLayout.SpiritRect, 10, _buttonContainer );
		_spiritPainter?.SetLayout( gw_spiritLayout );

		// ------------------
		// Cached Background 
		// ------------------
		gw_cachedBackground = new Bitmap( gw_boardScreenRect.Width, gw_boardScreenRect.Height );
		using Graphics graphics = Graphics.FromImage( gw_cachedBackground );
		graphics.TranslateTransform( -gw_boardScreenRect.X, -gw_boardScreenRect.Y );
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

		foreach(Board board in _gameState.Island.Boards)
			DrawBoardSpacesOnly( graphics, board );
	}

	/// <summary>
	/// Invalidtes pieces that are Game-Dependent, then triggers Game/Window layout invalidation
	/// </summary>
	void Invalidate_GameLayout() {
		_insidePoints = null;
		_boardWorldRect = null;
		AddButtonsToContainer();

		// Cascade to layout that depends on this
		Invalidate_GameWindowLayout();
	}

	void Invalidate_GameWindowLayout() {

		// This stuff needs recalculated when either game changes OR window changes

		gw_spiritLayout = null;
		_cardData.Layout = null;

		if(gw_cachedBackground != null) {
			gw_cachedBackground.Dispose();
			gw_cachedBackground = null;
		}
		Invalidate();
	}

	protected override void OnClick( EventArgs e ) {
		var clientCoords = PointToClient( Control.MousePosition );
		IOption option = FindOption( clientCoords );

		if( option != null )
			OptionSelected?.Invoke(option);
		else
			_cardData.GetClickAction( clientCoords )?.Invoke();

		// Spirit => Special Rules
		if(gw_spiritLayout != null && gw_spiritLayout.imgBounds.Contains( clientCoords )) {
			string msg = this._spirit.SpecialRules.Select(r=>r.ToString()).Join("\r\n\r\n");
			MessageBox.Show( msg );
		}

		// Adversay Rules
		if(_adversary != null && RegionLayout.AdversaryFlagRect.Contains( clientCoords ) )
			PopUpAdversaryRules();
	}

	void PopUpAdversaryRules() {
		var adv = ConfigureGameDialog.GameBuilder.BuildAdversary( _adversary );
		var adjustments = adv.Adjustments;
		var rows = new List<string> {
			$"==== {_adversary.Name} - Level:{_adversary.Level} - Difficulty:{adjustments[_adversary.Level].Difficulty} ===="
		};
		for(int i = 0; i <= _adversary.Level; ++i) {
			var a = adjustments[i];
			string label = i == 0 ? "Escalation: " : "Level:" + i;
			rows.Add( $"\r\n-- {label} {a.Title} --" );
			rows.Add( $"{a.Description}" );
		}
		MessageBox.Show( rows.Join( "\r\n" ) );
	}

	protected override void OnMouseMove( MouseEventArgs e ) {
		base.OnMouseMove( e );

		Point point = this.PointToClient( Control.MousePosition );
		bool inCircle = FindOption( point ) != null
			|| _cardData.GetClickAction( point ) != null;

		Cursor = inCircle ? Cursors.Hand : Cursors.Default;

	}

	IOption FindOption( Point clientCoords ) {
		return FindHotSpot( clientCoords )
			?? _buttonContainer.FindEnabledOption( clientCoords);
	}

	IOption FindHotSpot( Point clientCoords ) => _hotSpots.Keys.FirstOrDefault(key=>_hotSpots[key].Contains(clientCoords));

	#region User Action Events - Notify main form


	public event Action<IOption> OptionSelected;

	#endregion

	static Font UseGameFont( float fontHeight ) => ResourceImages.Singleton.UseGameFont( fontHeight );

	#region private Option fields

	public void OptionProvider_OptionsChanged( IDecision decision ) {
		_decision = decision;

		// !!! Buttonize these
		options_GrowthActions = decision.Options.OfType<GrowthActionFactory>().ToArray(); // Need to update when starlight adds new
		_cardData.HandleNewDecision( decision ); // Need to add/remove when user changes views

		// !!! Buttonize Pop-ups - need to add dynamically and be able to remove themselves when done/clicked
		options_FearPopUp = decision.Options.OfType<IFearCard>().FirstOrDefault();
		options_BlightPopUp = decision.Options.OfType<IBlightCard>().FirstOrDefault();

		// !!! ADD Special Rules (Spirit) button click - BTN needs to handle its own Click event
		// !!! ADD Adversary Button click - BTN needs to handle its own Click event

		// Dialog Style Popup where multiple buttons are on a common background, and only draw when active.
		decision_DeckToDrawFrom = decision as Select.DeckToDrawFrom;
		decision_Element        = decision as Select.Element;

		// Option Buttons
		_buttonContainer.EnableOptions(_decision);

		Invalidate();
	}

	IDecision _decision;

	Select.DeckToDrawFrom            decision_DeckToDrawFrom;
	Select.Element                   decision_Element;

	IFearCard				options_FearPopUp;
	IBlightCard		        options_BlightPopUp;
	GrowthActionFactory[]   options_GrowthActions;

	#endregion

	#region private Misc fields

	// Stores the locations of ALL SpaceTokens (invaders, dahan, presence, wilds, disease, beast, etx)
	// When we are presented with a decision, the location of each option is pulled from here
	// and added to the HotSpots.
	readonly Dictionary<SpaceToken, Rectangle> _tokenLocations = new Dictionary<SpaceToken, Rectangle>();
	readonly Dictionary<IOption, RectangleF> _hotSpots = new();


	readonly CardUi _cardData;
	SpiritPainter _spiritPainter;
	GameState _gameState;
	Spirit _spirit;

	#endregion

	#region Cached Image Resources

	void LoadStandardTokenImages() {
		var images = ResourceImages.Singleton;
		_strife = images.Strife();
		_fearTokenImage = images.Fear();
		_grayFear = images.FearGray();
		_tokenImages = new Dictionary<ISpaceEntity, Image> {
			[Token.Blight] = images.GetImage( Img.Blight ),
			[Token.Beast] = images.GetImage( Img.Beast ),
			[Token.Wilds] = images.GetImage( Img.Wilds ),
			// [Token.Disease] = images.GetImage( Img.Disease ),
			[Token.Badlands] = images.GetImage( Img.Badlands ),
			// assign slot so we can access via key when we need to ?.Dispose() when initializing spirit
			[Token.Defend] = null,
			[Token.Isolate] = null,
		};
	}

	Image _presenceImg;
	Image _strife;
	Image _fearTokenImage;
	Image _grayFear;
	Dictionary<ISpaceEntity, Image> _tokenImages; // because we need different images for different damaged invaders.

	#endregion

	#region Color & Appearance

	const int hotspotRadius = 40;
	static Color ArrowColor => Color.DeepSkyBlue;
	static Color SacredSiteColor => Color.Yellow;
	static Color SpacePerimeterColor => Color.Black;
	static Color MultiSpacePerimeterColor => Color.Gold;

	// Brushes for Text
	static Brush SpaceLabelBrush => Brushes.White;
	static Brush CardLabelBrush => Brushes.Black;
	static Brush GameTextBrush_Default => Brushes.Black;
	static Brush GameTextBrush_Victory => Brushes.DarkGreen;
	static Brush GameTextBrush_Defeat => Brushes.DarkRed;

	// Fill / Background
	static Brush EmptySlotBrush => Brushes.DarkGray;
	static Color PopupBorderColor => Color.DarkGray;
	static Brush PopupBackgroundBrush => Brushes.DarkGray;
	static Brush SpiritPanelBackgroundBrush => Brushes.LightYellow;

	static Color SpaceColor( Space space )
		=> space.IsWetland ? Color.LightBlue
		: space.IsSand ? Color.PaleGoldenrod
		: space.IsMountain ? Color.Gray
		: space.IsJungle ? Color.ForestGreen
		: space.IsOcean ? Color.Blue
		: Color.Gold;

	#endregion

	public bool Debug {
		get { return _debug; }
		set { _debug = value; Invalidate(); }
	}
	bool _debug;

}
