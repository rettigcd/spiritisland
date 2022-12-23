using SpiritIsland.Basegame;
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
	}

	public void Init( GameState gameState, IHaveOptions optionProvider, PresenceTokenAppearance presenceAppearance, AdversaryConfig adversary ) {
		// Dispose old spirit tokens
		_presenceImg?.Dispose();
		_tokenImages[TokenType.Defend]?.Dispose();
		_tokenImages[TokenType.Isolate]?.Dispose();

		spiritLayout = null;

		optionProvider.NewDecision += OptionProvider_OptionsChanged;

		ClearCachedBackgroundImage();

		_gameState = gameState;
		_spirit    = gameState.Spirits.Single();
		_adversary = adversary;

		_spiritPainter = new SpiritPainter( _spirit, presenceAppearance );
		
		// Init new Presence, Defend, Isolate
		_presenceImg = ResourceImages.Singleton.GetPresenceImage( presenceAppearance.BaseImage );
		_tokenImages[TokenType.Defend] = ResourceImages.Singleton.GetImage( Img.Defend );
		_tokenImages[TokenType.Isolate] = ResourceImages.Singleton.GetImage( Img.Isolate );
		presenceAppearance.Adjustment?.Adjust( (Bitmap)_presenceImg );
		presenceAppearance.Adjustment?.Adjust( (Bitmap)_tokenImages[TokenType.Defend] );
		presenceAppearance.Adjustment?.Adjust( (Bitmap)_tokenImages[TokenType.Isolate] );
	}

	#endregion constructor / Init

	#region Calc Layout

	// Layout
	const float gameLabelFontHeight = .05f;

	const float spiritWidth = .35f; // % of screen width to use for spirit
	const float oceanWidth = 1f - spiritWidth;
	const float invaderDeckWidth = oceanWidth * .4f;
	const float invaderDeckHeight = .3f;
	const float fearWidth = .2f;
	const float fearHeight = fearWidth * .2f;
	const float blightWidth = .15f;
	const float blightHeight = .1f;
	Rectangle SpiritRect => new Rectangle( Width - (int)(spiritWidth * Width), 0, (int)(spiritWidth * Width), Height );
	RectangleF CalcFearPoolRect => new RectangleF( Width * (oceanWidth - fearWidth), 0f, Width * fearWidth, Width * fearHeight );
	RectangleF CalcAdversaryFlagRect => new RectangleF( 10, 10 + (gameLabelFontHeight)*Height, Width * .05f, Width * .033f );
	RectangleF CalcBlightRect => new RectangleF( Width*(oceanWidth-blightWidth), Height*(1-invaderDeckHeight-blightHeight), Width * blightWidth, Height * blightHeight );
	RectangleF CalcInvaderCardRect => new RectangleF( 
		Width * (oceanWidth - invaderDeckWidth), 
		Height * (1f - invaderDeckHeight), 
		Width * invaderDeckWidth, 
		Height * invaderDeckHeight
	);
	Rectangle CalcPopupFearRect() {
		var bounds = new Rectangle( 0, 0, (int)(Width * .65f), Height );
		// Active Fear Layout
		int fearHeight = (int)(bounds.Height * .8f);
		var fearWidth = fearHeight * 2 / 3;
		var bob = new Rectangle( bounds.Width - fearWidth - (int)(bounds.Height * .1f), (bounds.Height - fearHeight) / 2, fearWidth, fearHeight );
		return bob;
	}
	RectangleF CalcIslandExtents() {
		float left = float.MaxValue;
		float top = float.MaxValue;
		float right = float.MinValue;
		float bottom = float.MinValue;
		foreach(var board in _gameState.Island.Boards) {
			var e = board.Layout.CalcExtents();
			if(e.Left < left) left = e.Left;
			if(e.Top < top) top = e.Top;
			if(e.Right > right) right = e.Right;
			if(e.Bottom > bottom) bottom = e.Bottom;
		}
		return new RectangleF( left, top, right - left, bottom - top );
	}
	RectangleF CalcElementPopUpBounds( int count ) {
		// calculate layout based on count
		float maxHeight = Height * .16f;
		float maxWidth = Width * (oceanWidth * .75f);
		int desiredMmargin = 20;

		var maxBounds = new RectangleF( (Width - maxWidth) / 2, (Height - maxHeight) / 2, maxWidth, maxHeight ); // max size allowed to use
		var desiredSize = new SizeF( count * (maxHeight - desiredMmargin) + desiredMmargin, maxHeight ); // share we want.
		RectangleF bounds = maxBounds.FitBoth( desiredSize );
		return bounds;
	}

	SpiritLayout spiritLayout;
	Rectangle popUpFearRect;
	AdversaryConfig _adversary;

	void CalcSpiritLayout( Graphics graphics, Rectangle bounds ) {
		spiritLayout = new SpiritLayout( graphics, _spirit, bounds, 10 );
		growthOptionCount = _spirit.GrowthTrack.Options.Length;
	}
	int growthOptionCount = -1; // auto-update when Starlight adds option

	/// <summary>
	/// Maps from normallized space to Client space.
	/// </summary>
	PointMapper _mapper;

	PointMapper MapWorldToViewPort( RectangleF worldRect ) {
		var upperLeft = new PointF( 20f, 60f );
		float usableHeight = (this.Height - upperLeft.Y - 20 );

		// calculate scaling Assuming height-limited
		float islandHeight = worldRect.Height; // (float)(0.5 * Math.Sqrt( 3 )); // each board size is 1. Equalateral triangle height is sqrt(3)/2
		float scale = usableHeight / islandHeight;

		return new PointMapper(
			  RowVector.Translate( -worldRect.X, -worldRect.Y ) // translate to origin
			* RowVector.Scale( scale, -scale ) // flip-y and scale
			* RowVector.Translate( upperLeft.X, upperLeft.Y + usableHeight ) // translate to view port
		);
	}

	static Rectangle FitClientBounds( Rectangle bounds ) {
		const float windowRatio = 1.05f;
		if(bounds.Width > bounds.Height / windowRatio) {
			int widthClip = bounds.Width - (int)(bounds.Height / windowRatio);
			bounds.X += widthClip;
			bounds.Width -= widthClip;
		} else
			bounds.Height = (int)(bounds.Width * windowRatio);
		return bounds;
	}

	static Rectangle FitWidth( float x, float y, float width, Size sz ) {
		return new Rectangle( (int)x, (int)y, (int)width, (int)(width / sz.Width * sz.Height) );
	}

	#endregion Calc Layout

	protected override void OnPaint( PaintEventArgs pe ) {
		base.OnPaint( pe );

		optionRects.Clear();
		_tokenLocations.Clear();
		hotSpots.Clear(); // Clear this at beginning so any of the DrawX methods can add to it

		if(_gameState is null) return;

		StopWatch.timeLog.Clear();

		using(new StopWatch( "Total" )) {

			DrawBoard_Static( pe );

			DrawGameRound( pe.Graphics );

			// mostly static
			DrawAdversary( pe );
			DrawFearPool( pe.Graphics );
			DrawBlight( pe.Graphics );
			DrawInvaderCards( pe.Graphics ); // other than highlights, do this last since it contains the Fear Card that we want to be on top of everything.

			// Island / Spaces
			foreach(SpaceState space in _gameState.AllSpaces)
				DecorateSpace( pe.Graphics, space );

			// Pop-ups
			DrawDeckPopUp( pe.Graphics );
			DrawElementsPopUp( pe.Graphics );
			DrawFearPopUp( pe.Graphics );

			using(var hotSpotPen = new Pen( HotSpotColor, 5 )) {
				DrawHotSpots_SpaceToken( pe.Graphics, hotSpotPen );
				DrawHotSpots_Space( pe.Graphics, hotSpotPen );
			}

			DrawSpirit( pe.Graphics, SpiritRect );

			// pe.Graphics.DrawRectangle(Pens.Red, CalcInvaderCardRect.ToInts() );
			// pe.Graphics.DrawRectangle( Pens.Green, CalcBlightRect.ToInts() );

		}

		// non drawing - record Hot spots
		RecordSpiritHotspots();
		if(options_FearPopUp is not null) 
			hotSpots.Add(options_FearPopUp,popUpFearRect);

	}

	#region Draw Static Board

	void DrawBoard_Static( PaintEventArgs pe ) {
		using var stopwatch = new StopWatch( "Island-static" );

		if(_cachedBackground == null) {

			// Map world-coord island Rect onto viewport
			var boardWorldRect = CalcIslandExtents();
			_mapper = MapWorldToViewPort( boardWorldRect );

			// create a viewport size screen we can draw on.
			bool limitIsWidth = (boardWorldRect.Width * Height > Width * boardWorldRect.Height);
			_boardScreenSize = (limitIsWidth)
				? new Size( Width, (int)(boardWorldRect.Height * Width / boardWorldRect.Width) )
				: new Size( (int)(boardWorldRect.Width * Height / boardWorldRect.Height), Height );
			_cachedBackground = new Bitmap( _boardScreenSize.Width, _boardScreenSize.Height );
			using var graphics = Graphics.FromImage( _cachedBackground );

			foreach(var board in _gameState.Island.Boards)
				DrawBoardSpacesOnly( graphics, board );

			// calc spots to put tokens
			_insidePoints = _gameState.AllSpaces
				.ToDictionary(  ss => ss.Space,  ss => new ManageInternalPoints( ss ) );

			// DrawInnerPoints( graphics );
			foreach(var x in _insidePoints)
				graphics.DrawString( x.Key.Text, SystemFonts.MessageBoxFont, SpaceLabelBrush, _mapper.Map( x.Value.NameLocation ) );

		}

		pe.Graphics.DrawImage( _cachedBackground, 0, 0, _cachedBackground.Width, _cachedBackground.Height );
	}

	void DrawBoardSpacesOnly( Graphics graphics, Board board ) {
		var perimeterPen = new Pen( SpacePerimeterColor, 5f );
		var normalizedBoardLayout = board.Layout;
		for(int i = 0; i < normalizedBoardLayout.Spaces.Length; ++i) {
			using Brush brush = UseSpaceBrush( board[i] );
			var points = normalizedBoardLayout.Spaces[i].Corners.Select( _mapper.Map ).ToArray();

			// Draw blocky
			//pe.Graphics.FillPolygon( brush, points );
			//pe.Graphics.DrawPolygon( perimeterPen, points );

			// Draw smoothy
			graphics.FillClosedCurve( brush, points, System.Drawing.Drawing2D.FillMode.Alternate, .25f );
			graphics.DrawClosedCurve( perimeterPen, points, .25f, System.Drawing.Drawing2D.FillMode.Alternate );
		}

	}

	#endregion Draw Static Board

	void DrawGameRound( Graphics graphics ) {
		using Font font = UseGameFont( Height * gameLabelFontHeight );

		Brush brush = GameTextBrush_Default;
		string snippet = "Fight!";

		// If game is over, update
		if(_gameState.Result != null) {

			brush = _gameState.Result.Result == GameOverResult.Victory ? GameTextBrush_Victory : GameTextBrush_Defeat;
			snippet = _gameState.Result.Msg();
		}
		graphics.DrawString( $"Round {_gameState.RoundNumber} - {snippet}", font, brush, 0, 0 );

	}

	#region Draw - Fear, Blight, Invader Cards

	void DrawAdversary( PaintEventArgs pe ) {
		if(_adversary != null) {
			using var flag = ResourceImages.Singleton.GetAdversaryFlag( _adversary.Name );
			pe.Graphics.DrawImage( flag, this.CalcAdversaryFlagRect );
		}
	}

	void DrawFearPool( Graphics graphics ) {
		var outterBounds = CalcFearPoolRect;
		float margin = Math.Max( 5f, outterBounds.Height * .05f );
		var bounds = outterBounds.InflateBy(-margin);

		using var cardImg = ResourceImages.Singleton.FearCard(); // Maybe load this with the control and not dispose of it every time we draw.

		float slotWidth = bounds.Width/6;

		// -1 slot width for #/# and 
		// -1 slot width for last fear token
		var tokenSize = new SizeF( slotWidth, slotWidth * _fearTokenImage.Height / _fearTokenImage.Width ); // assume token is wider than tall.

		// Calc Terror Level bounds - slotWidth reserved but only tokenSize.Width used
		var terrorLevelBounds = new RectangleF( bounds.X, bounds.Y, tokenSize.Width, tokenSize.Height );

		// Calc Fear Pool bounds - skip 1 slotWidth, 
		int poolMax = this._gameState.Fear.PoolMax;
		float step = (bounds.Width - 4 * slotWidth) / (poolMax - 1);
		RectangleF CalcBounds( int i ) => new RectangleF( bounds.X + slotWidth + step * i, bounds.Y, tokenSize.Width, tokenSize.Height );

		// Calc Activated Fear Bounds
		var activatedCardRect = new RectangleF( bounds.Right - slotWidth*2, bounds.Y, tokenSize.Width, tokenSize.Height ).FitHeight( cardImg.Size ).ToInts();
		var futureCardRect = new RectangleF( bounds.Right - slotWidth, bounds.Y, tokenSize.Width, tokenSize.Height ).FitHeight( cardImg.Size ).ToInts();


		// Draw Terror Level
		using var terror = ResourceImages.Singleton.TerrorLevel( _gameState.Fear.TerrorLevel );
		graphics.DrawImage( terror, terrorLevelBounds );

		// Draw Fear Pool
		int fearCount = this._gameState.Fear.EarnedFear;
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
				using Image img = new FearCardImageManager().GetImage( top );
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
				using Image img = new FearCardImageManager().GetImage( top );
				graphics.DrawImage( img, futureCardRect );
			} else {
				// Draw Card
				graphics.DrawImage( cardImg, futureCardRect );
			}
			graphics.DrawCountIfHigherThan( futureCardRect, future );
			futureCardRect.Location = new Point( futureCardRect.X, futureCardRect.Bottom);
			using var cardCountFont = UseGameFont( margin * 3);
			graphics.DrawStringCenter( string.Join(" / ",_gameState.Fear.CardsPerLevelRemaining), cardCountFont, CardLabelBrush, futureCardRect);
		}
	}

	void DrawBlight( Graphics graphics ) {
		var bounds = CalcBlightRect;

		float margin = Math.Max( 5f, bounds.Height * .05f );
		float slotWidth = bounds.Height;

		int count = this._gameState.blightOnCard;
		int maxSpaces = 6;

		float step = (bounds.Width - 2 * margin - 2 * slotWidth) / (maxSpaces - 1);
		// -1 slot width for #/# and 
		// -1 slot width for last fear token
		float tokenWidth = slotWidth - 2 * margin;
		float tokenHeight = _fearTokenImage.Height * tokenWidth / _fearTokenImage.Width;
		RectangleF CalcBounds( int i ) => new RectangleF( bounds.X + slotWidth + margin + step * i, bounds.Y + margin, tokenWidth, tokenHeight );

		// draw fear tokens
		var img = this._tokenImages[TokenType.Blight];
		for(int i = 0; i < count; ++i)
			graphics.DrawImage( img, CalcBounds( i ) );

		if(_gameState.BlightCard.CardFlipped) {
			using var blightedFont = UseGameFont( slotWidth * .2f );
			graphics.DrawString( "Blighted!", blightedFont, BlightedTextBrush, bounds.Right - slotWidth * 1.5f, bounds.Top );
		}
	}

	void DrawInvaderCards( Graphics graphics ) {

		const float margin = 8;
		const float textHeight = 20f;

		var bounds = CalcInvaderCardRect;

		// Calculate Card Size based on # of slots
		float slots = _gameState.InvaderDeck.Slots.Count + 1.5f;
		float slotWidth = bounds.Width / slots;

		var cardSize = slotWidth * 1.5f < bounds.Height         // too narrow
			? new SizeF( slotWidth, slotWidth * 1.5f )         // use narrow width to limit height
			: new SizeF( bounds.Height / 1.5f, bounds.Height ); // plenty of width, use height to determine size

		// calc discard location
		var discardRect = new RectangleF( bounds.Left, bounds.Top + margin + (cardSize.Height - cardSize.Width) * .5f, cardSize.Height - margin * 2, cardSize.Width - margin * 2 );
		Point[] discardDestinationPoints = {
			new Point((int)discardRect.Left, (int)discardRect.Bottom),    // destination for upper-left point of original
			new Point((int)discardRect.Left, (int)discardRect.Top), // destination for upper-right point of original
			new Point((int)discardRect.Right, (int)discardRect.Bottom)      // destination for lower-left point of original
		};

		// locate each of the cards
		var cardMetrics = new InvaderCardMetrics[_gameState.InvaderDeck.Slots.Count];

		for(int i = 0; i < cardMetrics.Length; ++i)
			cardMetrics[i] = new InvaderCardMetrics( _gameState.InvaderDeck.Slots[i],
				bounds.Left + (i + 1.5f) * cardSize.Width + margin, //left+i*xStep, 
				bounds.Top + margin, // y, 
				cardSize.Width - margin * 2, cardSize.Height - margin * 2, // width, height, 
				textHeight
			);

		// Draw
		using var buildRavageFont = UseGameFont( textHeight );
		using var invaderStageFont = UseGameFont( textHeight * 2 );
		foreach(var cardMetric in cardMetrics)
			cardMetric.Draw( graphics, buildRavageFont, invaderStageFont );

		// # of cards in explore pile
		graphics.DrawCountIfHigherThan( cardMetrics.Last().Rect.First(), _gameState.InvaderDeck.UnrevealedCards.Count+1 );

		// Draw Discard
		var lastDiscard = _gameState.InvaderDeck.Discards.FirstOrDefault();
		if(lastDiscard is not null) {
			using Bitmap discardImg = ResourceImages.Singleton.GetInvaderCard( lastDiscard.Text );
			graphics.DrawImage( discardImg, discardDestinationPoints );
		}

	}

	#endregion Draw - Fear, Blight, Invader Cards

	#region Draw - Board Spaces & Tokens

	void DecorateSpace( Graphics graphics, SpaceState spaceState ) {
		if(spaceState.Space is MultiSpace ms)
			DrawMultiSpace( graphics, ms );

		float iconWidth = _boardScreenSize.Width * .040f; // !!! scale tokens based on board/space size, NOT widow size (for 2 boards, tokens are too big)
		DrawInvaderRow( graphics, spaceState, iconWidth );
		DrawRow( graphics, spaceState, iconWidth );
	}

	void DrawInvaderRow( Graphics graphics, SpaceState ss, float iconWidth ) {

		var orderedInvaders = ss.Keys
			.Where( k => k.Class.Category == TokenCategory.Invader )
			.Cast<HealthToken>()
			// Major ordering: (Type > Strife)
			.OrderByDescending( i => i.FullHealth )
			.ThenBy( x => x.StrifeCount )
			// Minor ordering: (remaining health)
			.ThenBy( i => i.RemainingHealth ); // show damaged first so when we apply damage, the damaged one replaces the old undamaged one.

		foreach(Token token in orderedInvaders) {

			// New way
			PointF center = _mapper.Map( _insidePoints[ss.Space].GetPointFor( token, ss ) );
			float x = center.X-iconWidth/2;
			float y = center.Y-iconWidth/2; //!! approximate - need Image to get actual Height to scale

			// Strife
			Token imageToken;
			if(token is HealthToken si && 0 < si.StrifeCount) {
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

	void DrawRow( Graphics graphics, SpaceState spaceState, float iconWidth ) {
		var tokenTypes = new List<Token> {
			TokenType.Defend, TokenType.Blight, // These don't show up in .OfAnyType if they are dynamic
			TokenType.Beast, TokenType.Wilds, TokenType.Disease, TokenType.Badlands, TokenType.Isolate
		}	.Union( spaceState.OfCategory( TokenCategory.Dahan ) )
			.Union( spaceState.OfAnyClass( _spirit.Presence.Token ) )
			.Union( spaceState.OfAnyClass( TokenType.Element ) )
			.Union( spaceState.OfClass( TokenType.OpenTheWays ) )
			.ToArray();


		foreach(var token in tokenTypes) {
			int count = spaceState[token];
			if(count == 0) continue;

			bool isPresence = token is SpiritPresenceToken;
			Image img = isPresence ? _presenceImg : AccessTokenImage( token );

			// calc rect
			float iconHeight = iconWidth / img.Width * img.Height;

			PointF pt = _mapper.Map( _insidePoints[spaceState.Space].GetPointFor(token,spaceState) );
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

		var elementOptions = decision_Element.Options.OfType<ItemOption<Element>>().ToArray();
		int count = elementOptions.Length;

		RectangleF bounds = CalcElementPopUpBounds( count );

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
		foreach(var opt in elementOptions) {
			using var img = ResourceImages.Singleton.GetImage( opt.Item.GetTokenImg() );
			var rect = new RectangleF( x, y, contentSize, contentSize );
			graphics.DrawImage( img, rect );
			hotSpots.Add( opt, rect );

			x += (contentSize + actualMargin);
		}

	}

	void DrawDeckPopUp(Graphics graphics ) {
		if(decision_DeckToDrawFrom is null) return;

		// calc layout
		int boundsHeight = Height / 3; // cards take up 1/3 of window vertically
		int boundsWidth = boundsHeight * 16 / 10;
		Rectangle bounds = new Rectangle( 0 + (Width-boundsWidth)/2, Height - boundsHeight-20, boundsWidth, boundsHeight );

		Rectangle innerDeckBounds = bounds.InflateBy(-boundsHeight/20);
		var cardWidth = innerDeckBounds.Height * 3 / 4;
		var minorRect = new Rectangle(innerDeckBounds.X,innerDeckBounds.Y,cardWidth,innerDeckBounds.Height);
		var majorRect = new Rectangle(innerDeckBounds.Right-cardWidth,innerDeckBounds.Y,cardWidth,innerDeckBounds.Height);

		// Hotspots
		// !! we are assuming minor is first...
		hotSpots.Add(decision_DeckToDrawFrom.Options[0],minorRect);
		hotSpots.Add(decision_DeckToDrawFrom.Options[1],majorRect);

		graphics.FillRectangle(PopupBackgroundBrush,bounds);
		using var minorImage = Image.FromFile( ".\\images\\minor.png" );
		using var majorImage = Image.FromFile( ".\\images\\major.png" );
		graphics.DrawImage(minorImage,minorRect);
		graphics.DrawImage(majorImage,majorRect);
	}

	void DrawFearPopUp( Graphics graphics ) {
		if(options_FearPopUp is null) return;

		popUpFearRect = CalcPopupFearRect();

		using var img = new FearCardImageManager().GetImage( options_FearPopUp );
		graphics.DrawImage( img, popUpFearRect );
	}

	#endregion Draw - Pop Ups

	#region Draw HotSpots - spaces / space tokens

	void DrawHotSpots_SpaceToken( Graphics graphics, Pen pen ) {

		// adjacent
		if(decision_AdjacentInfo != null)
			DrawAdjacentArrows( graphics );

		// Draw SpaceToken option hotspots & record option location
		if(decision_SpaceToken != null)
			foreach(SpaceToken spaceToken in decision_SpaceToken.Options.OfType<SpaceToken>())
				DrawSpaceTokenHotspot( graphics, pen, spaceToken, spaceToken );

		// Draw Token option hotspot & record option location
		if(decision_TokenOnSpace != null)
			foreach(Token token in decision_TokenOnSpace.Options.OfType<Token>())
				DrawSpaceTokenHotspot( graphics, pen, token, new SpaceToken( decision_TokenOnSpace.Space, token ) );

		if(decision_DeployedPresence != null) {
			options_Space = null; // disable circle drawing
			foreach(Space space in decision_DeployedPresence.Options.OfType<Space>())
				DrawSpaceTokenHotspot( graphics, pen, space, new SpaceToken( space, _spirit.Presence.Token ) );
		}

	}

	void DrawHotSpots_Space( Graphics graphics, Pen pen ) {
		if(options_Space != null)
			foreach(Space space in options_Space) {
				PointF center = GetPortPoint( space ); // _mapper.Map( space.Layout.Center );
				graphics.DrawEllipse( pen, center.X - hotspotRadius, center.Y - hotspotRadius, hotspotRadius * 2, hotspotRadius * 2 );
			}
	}

	void DrawSpaceTokenHotspot(
		Graphics graphics,
		Pen pen,
		IOption option,  // the actual option to record
		SpaceToken st   // the effective SpaceToken (location) of the option
	) {
		if( !_tokenLocations.ContainsKey( st ) ) return; // does this ever happen?

		var rect = _tokenLocations[st];
		rect.Inflate( 2, 2 );
		optionRects.Add( (rect, option) );
		graphics.DrawRectangle( pen, rect );

		if(decision_AdjacentInfo != null) {
			if( decision_AdjacentInfo.Direction == SpiritIsland.Select.AdjacentDirection.Incoming ) {
				var from = new PointF( rect.X + rect.Width / 2, rect.Y + rect.Height / 2 );
				var to = _mapper.Map(
					_insidePoints[decision_AdjacentInfo.Central].GetPointFor( st.Token, _gameState.Tokens[decision_AdjacentInfo.Central] )
                );
				using var arrowPen = UsingArrowPen;
				graphics.DrawArrow( arrowPen, from, to );
			}
		}

	}

	PointF GetPortPoint( Space space ) {
		PointF worldCoord = decision_Token is null 
			? space.Layout.Center // normal space 
			: _insidePoints[space].GetPointFor( decision_Token, _gameState.Tokens[space] );
		return _mapper.Map( worldCoord );
	}

	void DrawAdjacentArrows( Graphics graphics ) {

		var center = GetPortPoint( decision_AdjacentInfo.Central );

		// !!! When gathering, Adjacent doesn't have Token info, only space info
		// So, for gathering, don't supply 
		var others = decision_AdjacentInfo.Adjacent
			.Select( x => GetPortPoint( x ) )
			.ToArray();

		using Pen p = UsingArrowPen;
		switch(decision_AdjacentInfo.Direction) {

			case SpiritIsland.Select.AdjacentDirection.Incoming:
				foreach(var other in others)
					graphics.DrawArrow( p, other, center );
				break;

			case SpiritIsland.Select.AdjacentDirection.Outgoing:
				foreach(var other in others)
					graphics.DrawArrow( p, center, other );
				break;
		}
	}
	static Pen UsingArrowPen => new Pen( ArrowColor, 7 );


	#endregion

	void DrawSpirit( Graphics graphics, Rectangle bounds ) {

		bounds = FitClientBounds( bounds );

		// Layout
		if( spiritLayout is null
			|| growthOptionCount != _spirit.GrowthTrack.Options.Length
		) {
			CalcSpiritLayout( graphics, bounds );
			_spiritPainter?.SetLayout( spiritLayout );
		}

		graphics.FillRectangle( SpiritPanelBackgroundBrush, bounds );
		_spiritPainter.Paint( graphics,
			options_InnatePower,
			options_DrawableInate,
			options_GrowthOptions,
			options_GrowthActions,
			options_Track
		);
	}

	void DrawMultiSpace( Graphics graphics, MultiSpace multi ) {

		using var pen = new Pen( MultiSpacePerimeterColor, 3f );

		using var brush = UseMultiSpaceBrush( multi );

		var points = multi.Layout.Corners.Select( _mapper.Map ).ToArray();
		graphics.FillClosedCurve( brush, points, FillMode.Alternate, .25f );
		graphics.DrawClosedCurve( pen, points, .25f, FillMode.Alternate );
		// graphics.FillPolygon( brush, points );
		// graphics.DrawPolygon( pen, points );

	}

	static LinearGradientBrush UseMultiSpaceBrush( MultiSpace multi ) {
		var brush = new LinearGradientBrush( new Rectangle( 0, 0, 30, 30 ), Color.Transparent, Color.Transparent, 45F );

		var colors = multi.Parts
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
		foreach(var opt in options_GrowthOptions)
			if(spiritLayout.growthLayout.HasOption( opt ))
				hotSpots.Add( opt, spiritLayout.growthLayout[opt] );
		foreach(var act in options_GrowthActions)
			if(spiritLayout.growthLayout.HasAction( act )) // there might be delayed setup actions here that don't have a rect
				hotSpots.Add( act, spiritLayout.growthLayout[act] );
		// Presence
		foreach(var track in options_Track)
			hotSpots.Add( track, spiritLayout.trackLayout.ClickRectFor( track ) );
		// Innates - Select Innate Power
		foreach(var power in options_InnatePower)
			hotSpots.Add( power, spiritLayout.findLayoutByPower[power].Bounds );

		// Innates - Select Innate Option (for shifting memory)
		var innateOptionBounds = spiritLayout.InnateLayouts
			.SelectMany( x => x.Options )
			.ToDictionary( x=> x.InnateOption, x=>x.Bounds );
		// Loop through the clickable innates
		foreach(IDrawableInnateOption innateOption in options_DrawableInate) {
			// Find the bounds that belongs to this Innate
			Rectangle bounds = innateOptionBounds[innateOption];
			hotSpots.Add( innateOption, bounds );
		}

	}

	static Brush UseSpaceBrush( Space space ) {
		string terrainName = space.IsWetland ? "wetlands"
			: space.IsJungle   ? "jungle"
			: space.IsMountain ? "mountains"
			: space.IsSand     ? "sand"
			: space.IsOcean    ? "ocean"
			: throw new ArgumentException($"No brush is found for {space.Text}",nameof(space));
		using Image image = Image.FromFile( $".\\images\\{terrainName}.jpg" );
		return new TextureBrush( image );
	}

	Image AccessTokenImage( Token imageToken ) {
		if(!_tokenImages.ContainsKey( imageToken ))
			_tokenImages[imageToken] = GetImage( imageToken );
		return _tokenImages[imageToken];
	}

	static Bitmap GetImage( Token token ) {
		return token is HealthToken ht 
			? GetHealthTokenImage( ht )
			: token.Class is UniqueToken ut 
				? ResourceImages.Singleton.GetImage( ut.Img )
				: throw new Exception( "unknown token " + token );
	}

	static Bitmap GetHealthTokenImage( HealthToken ht ) {

		Bitmap orig = ResourceImages.Singleton.GetImage( ht.Class.Img );

		// Invert Dreaming Invaders
		if( ToDreamAThousandDeaths.DreamInvaders.Contains( ht.Class )) {
			for(int x = 0; x < orig.Width; ++x)
				for(int y = 0; y < orig.Height; ++y) {
					var p = orig.GetPixel( x, y );
//					orig.SetPixel( x, y, Color.FromArgb( p.A, 255 - p.R, 255 - p.G, 255 - p.B ) ); // invert
//					orig.SetPixel( x, y, Color.FromArgb( p.A, p.R/2, p.G/2, p.B/2 ) ); // half scale
					orig.SetPixel( x, y, Color.FromArgb( p.A, p.R/2, p.G/2, p.B*2/3 ) ); // red/green:50% blue:66%
				}
		}

		using var g = Graphics.FromImage( orig );

		// If Full Health is different than standard, show it
		if( ht.FullHealth != ht.Class.ExpectedHealthHint ) {
			using var font = ResourceImages.Singleton.UseGameFont( orig.Height/2 );
			StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			g.DrawString( ht.FullHealth.ToString(), font, Brushes.White, new RectangleF( 0, 0, orig.Width, orig.Height ), center );
		}

		// Draw Damage slashes
		if( 0 < ht.FullDamage ) {
			int dX = orig.Width / ht.FullHealth;
			int lx = dX/2;
			// Normal Damage
			if( 0 < ht.Damage )
				using( Pen redSlash = new Pen( Color.FromArgb( 128, Color.Red ), 30f ) ) {
					for(int i = 0; i < ht.Damage; ++i) {
						g.DrawLine( redSlash, lx, orig.Height,lx+dX,0);
						lx += dX;
					}
				}
			// Dream Damage
			if(0 < ht.DreamDamage)
				using(Pen dreamSlash = new Pen( Color.FromArgb( 128, Color.MidnightBlue) , 30f )) { // or maybe steal blue
					for(int i = 0; i < ht.DreamDamage; ++i) {
						g.DrawLine( dreamSlash, lx, orig.Height, lx + dX, 0 );
						lx += dX;
					}
				}
		}

		return orig;
	}

	protected override void OnSizeChanged( EventArgs e ) {
		base.OnSizeChanged( e );
		RefreshLayout();
	}

	public void RefreshLayout() {
		spiritLayout = null;
		ClearCachedBackgroundImage();
		this.Invalidate();
	}

	void ClearCachedBackgroundImage() {
		if(_cachedBackground != null) {
			_cachedBackground.Dispose();
			_cachedBackground = null;
		}
	}

	protected override void OnClick( EventArgs e ) {
		var clientCoords = PointToClient( Control.MousePosition );
		IOption option = FindOption( clientCoords );
		if(option is Space space)
			SpaceClicked?.Invoke(space);
		else if(option is Token invader)
			TokenClicked?.Invoke( invader );
		else if(option is SpaceToken st)
			SpaceTokenClicked?.Invoke( st );
		else if( option != null )
			OptionSelected?.Invoke(option);

		// Spirit => Special Rules
		if(spiritLayout != null && spiritLayout.imgBounds.Contains( clientCoords )) {
			string msg = this._spirit.SpecialRules.Select(r=>r.ToString()).Join("\r\n\r\n");
			MessageBox.Show( msg );
		}

		// Adversay Rules
		if(_adversary != null && CalcAdversaryFlagRect.Contains( clientCoords ) )
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

		if(options_Space==null) return;

		bool inCircle = FindOption( this.PointToClient( Control.MousePosition ) ) != null;
		Cursor = inCircle ? Cursors.Hand : Cursors.Default;

	}

	IOption FindOption( Point clientCoords ) {
		return FindInvader( clientCoords )
			?? FindSpaces( clientCoords )
			?? FindHotSpot( clientCoords );
	}

	IOption FindSpaces( Point clientCoords ) {
		return options_Space?.Select( s => {
				PointF center = GetPortPoint( s );
				float dx = clientCoords.X - center.X;
				float dy = clientCoords.Y - center.Y;
				return new { Space = s, d2 = dx * dx + dy * dy };
			} )
			.Where( x => x.d2 < hotspotRadius * hotspotRadius )
			.OrderBy( x => x.d2 )
			.Select( x => x.Space )
			.FirstOrDefault();
	}

	IOption FindInvader( Point clientCoords ) {
		return optionRects
			.Where(t=>t.Item1.Contains(clientCoords))
			.Select(t=>t.Item2)
			.FirstOrDefault();
	}

	IOption FindHotSpot( Point clientCoords ) => hotSpots.Keys.FirstOrDefault(key=>hotSpots[key].Contains(clientCoords));

	#region User Action Events - Notify main form

	public event Action<IOption> OptionSelected;
	public event Action<Space> SpaceClicked;
	public event Action<Token> TokenClicked;
	public event Action<SpaceToken> SpaceTokenClicked;

	#endregion

	static public Font UseGameFont( float fontHeight ) => ResourceImages.Singleton.UseGameFont( fontHeight );

	#region private Option fields

	void OptionProvider_OptionsChanged( IDecision decision ) {

		// These decision_ variables contain additional info the options need to render them on the screen
		decision_TokenOnSpace        = decision as Select.TokenFrom1Space;				// Identifies option as SpaceToken
		decision_SpaceToken          = decision as Select.TypedDecision<SpaceToken>;	// Identifies option as SpaceToken
		decision_DeployedPresence    = decision as Select.DeployedPresence;				// Identifies option as SpaceToken
		decision_DeckToDrawFrom      = decision as Select.DeckToDrawFrom;
		decision_Element             = decision as Select.Element;
		decision_AdjacentInfo        = decision is Select.IHaveAdjacentInfo adjacenInfoProvider ? adjacenInfoProvider.AdjacentInfo : null;
		decision_Token               = decision is Select.IHaveTokenInfo ti ? ti.Token : null;

		// These option_ variables contain everything they need to render on screen
		options_Space         = decision.Options.OfType<Space>().ToArray();
		options_FearPopUp	  = decision.Options.OfType<IFearCard>().FirstOrDefault();
		options_Track		  = decision.Options.OfType<Track>().ToArray();
		options_InnatePower	  = decision.Options.OfType<InnatePower>().ToArray();
		options_DrawableInate = decision.Options.OfType<IDrawableInnateOption>().ToArray();
		options_GrowthOptions = decision.Options.OfType<GrowthOption>().ToArray();
		options_GrowthActions = decision.Options.OfType<GrowthActionFactory>().ToArray();
	}

	Token decision_Token; // the already-known token associated with a decision.  Like presence being placed or token that is being pushed to a destination.
	Select.TokenFrom1Space           decision_TokenOnSpace;
	Select.AdjacentInfo              decision_AdjacentInfo;
	Select.TypedDecision<SpaceToken> decision_SpaceToken;
	Select.DeployedPresence          decision_DeployedPresence;
	Select.DeckToDrawFrom            decision_DeckToDrawFrom;
	Select.Element                   decision_Element;

	IFearCard				options_FearPopUp;
	Track[]                 options_Track; // until 1st decision is available
	InnatePower[]           options_InnatePower;
	IDrawableInnateOption[] options_DrawableInate;
	GrowthOption[]          options_GrowthOptions;
	GrowthActionFactory[]   options_GrowthActions;
	Space[]					options_Space;

	readonly List<(Rectangle,IOption)> optionRects = new List<(Rectangle, IOption)>();

	#endregion

	#region private Misc fields

	// Stores the locations of ALL SpaceTokens (invaders, dahan, presence, wilds, disease, beast, etx)
	// When we are presented with a decision, the location of each option is pulled from here
	// and added to the HotSpots.
	readonly Dictionary<SpaceToken, Rectangle> _tokenLocations = new Dictionary<SpaceToken, Rectangle>();

	Dictionary<Space, ManageInternalPoints> _insidePoints;

	SpiritPainter _spiritPainter;
	GameState _gameState;
	Spirit _spirit;

	Size _boardScreenSize;
	Bitmap _cachedBackground;

	readonly Dictionary<IOption, RectangleF> hotSpots = new();

	#endregion

	#region Cached Image Resources

	void LoadStandardTokenImages() {
		var images = ResourceImages.Singleton;
		_strife = images.Strife();
		_fearTokenImage = images.Fear();
		_grayFear = images.FearGray();
		_tokenImages = new Dictionary<Token, Image> {
			[TokenType.Blight] = images.GetImage( Img.Blight ),
			[TokenType.Beast] = images.GetImage( Img.Beast ),
			[TokenType.Wilds] = images.GetImage( Img.Wilds ),
			[TokenType.Disease] = images.GetImage( Img.Disease ),
			[TokenType.Badlands] = images.GetImage( Img.Badlands ),
			// assign slot so we can access via key when we need to ?.Dispose() when initializing spirit
			[TokenType.Defend] = null,
			[TokenType.Isolate] = null,
		};
	}

	Image _presenceImg;
	Image _strife;
	Image _fearTokenImage;
	Image _grayFear;
	Dictionary<Token, Image> _tokenImages; // because we need different images for different damaged invaders.

	#endregion

	#region Color & Appearance

	const float hotspotRadius = 40f;
	static Color ArrowColor => Color.DeepSkyBlue;
	static Color SacredSiteColor => Color.Yellow;
	static Color HotSpotColor => Color.Aquamarine;
	static Color SpacePerimeterColor => Color.Black;
	static Color MultiSpacePerimeterColor => Color.Gold;

	// Brushes for Text
	static Brush SpaceLabelBrush => Brushes.White;
	static Brush CardLabelBrush => Brushes.Black;
	static Brush BlightedTextBrush => Brushes.Red;
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

}


class InvaderCardMetrics {
	public InvaderCardMetrics( InvaderSlot slot, float x, float y, float width, float height, float textHeight ) {
		this.slot = slot;

		// Individual card rects
		int count = slot.Cards.Count;
		Rect = new RectangleF[count];
		float buildWidth = width / count, buildHeight = height / count;
		for(int i = 0; i < Rect.Length; ++i)
			Rect[i] = new RectangleF( x + i * buildWidth, y + i * buildHeight, buildWidth, buildHeight );

		// Text location
		textBounds = new RectangleF( x, y + height + textHeight * .1f, width, textHeight * 1.5f );
	}
	public readonly InvaderSlot slot;
	public readonly RectangleF[] Rect;
	public readonly RectangleF textBounds;

	public void Draw( Graphics graphics, Font labelFont, Font invaderStageFont ) {
		// Draw all of the cards in that slot
		// !! we could make them overlap and bigger
		for(int i = 0; i < Rect.Length; ++i) {
			var card = slot.Cards[i];
			if(card.Flipped)
				graphics.DrawInvaderCardFront( Rect[i], card );
			else {
				var cardRect = Rect[i];
				using(SolidBrush brush = new SolidBrush( Color.LightSteelBlue ))
					graphics.FillRectangle( brush, cardRect );
				var smallerRect = cardRect.InflateBy( -cardRect.Width * .1f );
				graphics.DrawInvaderCardBack( smallerRect, card );
				smallerRect = cardRect.InflateBy( -25f );
				graphics.DrawStringCenter( card.InvaderStage.ToString(), invaderStageFont, Brushes.DarkRed, smallerRect );
			}
		}
		graphics.DrawStringCenter( slot.Label, labelFont, Brushes.Black, textBounds );
	}

}

