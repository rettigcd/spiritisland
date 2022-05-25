using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;


public partial class IslandControl : Control {

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
		switch(board[0].Label[..1]) {
			case "A":
				boardImageFile = ".\\images\\board a.png";
				spaceLookup = new Dictionary<string, PointF> {
					["A0"] = new PointF( 0.12f, 0.53f ),
					["A1"] = new PointF( 0.42f, 0.30f ),
					["A2"] = new PointF( 0.25f, 0.52f ),
					["A3"] = new PointF( 0.17f, 0.82f ),
					["A4"] = new PointF( 0.42f, 0.71f ),
					["A5"] = new PointF( 0.61f, 0.54f ),
					["A6"] = new PointF( 0.59f, 0.27f ),
					["A7"] = new PointF( 0.75f, 0.61f ),
					["A8"] = new PointF( 0.81f, 0.19f ),
				};
				break;
			case "B":
				boardImageFile = ".\\images\\board b.png";
				spaceLookup = new Dictionary<string, PointF> {
					["B0"] = new PointF( 0.19f, 0.39f ),
					["B1"] = new PointF( 0.40f, 0.25f ),
					["B2"] = new PointF( 0.28f, 0.57f ),
					["B3"] = new PointF( 0.20f, 0.86f ),
					["B4"] = new PointF( 0.48f, 0.68f ),
					["B5"] = new PointF( 0.58f, 0.47f ),
					["B6"] = new PointF( 0.60f, 0.23f ),
					["B7"] = new PointF( 0.73f, 0.61f ),
					["B8"] = new PointF( 0.81f, 0.18f ),
				};
				break;
			case "C":
				boardImageFile = ".\\images\\board c.png";
				spaceLookup = new Dictionary<string, PointF> {
					["C0"] = new PointF( 0.106f, 0.610f ),
					["C1"] = new PointF( 0.352f, 0.287f ),
					["C2"] = new PointF( 0.265f, 0.609f ),
					["C3"] = new PointF( 0.184f, 0.875f ),
					["C4"] = new PointF( 0.449f, 0.818f ),
					["C5"] = new PointF( 0.504f, 0.613f ),
					["C6"] = new PointF( 0.568f, 0.255f ),
					["C7"] = new PointF( 0.696f, 0.557f ),
					["C8"] = new PointF( 0.789f, 0.193f ),
				};
				break;
			case "D":
				boardImageFile = ".\\images\\board d.png";
				spaceLookup = new Dictionary<string, PointF> {
					["D0"] = new PointF( 0.099f, 0.559f ),
					["D1"] = new PointF( 0.363f, 0.178f ),
					["D2"] = new PointF( 0.274f, 0.507f ),
					["D3"] = new PointF( 0.150f, 0.800f ),
					["D4"] = new PointF( 0.382f, 0.776f ),
					["D5"] = new PointF( 0.493f, 0.446f ),
					["D6"] = new PointF( 0.592f, 0.713f ),
					["D7"] = new PointF( 0.694f, 0.446f ),
					["D8"] = new PointF( 0.797f, 0.191f ),
				};
				break;
		}

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

	#region Paint

	protected override void OnPaint( PaintEventArgs pe ) {
		base.OnPaint( pe );

		optionRects.Clear();
		tokenLocations.Clear();
		hotSpots.Clear(); // Clear this at beginning so any of the DrawX methods can add to it

		if(gameState == null) return;

		StopWatch.timeLog.Clear();

		using(new StopWatch( "Total" )) {

			DrawBoard_Static( pe );
			using(new StopWatch( "Island-Tokens" ))
				foreach(Space space in gameState.Island.Boards[0].Spaces)
					DecorateSpace(pe.Graphics,space);

			using(new StopWatch( "fear" ))
				DrawFearPool( pe.Graphics, new RectangleF(Width*.50f, 0f, Width*.20f, Width*.04f ) );

			using(new StopWatch( "blight" ))
				DrawBlight  ( pe.Graphics, new RectangleF(Width*.55f,Width*.05f,Width*.15f,Width*.03f ) );

			using(new StopWatch( "round" ))
				DrawRound( pe.Graphics );

			using(new StopWatch( "invader cards" ))
				DrawInvaderCards( pe.Graphics, new Rectangle(0,0,(int)(Width*.65f),Height) ); // other than highlights, do this last since it contains the Fear Card that we want to be on top of everything.

			using(new StopWatch( "misc" )) {
				DrawDeck(pe.Graphics);
				DrawElements( pe.Graphics );
			}

			using(new StopWatch( "HotSpots" ))
				DrawHotspots( pe );

			const float spiritShare = .35f;
			DrawSpirit( pe.Graphics, new Rectangle( Width - (int)(spiritShare*Width), 0, (int)(spiritShare*Width), Height) );
		}

		// non drawing - record Hot spots
		RecordSpiritHotspots();
		if(fearCard!= null) 
			hotSpots.Add(fearCard,activeFearRect);

	}

