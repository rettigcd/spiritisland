using SpiritIsland.BranchAndClaw;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {

	public partial class IslandControl : Control {

		public IslandControl() {
			InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint 
				| ControlStyles.UserPaint 
				| ControlStyles.OptimizedDoubleBuffer 
				| ControlStyles.ResizeRedraw, true
			);

		}

		public void Init( GameState gameState, IHaveOptions optionProvider, string tokenColor ) {

			optionProvider.NewDecision += OptionProvider_OptionsChanged;

			var board = gameState.Island.Boards.VerboseSingle("Multiple Island boards not supported.");
            switch(board[0].Label.Substring( 0, 1 )) {
				case "A":
					this.board = Image.FromFile( ".\\images\\board a.png" );
					spaceLookup = new Dictionary<string, PointF> {
						["A0"] = new PointF( 0.119f, 0.526f ),
						["A1"] = new PointF( 0.417f, 0.304f ),
						["A2"] = new PointF( 0.252f, 0.524f ),
						["A3"] = new PointF( 0.165f, 0.815f ),
						["A4"] = new PointF( 0.419f, 0.711f ),
						["A5"] = new PointF( 0.608f, 0.543f ),
						["A6"] = new PointF( 0.585f, 0.269f ),
						["A7"] = new PointF( 0.752f, 0.611f ),
						["A8"] = new PointF( 0.814f, 0.189f ),
					};
					break;
                case "B":
					this.board = Image.FromFile( ".\\images\\board b.png" );
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
					this.board = Image.FromFile( ".\\images\\board c.png" );
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
					this.board = Image.FromFile( ".\\images\\board d.png" );
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
			//			boardA = Image.FromFile(".\\images\\board a.png");

			var images = ResourceImages.Singleton;
			presence = images.GetPresenceIcon( tokenColor );
			strife   = images.GetToken( "strife" );
			fear     = images.GetToken( "fear" );
			grayFear = images.GetToken( "fear_gray");

			tokenImages = new Dictionary<Token, Image> {
				[Invader.City[3]]     = images.GetToken( "city" ),
				[Invader.City[2]]     = images.GetToken( "city2" ),
				[Invader.City[1]]     = images.GetToken( "city1" ),
				[Invader.Town[2]]     = images.GetToken( "town" ),
				[Invader.Town[1]]     = images.GetToken( "town1" ),
				[Invader.Explorer[1]] = images.GetToken( "explorer" ),
				[TokenType.Dahan[2]]  = images.GetToken( "dahan" ),
				[TokenType.Dahan[1]]  = images.GetToken( "dahan1" ),
				[TokenType.Defend]    = images.GetToken( "defend1orange" ),
				[TokenType.Blight]    = images.GetToken( "blight" ),
				[TokenType.Beast]     = images.GetToken( "beast" ),
				[TokenType.Wilds]     = images.GetToken("wilds"),
				[TokenType.Disease]   = images.GetToken( "disease" ),
				[TokenType.Badlands]  = images.GetToken( "badlands" ),
			};

			this.gameState = gameState;
			this.spirit = gameState.Spirits.Single();
		}

		void OptionProvider_OptionsChanged( IDecision decision ) {
			tokenOnSpace      = decision as Decision.TokenOnSpace; // 
			adjacentDecision  = decision as Decision.IAdjacentDecision;
			spaceTokens       = decision as Decision.TypedDecision<SpaceToken>;
			deployedPresence  = decision as Decision.Presence.Deployed;
			this.activeSpaces = decision.Options.OfType<Space>().ToArray();
			fearCard          = decision.Options.OfType<DisplayFearCard>().FirstOrDefault();
		}

		#region Paint

		protected override void OnPaint( PaintEventArgs pe ) {
			base.OnPaint( pe );

			if(board != null) {
				// Assume limit is height
				boardScreenSize = (board.Width * Height > Width * board.Height)
					? new Size( Width, board.Height * Width / board.Width )
					: new Size( board.Width * Height / board.Height, Height );
				int boardHeight = boardScreenSize.Height;
				int boardWidth = boardScreenSize.Width;

				pe.Graphics.DrawImage( board, 0, 0, boardWidth, boardHeight );
			}

			optionRects.Clear();
			tokenLocations.Clear();

			if(gameState != null) {
				foreach(var space in gameState.Island.Boards[0].Spaces)
					DecorateSpace(pe.Graphics,space);
				DrawFearPool( pe.Graphics, new RectangleF(Width*.75f,0f,Width*.25f,Width*.05f ) );
				DrawBlight  ( pe.Graphics, new RectangleF(Width*.80f,Width*.05f,Width*.20f,Width*.03f ) );
				DrawRound( pe.Graphics );
				DrawInvaderCards( pe.Graphics ); // other than highlights, do this last since it contains the Fear Card that we want to be on top of everything.

				DrawHighlights( pe );
			}

		}

		void DrawRound( Graphics graphics ) {
			using var font = new Font( ResourceImages.Singleton.Fonts.Families[0], Height*.065f, GraphicsUnit.Pixel );
			graphics.DrawString("round: "+gameState.RoundNumber, font, Brushes.SteelBlue, 0, 0);
		}

		void DrawInvaderCards( Graphics graphics ) {

			// Invaders
			const float margin = 15;
			const float textHeight = 20f;
			float height = this.Height *.33f;
			float width = height * .66f;
			using var buildRavageFont = new Font( ResourceImages.Singleton.Fonts.Families[0], textHeight, GraphicsUnit.Pixel );

			float x = ClientRectangle.Width-width-margin-margin;
			float y = ClientRectangle.Height-height-margin*2 - textHeight;

			// Build Metrics
			int buildCount = gameState.InvaderDeck.Build.Count;
			RectangleF[] buildRect = new RectangleF[ buildCount ];
			float buildWidth = width / buildCount, buildHeight = height / buildCount;
			for(int i=0;i<buildCount;++i)
				buildRect[i] = new RectangleF( x+i*buildWidth, y+i*buildHeight, buildWidth, buildHeight );
			float buildTextWidth = graphics.MeasureString( "Build", buildRavageFont ).Width;
			PointF buildTextTopLeft = new PointF( x + (width - buildTextWidth) / 2, ClientRectangle.Bottom - textHeight - margin );

			// Ravage Metrics
			float ravageX = x - (width + margin);
			int ravageCounts = gameState.InvaderDeck.Ravage.Count;
			var ravageRects = new RectangleF[ ravageCounts];
			float ravageWidth = width / ravageCounts, ravageHeight = height / ravageCounts;
			for(int i=0;i<ravageCounts;++i)
				ravageRects[i] = new RectangleF( ravageX+i*ravageWidth, y+i*ravageHeight, ravageWidth, ravageHeight );
			float textWidth = graphics.MeasureString( "Ravage", buildRavageFont ).Width;
			PointF ravageTextTopLeft = new PointF( ravageX + (width - textWidth) / 2, ClientRectangle.Bottom - textHeight - margin );

			// Fear Metrics
			var fearHeight = this.Height * .8f;
			var fearWidth = fearHeight * .66f;
			var fearRect = new RectangleF( Width - fearWidth - this.Height * .1f, (Height - fearHeight) / 2, fearWidth, fearHeight );

			// Build
			for(int i=0;i<buildRect.Length;++i) 
				graphics.DrawInvaderCard( buildRect[i], gameState.InvaderDeck.Build[i] );
			graphics.DrawString("Build", buildRavageFont,Brushes.Black, buildTextTopLeft );

			// Ravage
			for(int i=0; i<ravageRects.Length;++i)
				graphics.DrawInvaderCard( ravageRects[i], gameState.InvaderDeck.Ravage[i] );
			graphics.DrawString( "Ravage", buildRavageFont, Brushes.Black, ravageTextTopLeft );

			// Fear
			graphics.DrawFearCard( fearRect, fearCard );

		}

		void DrawHighlights( PaintEventArgs pe ) {
			using var pen = new Pen(Brushes.Aquamarine,5);

			// adjacent
			if(adjacentDecision != null) {

				var center = SpaceCenter(adjacentDecision.Original);
				var others = adjacentDecision.Adjacent.Select(x=> SpaceCenter(x) ).ToArray();

				using Pen p = new Pen( Color.DeepSkyBlue, 7 );
				var drawer = new ArrowDrawer(pe.Graphics,p);
				switch(adjacentDecision.Direction) {
					case Decision.AdjacentDirection.Incoming:
						foreach(var other in others)
							drawer.Draw( other, center );
						break;
					case Decision.AdjacentDirection.Outgoing:
						foreach(var other in others)
							drawer.Draw( center, other );
						break;
				}

			}

			if(spaceTokens != null) {
				// Draw tokens on space && set them as hotspots
				foreach(var st in spaceTokens.Options.OfType<SpaceToken>()) {
					string key = st.Space.Label + ":" + st.Token.Summary;
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
					string key = tokenOnSpace.Space.Label + ":" + token.Summary;
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
			bool isSS = spirit.SacredSites.Contains( space );
			DrawRow( graphics, tokens, x, ref y, iconWidth, xStep, presenceCount, isSS
				, TokenType.Dahan[2], TokenType.Dahan[1], TokenType.Defend, TokenType.Blight
			);

			DrawRow( graphics, tokens, x, ref y, iconWidth, xStep, 0, false
				, TokenType.Beast, TokenType.Wilds, TokenType.Disease, TokenType.Badlands
			);

		}

		void DrawInvaderRow( Graphics graphics, float x, ref float y, float width, float step, TokenCountDictionary tokens ) {

			Space space = tokens.Space;

			// tokens
			if(!tokens.HasInvaders()) return;

			float maxHeight = 0;

			var orderedInvaders = tokens.Invaders()
				.OrderByDescending(i=>i.FullHealth)
				.ThenBy(i=>i.Health); // show damaged first so when we apply damage, the damaged one replaces the old undamaged one.

			foreach(Token token in orderedInvaders) {

				// Strife
				Token imageToken;
				if(token is StrifedInvader si) {
					imageToken = si.WithStrife( 0 );

					Rectangle strifeRect = FitWidth( x, y, width, strife.Size );
					graphics.DrawImage( strife, strifeRect );
					graphics.DrawSuperscript( strifeRect, tokens[token] );
				} else {
					imageToken = token;
				}


				// Draw Token
				Image img = tokenImages[imageToken];
				Rectangle rect = FitWidth( x, y, width, img.Size );
				graphics.DrawImage( img, rect );

				// record token location
				tokenLocations.Add(space.Label+":"+token.Summary,rect);

				// Count
				graphics.DrawSubscript( rect, tokens[token] );

				maxHeight = Math.Max( maxHeight, rect.Height );
				x += step;
			}

			float gap = step - width;
			y += maxHeight + gap;
		}

		void DrawFearPool( Graphics graphics, RectangleF bounds ) {
			float margin = Math.Max(5f, bounds.Height*.05f);
			float slotWidth = bounds.Height; 

			int fearCount = this.gameState.Fear.Pool;
			int maxSpaces = this.gameState.Fear.ActivationThreshold;

			float step = (bounds.Width-2*margin-3*slotWidth)/(maxSpaces-1); 
			// -1 slot width for #/# and 
			// -1 slot width for last fear token
			float tokenWidth = slotWidth-2*margin;
			float tokenHeight = fear.Height * tokenWidth / fear.Width;
			RectangleF CalcBounds(int i) => new RectangleF( bounds.X+slotWidth+margin+step*i, bounds.Y+margin, tokenWidth, tokenHeight );

			// Terror Level
			using var terror = ResourceImages.Singleton.GetIcon( "TerrorLevel"+gameState.Fear.TerrorLevel );
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
				using var card = ResourceImages.Singleton.GetToken( "fearcard" );
				var rect = new RectangleF(bounds.Right-margin-slotWidth,bounds.Y+margin,tokenWidth,tokenHeight);
				graphics.DrawImageFitHeight(card,rect);
				rect.X -= rect.Width * .25f; // shift x2 left onto the card
				graphics.DrawSubscript( rect.ToInts(), activated );
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

			if(gameState.BlightCard.IslandIsBlighted)
				graphics.DrawString("Blighted!",SystemFonts.DialogFont, Brushes.Red, bounds.Right-slotWidth*1.5f,bounds.Top);
		}


		void DrawRow( Graphics graphics, TokenCountDictionary tokens, float x, ref float y, float width, float step, int presenceCount, bool isSacredSite, params Token[] tokenTypes ) {
			float maxHeight = 0;

			using Font countFont = new( "Arial", 7, FontStyle.Bold, GraphicsUnit.Point );

			foreach(var token in tokenTypes) {
				int count = tokens[token];
				if(count == 0) continue;

				var tokenForImage = token;
				if( token.Generic == TokenType.Dahan && token.Health > token.FullHealth ) tokenForImage = TokenType.Dahan.Default;
				var img = tokenImages[tokenForImage];

				// calc rect
				float height = width / img.Width * img.Height;
				maxHeight = Math.Max( maxHeight, height );
				var rect = new Rectangle( (int)x, (int)y, (int)width, (int)height );
				x += step;

				// record token location
				tokenLocations.Add( tokens.Space.Label + ":" + token.Summary, rect );

				// Draw Tokens
				graphics.DrawImage( img, rect );
				graphics.DrawSubscript( rect, count );
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
				graphics.DrawSubscript( presenceRect, presenceCount );

			}

			float gap = step-width;
			y += maxHeight + gap;
		}

		static Rectangle FitWidth( float x, float y, float width, Size sz ) {
			return new Rectangle( (int)x, (int)y, (int)width, (int)(width / sz.Width * sz.Height) );
		}

		/// <returns>the center of a Game Space</returns>
		PointF SpaceCenter( Space s ) {
			var norm = this.spaceLookup[s.Label];
			return new PointF( norm.X * boardScreenSize.Width, norm.Y * boardScreenSize.Height );
		}

		#endregion

		protected override void OnClick( EventArgs e ) {

			Point clientCoords = this.PointToClient(Control.MousePosition);

			IOption match = FindOption();
			if(match is Space space)
				SpaceClicked?.Invoke(space);
			else if(match is Token invader)
				TokenClicked?.Invoke( invader );
			else if(match is SpaceToken st)
				SpaceTokenClicked?.Invoke( st );

		}

		protected override void OnMouseMove( MouseEventArgs e ) {
			base.OnMouseMove( e );

			if(activeSpaces==null) return;

			bool inCircle = FindOption() != null;
			Cursor = inCircle ? Cursors.Hand : Cursors.Default;

		}

		IOption FindOption() {
			Point clientCoords = this.PointToClient( Control.MousePosition );
			return FindInvader( clientCoords )
				?? FindSpaces( clientCoords );
		}

		IOption FindSpaces( Point clientCoords ) {
			if(activeSpaces==null) return null;
			return activeSpaces
				.Select( s => {
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

		public event Action<Space> SpaceClicked;
		public event Action<Token> TokenClicked;
		public event Action<SpaceToken> SpaceTokenClicked;

		#region private fields

		DisplayFearCard fearCard;
		Decision.TokenOnSpace tokenOnSpace;
		Decision.IAdjacentDecision adjacentDecision;
		Decision.TypedDecision<SpaceToken> spaceTokens;
		Decision.Presence.Deployed deployedPresence;
		
		readonly List<(Rectangle,IOption)> optionRects = new List<(Rectangle, IOption)>();

		readonly Dictionary<string,Rectangle> tokenLocations = new Dictionary<string, Rectangle>();

		GameState gameState;
		Spirit spirit;

		Size boardScreenSize;
		const float radius = 40f;
		Space[] activeSpaces;
		Dictionary<string,PointF> spaceLookup;

		Image board;
		Image presence;
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


}
