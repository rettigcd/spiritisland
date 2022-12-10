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
	3a) DecorateSpace => DrawRow => Record where Tokens cards, etc are located
	3b) Other controls draw themselves and record location
	4) DrawHotspots => foreeach Option => Find location and draw it.
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

	}

	public void Init( GameState gameState, IHaveOptions optionProvider, string presenceColor ) {

		spiritLayout = null;

		optionProvider.NewDecision += OptionProvider_OptionsChanged;

		var board = gameState.Island.Boards.VerboseSingle("Multiple Island boards not supported.");
		ClearCachedImage();
		var boardLabel = board[0].Label;
		var images = ResourceImages.Singleton;
		presence = images.GetPresenceIcon( presenceColor );
		this.presenceColor = presenceColor;
		strife   = images.Strife();
		fear     = images.Fear();
		grayFear = images.FearGray();

		tokenImages = new Dictionary<Token, Image> {
			[TokenType.Defend]    = images.GetImage( Img.Defend ),
			[TokenType.Blight]    = images.GetImage( Img.Blight ),
			[TokenType.Beast]     = images.GetImage( Img.Beast ),
			[TokenType.Wilds]     = images.GetImage( Img.Wilds ),
			[TokenType.Disease]   = images.GetImage( Img.Disease ),
			[TokenType.Badlands]  = images.GetImage( Img.Badlands ),
			[TokenType.Isolate]   = images.GetImage( Img.Isolate ),
		};

		this.gameState = gameState;
		this.spirit = gameState.Spirits.Single();
	}

	#endregion constructor / Init

	#region Paint

	// Layout
	const float spiritWidth = .35f; // % of screen width to use for spirit
	const float oceanWidth = 1f - spiritWidth;
	const float invaderDeckWidth = oceanWidth * .4f;
	const float invaderDeckHeight = .3f;
	Rectangle SpiritRect => new Rectangle( Width - (int)(spiritWidth * Width), 0, (int)(spiritWidth * Width), Height );
	RectangleF CalcFearPoolRect => new RectangleF( Width * .50f, 0f, Width * .20f, Width * .04f );
	RectangleF CalcBlightRect => new RectangleF( Width * .55f, Width * .05f, Width * .15f, Width * .03f );
	RectangleF CalcInvaderCardRect => new RectangleF( Width * (oceanWidth-invaderDeckWidth), Height*(1f-invaderDeckHeight), (Width * invaderDeckWidth), Height*invaderDeckHeight );
	Rectangle CalcPopupFearRect() {
		var bounds = new Rectangle( 0, 0, (int)(Width * .65f), Height );
		// Active Fear Layout
		int fearHeight = (int)(bounds.Height * .8f);
		var fearWidth = fearHeight * 2 / 3;
		var bob = new Rectangle( bounds.Width - fearWidth - (int)(bounds.Height * .1f), (bounds.Height - fearHeight) / 2, fearWidth, fearHeight );
		return bob;
	}


	protected override void OnPaint( PaintEventArgs pe ) {
		base.OnPaint( pe );

		optionRects.Clear();
		tokenLocations.Clear();
		hotSpots.Clear(); // Clear this at beginning so any of the DrawX methods can add to it

		if(gameState == null) return;

		StopWatch.timeLog.Clear();

		using(new StopWatch( "Total" )) {

			DrawBoard_Static( pe );

			DrawGameRound( pe.Graphics );

			// mostly static
			DrawFearPool( pe.Graphics );
			DrawBlight( pe.Graphics );
			DrawInvaderCards( pe.Graphics ); // other than highlights, do this last since it contains the Fear Card that we want to be on top of everything.

			// Island / Spaces
			foreach(SpaceState space in gameState.AllSpaces)
				DecorateSpace(pe.Graphics,space);

			// Pop-ups
			DrawDeckPopUp(pe.Graphics);
			DrawElementsPopUp( pe.Graphics );
			DrawFearPopUp( pe.Graphics );

			using(var hotSpotPen = new Pen( Brushes.Aquamarine, 5 )) {
				DrawHotSpots_SpaceToken( pe.Graphics, hotSpotPen );
				DrawHotSpots_Space( pe.Graphics, hotSpotPen );
			}

			DrawSpirit( pe.Graphics, SpiritRect );
		}

		// non drawing - record Hot spots
		RecordSpiritHotspots();
		if(options_FearCard!= null) 
			hotSpots.Add(options_FearCard,activeFearRect);

	}

	RectangleF IslandExtents() {
		float left = float.MaxValue;
		float top = float.MaxValue;
		float right = float.MinValue;
		float bottom = float.MinValue;
		foreach(var board in gameState.Island.Boards) {
			var e = board.Layout.CalcExtents();
			if(e.Left<left) left = e.Left;
			if(e.Top<top) top = e.Top;
			if(e.Right>right) right = e.Right;
			if(e.Bottom>bottom) bottom = e.Bottom;
		}
		return new RectangleF(left, top, right - left, bottom - top);
	}

	/// <summary>
	/// Maps from normallized space to Client space.
	/// </summary>
	PointMapper _mapper;

	void DrawBoard_Static( PaintEventArgs pe ) {
		using var stopwatch = new StopWatch( "Island-static" );

		if(cachedBackground == null) {

			spaceLookup = new Dictionary<Space, SpaceLayout>();
			var boardRect = IslandExtents();
			// Assume limit is height
			bool bb = (boardRect.Width * Height > Width * boardRect.Height);
			boardScreenSize = (bb)
				? new Size( Width, (int)(boardRect.Height * Width / boardRect.Width) )
				: new Size( (int)(boardRect.Width * Height / boardRect.Height), Height );
			_mapper = SetupSingleBoardTransform(boardRect);

			cachedBackground = new Bitmap( boardScreenSize.Width, boardScreenSize.Height );
			using var graphics = Graphics.FromImage( cachedBackground );

			foreach(var board in gameState.Island.Boards) {
				for(int i = 0; i <= 8; ++i)
					spaceLookup.Add( board[i], board.Layout.Spaces[i] );
				DrawBoardSpacesOnly( graphics, board );
			}

			// -- new --
			// mapper = SetupIsland1of2();
			// mapper = SetupIsland2of2();

		}

		pe.Graphics.DrawImage( cachedBackground, 0, 0, cachedBackground.Width, cachedBackground.Height );
	}


	static Brush SpaceBrush( Space space ) {
		if( space.IsWetland ) {
			using Image image = Image.FromFile( ".\\images\\wetlands.jpg" );
			TextureBrush tBrush = new TextureBrush( image );
			tBrush.Transform = new Matrix(
				0.25f, 0.0f, 0.0f,
				0.25f, 0.0f, 0.0f
			);
			return tBrush;
		}
		if(space.IsJungle) {
			using Image image = Image.FromFile( ".\\images\\jungle.jpg" );
			TextureBrush tBrush = new TextureBrush( image );
			tBrush.Transform = new Matrix(
				1f, 0.0f, 0.0f,
				1f, 0.0f, 0.0f
			);
			return tBrush;
		}
		if(space.IsMountain) {
			using Image image = Image.FromFile( ".\\images\\mountains.jpg" );
			TextureBrush tBrush = new TextureBrush( image );
			tBrush.Transform = new Matrix(
				0.5f, 0.0f, 0.0f,
				0.5f, 0.0f, 0.0f
			);
			return tBrush;
		}
		if(space.IsSand) {
			using Image image = Image.FromFile( ".\\images\\sand.jpg" );
			TextureBrush tBrush = new TextureBrush( image );
			tBrush.Transform = new Matrix(
				0.5f, 0.0f, 0.0f,
				0.5f, 0.0f, 0.0f
			);
			return tBrush;
		}
		if(space.IsOcean) {
			using Image image = Image.FromFile( ".\\images\\ocean.jpg" );
			TextureBrush tBrush = new TextureBrush( image );
			tBrush.Transform = new Matrix(
				0.5f, 0.0f, 0.0f,
				0.5f, 0.0f, 0.0f
			);
			return tBrush;
		}
		return new SolidBrush( SpaceColor( space ) );
	}

	static Color SpaceColor( Space space ) {
		return space.IsWetland ? Color.LightBlue
			: space.IsSand     ? Color.PaleGoldenrod
			: space.IsMountain ? Color.Gray
			: space.IsJungle   ? Color.ForestGreen
			: space.IsOcean    ? Color.Blue
			: Color.Gold;
	}


	void DrawBoardSpacesOnly( Graphics graphics, Board board ) {
		var perimeterPen = new Pen( Brushes.Black, 5f );
		var normalizedBoardLayout = board.Layout;
		for(int i = 0; i < normalizedBoardLayout.Spaces.Length; ++i) {
			using Brush brush = SpaceBrush( board[i] );
			var points = normalizedBoardLayout.Spaces[i].corners.Select( _mapper.Map ).ToArray();

			// Draw blocky
			//pe.Graphics.FillPolygon( brush, points );
			//pe.Graphics.DrawPolygon( perimeterPen, points );

			// Draw smoothy
			graphics.FillClosedCurve( brush, points, System.Drawing.Drawing2D.FillMode.Alternate, .25f );
			graphics.DrawClosedCurve( perimeterPen, points, .25f, System.Drawing.Drawing2D.FillMode.Alternate );
		}

	}

	PointMapper SetupSingleBoardTransform(RectangleF boardRect) {
		var upperLeft = new PointF( 24f, 75f );
		float usableHeight = (this.Height - upperLeft.Y * 2);

		// calculate scaling Assuming height-limited
		float islandHeight = boardRect.Height; // (float)(0.5 * Math.Sqrt( 3 )); // each board size is 1. Equalateral triangle height is sqrt(3)/2
		float scale = usableHeight / islandHeight;

		return new PointMapper ( 
			  RowVector.Translate( -boardRect.X, -boardRect.Y ) // translate to origin
			* RowVector.Scale( scale, -scale ) // flip-y and scale
			* RowVector.Translate( upperLeft.X, upperLeft.Y + usableHeight ) // translate to view port
		);
	}

	PointMapper SetupIsland1of2() { // left side, ocean at bottom
		return new PointMapper( Move2BoardsToScreen() );
	}

	PointMapper SetupIsland2of2() { // left side, ocean at bottom
		var alignToIsland1 = RowVector.RotateDegrees( 180 ) * RowVector.Translate( 1, 0 );
		return new PointMapper( alignToIsland1 * Move2BoardsToScreen() );
	}

	Matrix3D Move2BoardsToScreen() {
		var upperLeft = new PointF( 24f, 75f );
		float usableHeight = (this.Height - upperLeft.Y * 2)*.8f;
		float islandHeight = (float)(0.5 * Math.Sqrt( 3 )); // each board size is 1. Equalateral triangle height is sqrt(3)/2
		float scale = usableHeight / islandHeight;
		return RowVector.RotateDegrees( 120 )
			* RowVector.Scale( scale, -scale ) // flip-y 
			* RowVector.Translate( upperLeft.X + (float)(scale * 1.5), upperLeft.Y + usableHeight );
	}

	void DrawElementsPopUp(Graphics graphics ) {
		if(decision_Element is null) return;

		int boundsHeight = Height / 8;
		int margin = boundsHeight / 16;
		var elementOptions = decision_Element.Options.OfType<ItemOption<Element>>().ToArray();
		int count = elementOptions.Length;
		int boundsWidth = boundsHeight * count + margin * (count-1);
		Rectangle bounds = new Rectangle( 0 + (Width-boundsWidth)/2, Height - boundsHeight-20, boundsWidth, boundsHeight );
		var elementLayout = new ElementLayout(bounds.InflateBy(-boundsHeight/8));

		graphics.FillRectangle(Brushes.DarkGray,bounds);

		int i=0;
		foreach(var opt in elementOptions) {
			using var img = ResourceImages.Singleton.GetImage( opt.Item.GetTokenImg() );
			var rect = elementLayout.Rect(i++);
			graphics.DrawImage(img,rect);
			hotSpots.Add(opt,rect);
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

		graphics.FillRectangle(Brushes.DarkGray,bounds);
		using var minorImage = Image.FromFile( ".\\images\\minor.png" );
		using var majorImage = Image.FromFile( ".\\images\\major.png" );
		graphics.DrawImage(minorImage,minorRect);
		graphics.DrawImage(majorImage,majorRect);
	}

	static Rectangle FitClientBounds(Rectangle bounds) {
		const float windowRatio = 1.05f;
		if(bounds.Width > bounds.Height / windowRatio) {
			int widthClip = bounds.Width - (int)(bounds.Height / windowRatio);
			bounds.X += widthClip;
			bounds.Width -= widthClip;
		} else
			bounds.Height = (int)(bounds.Width * windowRatio);
		return bounds;
	}

	#region Layout

	SpiritLayout spiritLayout;
	Rectangle activeFearRect;

	#endregion

	SpiritPainter spiritPainter;
	void DrawSpirit( Graphics graphics, Rectangle bounds ) {

		bounds = FitClientBounds( bounds );

		// Layout
		if( spiritLayout == null || growthOptionCount != spirit.GrowthTrack.Options.Length) {
			CalcSpiritLayout( graphics, bounds );
			if(spiritPainter != null) spiritPainter.Dispose(); // release old
			spiritPainter = new SpiritPainter( spirit, spiritLayout, presenceColor );
		}

		graphics.FillRectangle( Brushes.LightYellow, bounds );
		spiritPainter.Paint( graphics,
			options_InnatePower,
			options_DrawableInate,
			options_GrowthOptions,
			options_GrowthActions,
			options_Track
		);
	}

	void CalcSpiritLayout( Graphics graphics, Rectangle bounds ) {
		spiritLayout = new SpiritLayout( graphics, spirit, bounds, 10 );
		growthOptionCount = spirit.GrowthTrack.Options.Length;
	}
	int growthOptionCount = -1; // auto-update when Starlight adds option

	void RecordSpiritHotspots() {
		// Growth Options/Actions
		foreach(var opt in options_GrowthOptions)
			if(spiritLayout.growthLayout.HasOption(opt))
				hotSpots.Add( opt, spiritLayout.growthLayout[opt] );
		foreach(var act in options_GrowthActions)
			if(spiritLayout.growthLayout.HasAction( act )) // there might be delayed setup actions here that don't have a rect
				hotSpots.Add( act, spiritLayout.growthLayout[act] );
		// Presence
		foreach(var track in options_Track)
			hotSpots.Add( track, spiritLayout.trackLayout.ClickRectFor( track ) );
		// Innates - Select Innate Power
		foreach(var power in options_InnatePower)
			hotSpots.Add( power, spiritLayout.innateLayouts[power].Bounds );
		// Innates - Select Innate Option (for shifting memory)
		var grpOptionLayouts = spiritLayout.innateLayouts.Values.SelectMany(x=>x.Options).ToArray();
		foreach(var grp in options_DrawableInate) {
			var bounds = grpOptionLayouts.First(x=>x.GroupOption == grp).Bounds;
			hotSpots.Add( grp, bounds );
		}

	}

	void DrawGameRound( Graphics graphics ) {
		float fontHeight = Height*.05f;
		using var font = new Font( ResourceImages.Singleton.Fonts.Families[0], fontHeight, GraphicsUnit.Pixel );

		// Default - black, Fight
		Brush brush = Brushes.Black; 
		string snippet = "Fight!";

		// If game is over, update
		if( gameState.Result != null) { 
			brush = gameState.Result.Result == GameOverResult.Victory ? Brushes.DarkGreen : Brushes.DarkRed;
			snippet = gameState.Result.Msg();
		}
		graphics.DrawString($"Round {gameState.RoundNumber} - {snippet}", font, brush, 0, 0);
		
	}

	void DrawInvaderCards( Graphics graphics ) {

		const float margin = 8;
		const float textHeight = 20f;

		var bounds = CalcInvaderCardRect;

		// Calculate Card Size based on # of slots
		float slots = gameState.InvaderDeck.Slots.Count + 1.5f;
		float slotWidth = bounds.Width / slots;

		var cardSize = slotWidth *1.5f < bounds.Height         // too narrow
			? new SizeF( slotWidth, slotWidth * 1.5f )         // use narrow width to limit height
			: new SizeF( bounds.Height / 1.5f, bounds.Height); // plenty of width, use height to determine size

		// calc discard location
		var discardRect = new RectangleF( bounds.Left, bounds.Top + margin + (cardSize.Height - cardSize.Width) * .5f, cardSize.Height-margin*2, cardSize.Width-margin*2 );
		// using(var pen = new Pen( Color.Orange, 5f )) graphics.DrawRectangle( pen, discardRect.ToInts() ); // Debug
		//Point[] discardDestinationPoints = {
		//		new Point((int)discardRect.Right, (int)discardRect.Top),    // destination for upper-left point of original
		//		new Point((int)discardRect.Right, (int)discardRect.Bottom), // destination for upper-right point of original
		//		new Point((int)discardRect.Left, (int)discardRect.Top)      // destination for lower-left point of original
		//	};
		Point[] discardDestinationPoints = {
				new Point((int)discardRect.Left, (int)discardRect.Bottom),    // destination for upper-left point of original
				new Point((int)discardRect.Left, (int)discardRect.Top), // destination for upper-right point of original
				new Point((int)discardRect.Right, (int)discardRect.Bottom)      // destination for lower-left point of original
			};


		// using(var pen = new Pen( Color.Orange, 15f)) graphics.DrawRectangle( pen, bounds.ToInts() ); // Debug

		// locate each of the cards
		var cardMetrics = new InvaderCardMetrics[gameState.InvaderDeck.Slots.Count];

		for(int i=0; i < cardMetrics.Length; ++i)
			cardMetrics[i] = new InvaderCardMetrics( gameState.InvaderDeck.Slots[i],  
				bounds.Left + (i+1.5f) * cardSize.Width + margin, //left+i*xStep, 
				bounds.Top + margin, // y, 
				cardSize.Width-margin*2, cardSize.Height - margin * 2, // width, height, 
				textHeight
			);

		// Draw
		using var buildRavageFont = new Font( ResourceImages.Singleton.Fonts.Families[0], textHeight, GraphicsUnit.Pixel );
		using var invaderStageFont = new Font( ResourceImages.Singleton.Fonts.Families[0], textHeight*2, GraphicsUnit.Pixel );
		foreach(var cardMetric in cardMetrics)
			cardMetric.Draw( graphics, buildRavageFont, invaderStageFont );

		// Draw Discard
		var lastDiscard = gameState.InvaderDeck.Discards.FirstOrDefault();
		if(lastDiscard is not null) {
			using Bitmap discardImg = ResourceImages.Singleton.GetInvaderCard( lastDiscard.Text );
			graphics.DrawImage( discardImg, discardDestinationPoints );
		}

	}



	void DrawFearPopUp( Graphics graphics ) {
		if(options_FearCard is null) return;

		activeFearRect = CalcPopupFearRect();

		using var img = new FearCardImageManager().GetImage( options_FearCard );
		graphics.DrawImage( img, activeFearRect );
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
			textBounds = new RectangleF( x, y + height + textHeight*.1f, width, textHeight*1.5f );
		}
		readonly InvaderSlot slot;
		readonly RectangleF[] Rect;
		readonly RectangleF textBounds;

		public void Draw( Graphics graphics, Font labelFont, Font invaderStageFont ) {
			// Draw all of the cards in that slot
			// !! we could make them overlap and bigger
			for(int i = 0; i < Rect.Length; ++i) {
				var card = slot.Cards[i];
				if(card.Flipped)
					graphics.DrawInvaderCardFront( Rect[i], card );
				else {
					var cardRect = Rect[i];
					using(SolidBrush brush = new SolidBrush(Color.LightSteelBlue))
						graphics.FillRectangle(brush,cardRect);
					var smallerRect = cardRect.InflateBy(-cardRect.Width*.1f);
					graphics.DrawInvaderCardBack( smallerRect, card );
					smallerRect = cardRect.InflateBy( -25f );
					graphics.DrawString( card.InvaderStage.ToString(), invaderStageFont, Brushes.DarkRed, smallerRect, alignCenter );
				}
			}
			graphics.DrawString( slot.Label, labelFont, Brushes.Black, textBounds, alignCenter );
		}
		readonly StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Center };
	}

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
				DrawSpaceTokenHotspot( graphics, pen, space, new SpaceToken( space, spirit.Presence.Token ) );
		}

	}

	void DrawHotSpots_Space( Graphics graphics, Pen pen ) {
		if(options_Space != null)
			foreach(Space space in options_Space) {
				PointF center = SpaceCenter( space );
				graphics.DrawEllipse( pen, center.X - radius, center.Y - radius, radius * 2, radius * 2 );
			}
	}

	void DrawSpaceTokenHotspot( 
		Graphics graphics, 
		Pen pen,
		IOption option,  // the actual option to record
		SpaceToken st	// the effective SpaceToken (location) of the option
	) {
		if(HasLocation( st )) {
			var rect = GetLocation( st );
			rect.Inflate( 4, 4 );
			optionRects.Add( (rect, option) );
			graphics.DrawRectangle( pen, rect );
		}
	}

	void DrawAdjacentArrows( Graphics graphics ) {
		var center = SpaceCenter( decision_AdjacentInfo.Original );
		var others = decision_AdjacentInfo.Adjacent.Select( x => SpaceCenter( x ) ).ToArray();
		using Pen p = new Pen( Color.DeepSkyBlue, 7 );
		var drawer = new ArrowDrawer( graphics, p );
		switch(decision_AdjacentInfo.Direction) {
			case SpiritIsland.Select.AdjacentDirection.Incoming:
				foreach(var other in others)
					drawer.Draw( other, center );
				break;
			case SpiritIsland.Select.AdjacentDirection.Outgoing:
				foreach(var other in others)
					drawer.Draw( center, other );
				break;
		}
	}

	void DecorateSpace( Graphics graphics, SpaceState spaceState ) {
		MultiSpace ms = spaceState.Space as MultiSpace;
		Space spaceToShowTokensOn = ms!=null ? ms.Parts[0] : spaceState.Space;
		if(!spaceLookup.ContainsKey( spaceToShowTokensOn )) return; // happens during developement

		PointF xy = _mapper.Map( spaceLookup[spaceToShowTokensOn].Center );

		// !!! scale tokens based on board/space size, NOT widow size (for 2 boards, tokens are too big)
		float iconWidth = boardScreenSize.Width * .045f; 
		float xStep = iconWidth + 10f;

		float x = xy.X - iconWidth;
		float y = xy.Y - iconWidth;

		if(ms != null)
			DrawMultiSpace(graphics,ms);

		// Row 1 - Invaders
		DrawInvaderRow( graphics, x, ref y, iconWidth, xStep, spaceState );

		// Row 2 - Dahan, Blight, Elements, Presence
		List<Token> row2Tokens = new List<Token> { TokenType.Defend, TokenType.Blight }; // These don't show up in .OfAnyType if they are dynamic
		row2Tokens.AddRange( spaceState.OfAnyType( TokenType.Dahan ) );
		row2Tokens.AddRange( spaceState.OfAnyType( spirit.Presence.Token ) );
		row2Tokens.AddRange( spaceState.OfAnyType( TokenType.Element ) );
		DrawRow( graphics, spaceState, x, ref y, iconWidth, xStep, row2Tokens.ToArray() );

		// Row 3 - BAC Tokens 
		List<Token> row3Tokens = new List<Token> { TokenType.Beast,TokenType.Wilds,TokenType.Disease,TokenType.Badlands,TokenType.Isolate };
		row3Tokens.AddRange( spaceState.OfType( TokenType.OpenTheWays ) );
		DrawRow( graphics, spaceState, x, ref y, iconWidth, xStep, row3Tokens.ToArray() );
	}

	void DrawMultiSpace( Graphics graphics, MultiSpace multi ) {
		var merged = spaceLookup[multi.Parts[0]].corners;
		for(int i=1;i<multi.Parts.Length;++i)
			merged = BoardLayout.JoinAdjacentPolgons( merged, spaceLookup[multi.Parts[i]].corners );

//		using var pen = new Pen( Brushes.Yellow, 5f );
		using var pen = new Pen( Brushes.Gold, 3f );


		var colors = multi.Parts
			.Select( x=> Color.FromArgb(92, SpaceColor(x) ))
			.ToArray();

		using var brush = new LinearGradientBrush( new Rectangle(0, 0, 30, 30 ), Color.Transparent, Color.Transparent, 45F );
		var blend = new ColorBlend();
		blend.Positions = new float[colors.Length*2];
		blend.Colors = new Color[colors.Length*2];
		float step = 1.0f/colors.Length;
		for(int i = 0; i < colors.Length; ++i) {
			blend.Positions[i * 2] = i*step;
			blend.Positions[i * 2+1] = (i+1) * step;
			blend.Colors[i*2] = blend.Colors[i*2+1] = colors[i];
		}

		brush.InterpolationColors = blend;

		var points = merged.Select( _mapper.Map ).ToArray();

		graphics.FillClosedCurve( brush, points, FillMode.Alternate, .25f );
		graphics.DrawClosedCurve( pen, points, .25f, FillMode.Alternate );

//		graphics.FillPolygon( brush, points );
//		graphics.DrawPolygon( pen, points );


	}

	void DrawInvaderRow( Graphics graphics, float x, ref float y, float width, float step, SpaceState tokens ) {

		Space space = tokens.Space;

		// tokens
		var invaders = tokens.Keys
			.Where(k=>{ var c=k.Class.Category; return c==TokenCategory.Invader || c==TokenCategory.DreamingInvader; } )
			.Cast<HealthToken>()
			.ToArray();
		if(invaders.Length==0) return;

		float maxHeight = 0;

		var orderedInvaders = invaders
			// Major ordering: (Type > Strife)
			.OrderByDescending( i => i.FullHealth )
			.ThenBy(x=>x.StrifeCount)
			// Minor ordering: (remaining health)
			.ThenBy(i=>i.RemainingHealth); // show damaged first so when we apply damage, the damaged one replaces the old undamaged one.

		foreach(Token token in orderedInvaders) {

			// Strife
			Token imageToken;
			if(token is HealthToken si && 0<si.StrifeCount) {
				imageToken = si.HavingStrife( 0 );

				Rectangle strifeRect = FitWidth( x, y, width, strife.Size );
				graphics.DrawImage( strife, strifeRect );
				if(si.StrifeCount > 1)
					graphics.DrawSuperscript( strifeRect, "x" + si.StrifeCount );
			} else {
				imageToken = token;
			}

			// Draw Token
			Image img = AccessTokenImage( imageToken );
			Rectangle rect = FitWidth( x, y, width, img.Size );
			graphics.DrawImage( img, rect );

			// record token location
			RecordSpaceTokenLocation( new SpaceToken(space, token), rect );

			// Count
			graphics.DrawCountIfHigherThan( rect, tokens[token] );

			maxHeight = Math.Max( maxHeight, rect.Height );
			x += step;
		}

		float gap = step - width;
		y += maxHeight + gap;
	}

	private Image AccessTokenImage( Token imageToken ) {
		if(!tokenImages.ContainsKey( imageToken ))
			tokenImages[imageToken] = GetImage( imageToken );
		return tokenImages[imageToken];
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
		if(ht.Class.Category == TokenCategory.DreamingInvader) {
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
			using var font = new Font( ResourceImages.Singleton.Fonts.Families[0], orig.Height/2, GraphicsUnit.Pixel );
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

	void DrawFearPool( Graphics graphics ) {
		var bounds = CalcFearPoolRect;
		float margin = Math.Max(5f, bounds.Height*.05f);
		float slotWidth = bounds.Height; 

		int fearCount = this.gameState.Fear.EarnedFear;
		int maxSpaces = this.gameState.Fear.PoolMax;

		float step = (bounds.Width-2*margin-3*slotWidth)/(maxSpaces-1); 
		// -1 slot width for #/# and 
		// -1 slot width for last fear token
		float tokenWidth = slotWidth-2*margin;
		float tokenHeight = fear.Height * tokenWidth / fear.Width;
		RectangleF CalcBounds(int i) => new RectangleF( bounds.X+slotWidth+margin+step*i, bounds.Y+margin, tokenWidth, tokenHeight );

		// Terror Level
		using var terror = ResourceImages.Singleton.TerrorLevel( gameState.Fear.TerrorLevel );
		graphics.DrawImage(terror,new RectangleF(bounds.X+margin,bounds.Y+margin,tokenWidth,tokenHeight));

		// draw gray underneath
		for(int i = fearCount; i < maxSpaces; ++i)
			graphics.DrawImage(grayFear,CalcBounds(i));
		// draw fear tokens
		for(int i = 0; i<fearCount; ++i)
			graphics.DrawImage(fear,CalcBounds(i));

		// Activated Cards
		int activated = gameState.Fear.ActivatedCards.Count;
		if(activated > 0) {
			using var card = ResourceImages.Singleton.FearCard();
			var rect = new RectangleF(bounds.Right-margin-slotWidth,bounds.Y+margin,tokenWidth,tokenHeight);
			graphics.DrawImageFitHeight(card,rect);
			rect.X -= rect.Width * .25f; // shift x2 left onto the card
			graphics.DrawCountIfHigherThan( rect.ToInts(), activated );
		}

	}

	void DrawBlight( Graphics graphics ) {
		var bounds = CalcBlightRect;

		float margin = Math.Max(5f, bounds.Height*.05f);
		float slotWidth = bounds.Height; 

		int count = this.gameState.blightOnCard;
		int maxSpaces = 6;

		float step = (bounds.Width-2*margin-2*slotWidth)/(maxSpaces-1); 
		// -1 slot width for #/# and 
		// -1 slot width for last fear token
		float tokenWidth = slotWidth-2*margin;
		float tokenHeight = fear.Height * tokenWidth / fear.Width;
		RectangleF CalcBounds(int i) => new RectangleF( bounds.X+slotWidth+margin+step*i, bounds.Y+margin, tokenWidth, tokenHeight );

		// draw fear tokens
		var img = this.tokenImages[TokenType.Blight];
		for(int i = 0; i<count; ++i)
			graphics.DrawImage(img,CalcBounds(i));

		if(gameState.BlightCard.CardFlipped)
			graphics.DrawString("Blighted!",SystemFonts.DialogFont, Brushes.Red, bounds.Right-slotWidth*1.5f,bounds.Top);
	}


	void DrawRow( Graphics graphics, SpaceState tokens, float x, ref float y, float width, float step, params Token[] tokenTypes ) {
		float maxHeight = 0;

		using Font countFont = new( "Arial", 7, FontStyle.Bold, GraphicsUnit.Point );

		foreach(var token in tokenTypes) {
			int count = tokens[token];
			if(count == 0) continue;

			bool isPresence = token is SpiritPresenceToken;
			Image img = isPresence ? presence : AccessTokenImage( token );

			// calc rect
			float height = width / img.Width * img.Height;
			maxHeight = Math.Max( maxHeight, height );
			var rect = new Rectangle( (int)x, (int)y, (int)width, (int)height );
			x += step;

			// record token location
			RecordSpaceTokenLocation( new SpaceToken( tokens.Space, token), rect );

			if(isPresence && spirit.Presence.IsSacredSite(tokens) ) {
				const int inflationSize = 10;
				rect.Inflate( inflationSize, inflationSize );
				Color newColor = Color.FromArgb( 100, Color.Yellow );
				using var brush = new SolidBrush( newColor );
				graphics.FillEllipse( brush, rect );
				rect.Inflate( -inflationSize, -inflationSize );
			}

			// Draw Tokens
			graphics.DrawImage( img, rect );
			graphics.DrawCountIfHigherThan( rect, count );
		}

		float gap = step-width;
		y += maxHeight + gap;
	}

	static Rectangle FitWidth( float x, float y, float width, Size sz ) {
		return new Rectangle( (int)x, (int)y, (int)width, (int)(width / sz.Width * sz.Height) );
	}

	/// <returns>the center of a Game Space</returns>
	PointF SpaceCenter( Space s ) {
		var norm = spaceLookup.ContainsKey(s)
				? spaceLookup[s].Center
			: s is MultiSpace ms
				? spaceLookup[ ms.Parts[0] ].Center
			: throw new ArgumentException($"Space {s.Label} does not have a screen location");
		return _mapper.Map(norm); //  new PointF( norm.X * boardScreenSize.Width, norm.Y * boardScreenSize.Height );
	}

	protected override void OnSizeChanged( EventArgs e ) {
		base.OnSizeChanged( e );
		RefreshLayout();
	}

	public void RefreshLayout() {
		spiritLayout = null;
		ClearCachedImage();
		this.Invalidate();
	}

	void ClearCachedImage() {
		if(cachedBackground != null) {
			cachedBackground.Dispose();
			cachedBackground = null;
		}
	}

	#endregion

	protected override void OnClick( EventArgs e ) {

		IOption option = FindOption();
		if(option is Space space)
			SpaceClicked?.Invoke(space);
		else if(option is Token invader)
			TokenClicked?.Invoke( invader );
		else if(option is SpaceToken st)
			SpaceTokenClicked?.Invoke( st );
		else if( option != null )
			OptionSelected?.Invoke(option);

		Point clientCoords = this.PointToClient( Control.MousePosition );
		if(spiritLayout != null && spiritLayout.imgBounds.Contains( clientCoords )) {
			string msg = this.spirit.SpecialRules.Select(r=>r.ToString()).Join("\r\n\r\n");
			MessageBox.Show( msg );
		}

	}

	protected override void OnMouseMove( MouseEventArgs e ) {
		base.OnMouseMove( e );

		if(options_Space==null) return;

		bool inCircle = FindOption() != null;
		Cursor = inCircle ? Cursors.Hand : Cursors.Default;

	}

	IOption FindOption() {
		Point clientCoords = this.PointToClient( Control.MousePosition );
		return FindInvader( clientCoords )
			?? FindSpaces( clientCoords )
			?? FindHotSpot( clientCoords );
	}

	IOption FindSpaces( Point clientCoords ) {
		return options_Space?.Select( s => {
				PointF center = SpaceCenter( s );
				float dx = clientCoords.X - center.X;
				float dy = clientCoords.Y - center.Y;
				return new { Space = s, d2 = dx * dx + dy * dy };
			} )
			.Where( x => x.d2 < radius * radius )
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

	IOption FindHotSpot( Point clientCoords ) {
		return hotSpots.Keys.FirstOrDefault(key=>hotSpots[key].Contains(clientCoords));
	}

	readonly Dictionary<IOption, RectangleF> hotSpots = new();

	#region User Action Events - Notify main form

	public event Action<IOption> OptionSelected;
	public event Action<Space> SpaceClicked;
	public event Action<Token> TokenClicked;
	public event Action<SpaceToken> SpaceTokenClicked;

	#endregion

	#region private fields

	void OptionProvider_OptionsChanged( IDecision decision ) {

		// These decision_ variables contain additional info the options need to render them on the screen
		decision_TokenOnSpace        = decision as Select.TokenFrom1Space;				// Identifies option as SpaceToken
		decision_SpaceToken          = decision as Select.TypedDecision<SpaceToken>;	// Identifies option as SpaceToken
		decision_DeployedPresence    = decision as Select.DeployedPresence;				// Identifies option as SpaceToken
		decision_DeckToDrawFrom      = decision as Select.DeckToDrawFrom;
		decision_Element             = decision as Select.Element;
		decision_AdjacentInfo        = decision is Select.IHaveAdjacentInfo adjacenInfoProvider ? adjacenInfoProvider.AdjacentInfo : null;

		// These option_ variables contain everything they need to render on screen
		options_Space         = decision.Options.OfType<Space>().ToArray();
		options_FearCard      = decision.Options.OfType<ActivatedFearCard>().FirstOrDefault();
		options_Track         = decision.Options.OfType<Track>().ToArray();
		options_InnatePower   = decision.Options.OfType<InnatePower>().ToArray();
		options_DrawableInate = decision.Options.OfType<IDrawableInnateOption>().ToArray();
		options_GrowthOptions = decision.Options.OfType<GrowthOption>().ToArray();
		options_GrowthActions = decision.Options.OfType<GrowthActionFactory>().ToArray();
	}

	Select.TokenFrom1Space           decision_TokenOnSpace;
	Select.AdjacentInfo              decision_AdjacentInfo;
	Select.TypedDecision<SpaceToken> decision_SpaceToken;
	Select.DeployedPresence          decision_DeployedPresence;
	Select.DeckToDrawFrom            decision_DeckToDrawFrom;
	Select.Element                   decision_Element;

	ActivatedFearCard       options_FearCard;
	Track[]                 options_Track;
	InnatePower[]           options_InnatePower;
	IDrawableInnateOption[] options_DrawableInate;
	GrowthOption[]          options_GrowthOptions;
	GrowthActionFactory[]   options_GrowthActions;

		
	readonly List<(Rectangle,IOption)> optionRects = new List<(Rectangle, IOption)>();


	// Stores the locations of ALL SpaceTokens (invaders, dahan, presence, wilds, disease, beast, etx)
	// When we are presented with a decision, the location of each option is pulled from here
	// and added to the HotSpots.
	readonly Dictionary<SpaceToken, Rectangle> tokenLocations = new Dictionary<SpaceToken, Rectangle>();
	void RecordSpaceTokenLocation( SpaceToken sp, Rectangle rect ) => tokenLocations.Add( MakeKey(sp), rect );
	bool HasLocation( SpaceToken sp ) => tokenLocations.ContainsKey( MakeKey(sp) );

	Rectangle GetLocation( SpaceToken sp ) => tokenLocations[ MakeKey(sp) ];
	static SpaceToken MakeKey( SpaceToken sp ) => sp;


	GameState gameState;
	Spirit spirit;

	const float radius = 40f;
	Space[] options_Space;
	Dictionary<Space,SpaceLayout> spaceLookup;

	Size boardScreenSize;
	Bitmap cachedBackground;

	Image presence;
	string presenceColor;
	Image strife;
	Image fear;
	Image grayFear;
	Dictionary<Token, Image> tokenImages; // not token class, because we need different images for different damaged invaders.
	#endregion

}

class ArrowDrawer {
	readonly Graphics graphics; 
	readonly Pen pen;
	const float startNorm = 0.2f;
	const float endNorm = 0.8f;
	const float arrowNorm = .1f;
	public ArrowDrawer(Graphics graphics, Pen pen){
		this.graphics = graphics;
		this.pen = pen;
	}
	public void Draw(PointF from, PointF to ) {
		float dx = to.X-from.X;
		float dy = to.Y-from.Y;
		PointF newFrom = new PointF( from.X+dx*startNorm, from.Y+dy*startNorm );
		PointF newTo = new PointF( from.X + dx * endNorm, from.Y + dy * endNorm );

		float inlineX = dx * arrowNorm, inlineY = dy * arrowNorm;
		float perpX = -inlineY, perpY = inlineX;

		PointF wing1 = new PointF( newTo.X +perpX-inlineX, newTo.Y + perpY-inlineY );
		PointF wing2 = new PointF( newTo.X - perpX - inlineX, newTo.Y - perpY - inlineY );

		graphics.DrawLine( pen,newFrom,newTo );
		graphics.DrawLine( pen,newTo,wing1);
		graphics.DrawLine( pen, newTo, wing2 );
	}
}