	BoardLayout bl;

	void DrawBoard_Static( PaintEventArgs pe ) {
		using var stopwatch = new StopWatch( "Island-static" );

		if(cachedBackground == null) {

			bl = gameState.Island.Boards[0][0].Text[0] switch {
				'A' => BoardLayout.BoardA(),
				'B' => BoardLayout.BoardB(),
				'C' => BoardLayout.BoardC(),
				'D' => BoardLayout.BoardD(),
				_ => throw new Exception( "unknown board" )
			};

			using var board = Image.FromFile( boardImageFile );

			// Assume limit is height
			boardScreenSize = (board.Width * Height > Width * board.Height)
				? new Size( Width, board.Height * Width / board.Width )
				: new Size( board.Width * Height / board.Height, Height );

			cachedBackground = new Bitmap( boardScreenSize.Width, boardScreenSize.Height );

			var graphics = Graphics.FromImage(cachedBackground);
			graphics.DrawImage( board, 0, 0, boardScreenSize.Width, boardScreenSize.Height );
		}

		pe.Graphics.DrawImage( cachedBackground, 0, 0, cachedBackground.Width, cachedBackground.Height );

		static PointF map(PointF p) => new PointF( p.X * 720 + 24f, 710 * (1f - p.Y) - 20f );
		using var perimeterPen = new Pen( Brushes.Black, 5f );
		for(int i = 0; i < bl.spaces.Length; ++i) {
			var space = gameState.Island.Boards[0][i];
			Brush brush = space.IsWetland ? Brushes.LightBlue
				: space.IsSand ? Brushes.PaleGoldenrod
				: space.IsMountain ? Brushes.Gray
				: space.IsJungle ? Brushes.ForestGreen
				: Brushes.Blue;
			var points = bl.spaces[i].Select( map ).ToArray();

			// Draw blocky
			//pe.Graphics.FillPolygon( brush, points );
			//pe.Graphics.DrawPolygon( perimeterPen, points );

			// Draw smoothy
			pe.Graphics.FillClosedCurve( brush, points, System.Drawing.Drawing2D.FillMode.Alternate, .25f );
			pe.Graphics.DrawClosedCurve( perimeterPen, points,.25f, System.Drawing.Drawing2D.FillMode.Alternate);
		}

		//var path = new System.Drawing.Drawing2D.GraphicsPath()
		//pe.Graphics.SetClip(path);

		// Draw foreground image into clipping region
		// myGraphic.SetClip( clipPath, CombineMode.Replace );
		// myGraphic.DrawImage( imgF, outRect );
		// myGraphic.ResetClip();

	}

	void DrawElements(Graphics graphics ) {
		if(elementDecision == null) return;

		int boundsHeight = Height / 8;
		int margin = boundsHeight / 16;
		var elementOptions = elementDecision.Options.OfType<ItemOption<Element>>().ToArray();
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

	void DrawDeck(Graphics graphics ) {
		if(deckDecision==null) return;

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
		hotSpots.Add(deckDecision.Options[0],minorRect);
		hotSpots.Add(deckDecision.Options[1],majorRect);

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
		if( spiritLayout == null || growthOptionCount != spirit.Growth.Options.Length) {
			CalcSpiritLayout( graphics, bounds );
			if(spiritPainter != null) spiritPainter.Dispose(); // release old
			spiritPainter = new SpiritPainter( spirit, spiritLayout, presenceColor );
		}

		graphics.FillRectangle( Brushes.LightYellow, bounds );
		spiritPainter.Paint( graphics,
			selectableInnateOptions,
			selectableInnateGroupOptions,
			selectableGrowthOptions,
			selectableGrowthActions,
			clickableTrackOptions
		);
	}

	void CalcSpiritLayout( Graphics graphics, Rectangle bounds ) {
		spiritLayout = new SpiritLayout( graphics, spirit, bounds, 10 );
		growthOptionCount = spirit.Growth.Options.Length;
	}
	int growthOptionCount = -1; // auto-update when Starlight adds option

	void RecordSpiritHotspots() {
		// Growth Options/Actions
		foreach(var opt in selectableGrowthOptions)
			if(spiritLayout.growthLayout.HasOption(opt))
				hotSpots.Add( opt, spiritLayout.growthLayout[opt] );
		foreach(var act in selectableGrowthActions)
			if(spiritLayout.growthLayout.HasAction( act )) // there might be delayed setup actions here that don't have a rect
				hotSpots.Add( act, spiritLayout.growthLayout[act] );
		// Presence
		foreach(var track in clickableTrackOptions)
			hotSpots.Add( track, spiritLayout.trackLayout.ClickRectFor( track ) );
		// Innates - Select Innate Power
		foreach(var power in selectableInnateOptions)
			hotSpots.Add( power, spiritLayout.innateLayouts[power].Bounds );
		// Innates - Select Innate Option (for shifting memory)
		var grpOptionLayouts = spiritLayout.innateLayouts.Values.SelectMany(x=>x.Options).ToArray();
		foreach(var grp in selectableInnateGroupOptions) {
			var bounds = grpOptionLayouts.First(x=>x.GroupOption == grp).Bounds;
			hotSpots.Add( grp, bounds );
		}

	}

	void DrawRound( Graphics graphics ) {
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

	void DrawInvaderCards( Graphics graphics, Rectangle bounds ) {

		// Active Fear Layout
		int fearHeight = (int)(bounds.Height * .8f);
		var fearWidth = fearHeight * 2/3;
		activeFearRect = new Rectangle( bounds.Width - fearWidth - (int)(bounds.Height * .1f), (bounds.Height - fearHeight) / 2, fearWidth, fearHeight );

		// Invaders
		const float margin = 15;
		const float textHeight = 20f;
		float height = bounds.Height *.33f;
		float width = height * .66f;
		using var buildRavageFont = new Font( ResourceImages.Singleton.Fonts.Families[0], textHeight, GraphicsUnit.Pixel );

		float x = bounds.Width-width-margin-margin;
		float y = bounds.Height-height-margin*2 - textHeight;

		// Build Metrics
		int buildCount = gameState.InvaderDeck.Build.Count;
		RectangleF[] buildRect = new RectangleF[ buildCount ];
		float buildWidth = width / buildCount, buildHeight = height / buildCount;
		for(int i=0;i<buildCount;++i)
			buildRect[i] = new RectangleF( x+i*buildWidth, y+i*buildHeight, buildWidth, buildHeight );
		float buildTextWidth = graphics.MeasureString( "Build", buildRavageFont ).Width;
		PointF buildTextTopLeft = new PointF( x + (width - buildTextWidth) / 2, bounds.Bottom - textHeight - margin );

		// Ravage Metrics
		float ravageX = x - (width + margin);
		int ravageCounts = gameState.InvaderDeck.Ravage.Count;
		var ravageRects = new RectangleF[ ravageCounts];
		float ravageWidth = width / ravageCounts, ravageHeight = height / ravageCounts;
		for(int i=0;i<ravageCounts;++i)
			ravageRects[i] = new RectangleF( ravageX+i*ravageWidth, y+i*ravageHeight, ravageWidth, ravageHeight );
		float textWidth = graphics.MeasureString( "Ravage", buildRavageFont ).Width;
		PointF ravageTextTopLeft = new PointF( ravageX + (width - textWidth) / 2, bounds.Bottom - textHeight - margin );

		// Build
		for(int i=0;i<buildRect.Length;++i) 
			graphics.DrawInvaderCard( buildRect[i], gameState.InvaderDeck.Build[i] );
		graphics.DrawString("Build", buildRavageFont,Brushes.Black, buildTextTopLeft );

		// Ravage
		for(int i=0; i<ravageRects.Length;++i)
			graphics.DrawInvaderCard( ravageRects[i], gameState.InvaderDeck.Ravage[i] );
		graphics.DrawString( "Ravage", buildRavageFont, Brushes.Black, ravageTextTopLeft );

		// Fear
		if(fearCard!= null) {
			using var img = new FearCardImageManager().GetImage( fearCard );
			graphics.DrawImage( img, activeFearRect );
		}

	}

	void DrawHotspots( PaintEventArgs pe ) {
		using var pen = new Pen(Brushes.Aquamarine,5);

		// adjacent
		if(adjacentInfo != null) {

			var center = SpaceCenter(adjacentInfo.Original);
			var others = adjacentInfo.Adjacent.Select(x=> SpaceCenter(x) ).ToArray();

			using Pen p = new Pen( Color.DeepSkyBlue, 7 );
			var drawer = new ArrowDrawer(pe.Graphics,p);
			switch(adjacentInfo.Direction) {
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

		if(spaceTokens != null) {
			// Draw tokens on space && set them as hotspots
			foreach(var st in spaceTokens.Options.OfType<SpaceToken>()) {
				string key = st.Space.Label + ":" + st.Token.ToString();
				if(tokenLocations.ContainsKey( key )) {
					var rect = tokenLocations[key];
					rect.Inflate( 4, 4 );
					optionRects.Add( (rect, st) );
					pe.Graphics.DrawRectangle( pen, rect );
				}
			}
		}

		if(tokenOnSpace != null) {
			// Draw tokens on space && set them as hotspots
			foreach(var token in tokenOnSpace.Options.OfType<Token>()) {
				string key = tokenOnSpace.Space.Label + ":" + token.ToString();
				if(tokenLocations.ContainsKey( key )) {
					var rect = tokenLocations[key];
					rect.Inflate(4,4);
					optionRects.Add( (rect, token) );
					pe.Graphics.DrawRectangle( pen, rect );
				}
			}
		}

		if(deployedPresence != null) {
			activeSpaces = null; // disable circle drawing
			// Presence (inherits from Space Cirlcles
			foreach(var space in deployedPresence.Options.OfType<Space>()) {
				string key = space.Label + ":" + "Presence";
				if(tokenLocations.ContainsKey( key )) {
					var rect = tokenLocations[key];
					rect.Inflate(4,4);
					optionRects.Add( (rect, space) );
					pe.Graphics.DrawRectangle( pen, rect );
				}
			}
		}
			
		if(activeSpaces != null)
			// Space Circles
			foreach(var space in activeSpaces) {
				var center = SpaceCenter(space);
				pe.Graphics.DrawEllipse( pen, center.X- radius, center.Y- radius, radius * 2, radius * 2 );
			}

	}
		
	void DecorateSpace( Graphics graphics, Space space ) {
		if(!spaceLookup.ContainsKey(space.Label)) return; // happens during developement

		PointF normalized = spaceLookup[space.Label];
		PointF xy = new PointF(normalized.X * boardScreenSize.Width, normalized.Y * boardScreenSize.Height);

		float iconWidth = boardScreenSize.Width * .045f; 
		float xStep = iconWidth + 10f;

		float x = xy.X - iconWidth;
		float y = xy.Y - iconWidth;

		TokenCountDictionary tokens = gameState.Tokens[space];
		DrawInvaderRow( graphics, x, ref y, iconWidth, xStep, tokens );

		// dahan & presence & blight
		int presenceCount = spirit.Presence.CountOn( space );
		bool isSS = spirit.Presence.SacredSites.Contains( space );
		List<Token> row2Tokens = new List<Token> { TokenType.Defend, TokenType.Blight }; // These don't show up in .OfAnyType if they are dynamic
		row2Tokens.AddRange( tokens.OfAnyType( TokenType.Dahan ) );
		DrawRow( graphics, tokens, x, ref y, iconWidth, xStep, presenceCount, isSS, row2Tokens.ToArray() );
		DrawRow( graphics, tokens, x, ref y, iconWidth, xStep, 0,             false, TokenType.Beast, TokenType.Wilds, TokenType.Disease, TokenType.Badlands, TokenType.Isolate );

	}

	void DrawInvaderRow( Graphics graphics, float x, ref float y, float width, float step, TokenCountDictionary tokens ) {

		Space space = tokens.Space;

		// tokens
		if(!tokens.HasInvaders()) return;

		float maxHeight = 0;

		var orderedInvaders = tokens.Invaders()
			.OrderByDescending(i=>i.FullHealth)
			.ThenBy(i=>i.RemainingHealth); // show damaged first so when we apply damage, the damaged one replaces the old undamaged one.

		foreach(Token token in orderedInvaders) {

			// Strife
			Token imageToken;
			if(token is HealthToken si && si.StrifeCount>0) {
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
			tokenLocations.Add( space.Label + ":" + token.ToString(), rect );

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
		return token is not HealthToken ht ? throw new Exception( "unknown token " + token )
			: token.Class == TokenType.Dahan ? GetDahanImage( ht )
			: GetInvaderImage( ht );
	}

	static Bitmap GetDahanImage( HealthToken ht ) {

//			if( ht.RemainingHealth != 1) {
			Bitmap orig = ResourceImages.Singleton.GetImage( Img.Dahan2 );
			using var g = Graphics.FromImage( orig );

			if(ht.FullHealth != 2) {
				using var font = new Font( ResourceImages.Singleton.Fonts.Families[0], orig.Height/2, GraphicsUnit.Pixel );
				StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
				g.DrawString( ht.FullHealth.ToString(), font, Brushes.White, new RectangleF( 0, 0, orig.Width, orig.Height ), center );
			}

			int dX = orig.Width / ht.FullHealth;
			int lx = dX/2;
			using Pen redSlash = new Pen( Brushes.Red, 20f );
			for(int i = 0; i < ht.Damage; ++i) {
				g.DrawLine( redSlash, lx,orig.Height,lx+dX,0);
				lx += dX;
			}

		return orig;
//			}

//			return ResourceImages.Singleton.GetImage( Img.Dahan1 );
	}

	static Bitmap GetInvaderImage( HealthToken ht ) {
		return ResourceImages.Singleton.GetImage( PickInvaderImg( ht ) );
	}

	static Img PickInvaderImg( HealthToken token ) {

		return token.Class == Invader.Explorer
				? Img.Explorer
			: token.Class == Invader.Town
				? token.RemainingHealth switch {
					1 => Img.Town1,
					_ => Img.Town2,
				}
			: token.Class == Invader.City
				? token.RemainingHealth switch {
					2 => Img.City2,
					1 => Img.City1,
					_ => Img.City3,
				}
			: throw new Exception( "unknown token " + token );
	}


	void DrawFearPool( Graphics graphics, RectangleF bounds ) {
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

	void DrawBlight( Graphics graphics, RectangleF bounds ) {

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


	void DrawRow( Graphics graphics, TokenCountDictionary tokens, float x, ref float y, float width, float step, int presenceCount, bool isSacredSite, params Token[] tokenTypes ) {
		float maxHeight = 0;

		using Font countFont = new( "Arial", 7, FontStyle.Bold, GraphicsUnit.Point );

		foreach(var token in tokenTypes) {
			int count = tokens[token];
			if(count == 0) continue;

			Image img = AccessTokenImage( token );

			// calc rect
			float height = width / img.Width * img.Height;
			maxHeight = Math.Max( maxHeight, height );
			var rect = new Rectangle( (int)x, (int)y, (int)width, (int)height );
			x += step;

			// record token location
			tokenLocations.Add( tokens.Space.Label + ":" + token.ToString(), rect );

			// Draw Tokens
			graphics.DrawImage( img, rect );
			graphics.DrawCountIfHigherThan( rect, count );
		}

		if(presenceCount > 0) { 
			// calc rect
			float height = width / presence.Width * presence.Height;
			maxHeight = Math.Max( maxHeight, height );
			var presenceRect = new Rectangle( (int)x, (int)y, (int)width, (int)height );
			x += step;

			tokenLocations.Add( tokens.Space.Label + ":" + "Presence", presenceRect );

			if( isSacredSite ) {
				const int inflationSize = 10;
				presenceRect.Inflate( inflationSize, inflationSize );
				Color newColor = Color.FromArgb( 100, Color.Yellow );
				using var brush = new SolidBrush(newColor);
				graphics.FillEllipse(brush,presenceRect);
				presenceRect.Inflate( -inflationSize, -inflationSize );
			}

			// Draw Presence
			graphics.DrawImage( presence, presenceRect );
			graphics.DrawCountIfHigherThan( presenceRect, presenceCount );

		}

		float gap = step-width;
		y += maxHeight + gap;
	}

	static Rectangle FitWidth( float x, float y, float width, Size sz ) {
		return new Rectangle( (int)x, (int)y, (int)width, (int)(width / sz.Width * sz.Height) );
	}

	/// <returns>the center of a Game Space</returns>
	PointF SpaceCenter( Space s ) {
		var norm = spaceLookup.ContainsKey(s.Label)
				? spaceLookup[s.Label]
			: s is MultiSpace ms
				? spaceLookup[ ms.Parts[0].Label ]
			: throw new ArgumentException($"Space {s.Label} does not have a screen location");
		return new PointF( norm.X * boardScreenSize.Width, norm.Y * boardScreenSize.Height );
	}

	protected override void OnSizeChanged( EventArgs e ) {
		base.OnSizeChanged( e );
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

	public event Action<IOption> OptionSelected;

	protected override void OnMouseMove( MouseEventArgs e ) {
		base.OnMouseMove( e );

		if(activeSpaces==null) return;

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
		return activeSpaces?.Select( s => {
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

	public event Action<Space> SpaceClicked;
	public event Action<Token> TokenClicked;
	public event Action<SpaceToken> SpaceTokenClicked;

	#region private fields

	void OptionProvider_OptionsChanged( IDecision decision ) {
		tokenOnSpace      = decision as Select.TokenFrom1Space;
		spaceTokens       = decision as Select.TypedDecision<SpaceToken>;
		deployedPresence  = decision as Select.DeployedPresence;
		deckDecision      = decision as Select.DeckToDrawFrom;
		elementDecision   = decision as Select.Element;

		adjacentInfo = decision is Select.IHaveAdjacentInfo adjacenInfoProvider
			? adjacenInfoProvider.AdjacentInfo
			: null;

		activeSpaces            = decision.Options.OfType<Space>().ToArray();
		fearCard                = decision.Options.OfType<ActivatedFearCard>().FirstOrDefault();
		clickableTrackOptions   = decision.Options.OfType<Track>().ToArray();
		selectableInnateOptions = decision.Options.OfType<InnatePower>().ToArray();
		selectableInnateGroupOptions = decision.Options.OfType<IDrawableInnateOption>().ToArray();
		selectableGrowthOptions = decision.Options.OfType<GrowthOption>().ToArray();
		selectableGrowthActions = decision.Options.OfType<GrowthActionFactory>().ToArray();
	}

	ActivatedFearCard fearCard;
	Select.TokenFrom1Space tokenOnSpace;
	Select.AdjacentInfo adjacentInfo;
	Select.TypedDecision<SpaceToken> spaceTokens;
	Select.DeployedPresence deployedPresence;
	Select.DeckToDrawFrom deckDecision;
	Select.Element elementDecision;
	Track[] clickableTrackOptions;
	InnatePower[] selectableInnateOptions;
	IDrawableInnateOption[] selectableInnateGroupOptions;
		
	GrowthOption[] selectableGrowthOptions;
	GrowthActionFactory[] selectableGrowthActions;

		
	readonly List<(Rectangle,IOption)> optionRects = new List<(Rectangle, IOption)>();

	readonly Dictionary<string,Rectangle> tokenLocations = new Dictionary<string, Rectangle>();

	GameState gameState;
	Spirit spirit;

	const float radius = 40f;
	Space[] activeSpaces;
	Dictionary<string,PointF> spaceLookup;

	Size boardScreenSize;
	Bitmap cachedBackground;
	string boardImageFile;

	Image presence;
	string presenceColor;
	Image strife;
	Image fear;
	Image grayFear;
	Dictionary<Token, Image> tokenImages;
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

class StopWatch : IDisposable {
	readonly string label;
	readonly DateTime start;
	public StopWatch(string label ) { this.label = label; start = DateTime.Now; }
	public void Dispose() {
		var dur = DateTime.Now - start;
		var duration = new RecordedDuration(label,(int)dur.TotalMilliseconds);
		timeLog.Add( duration );
	}
	static public List<RecordedDuration> timeLog = new List<RecordedDuration>();
}

class RecordedDuration {
	public int ms;
	public string label;
	public RecordedDuration(string label,int ms) { this.label=label; this.ms = ms; }
	public override string ToString() => $"{label}: {ms}ms";
}