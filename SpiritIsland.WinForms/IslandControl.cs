using SpiritIsland.BranchAndClaw;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public partial class IslandControl : Control {

		const float radius = 40f;

		public IslandControl() {
			InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint 
				| ControlStyles.UserPaint 
				| ControlStyles.OptimizedDoubleBuffer 
				| ControlStyles.ResizeRedraw, true
			);

		}

		Space[] activeSpaces;

		Dictionary<string,PointF> spaceLookup;

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
						["B0"] = new PointF( 0.191f, 0.392f ),
						["B1"] = new PointF( 0.401f, 0.252f ),
						["B2"] = new PointF( 0.280f, 0.570f ),
						["B3"] = new PointF( 0.203f, 0.857f ),
						["B4"] = new PointF( 0.477f, 0.683f ),
						["B5"] = new PointF( 0.575f, 0.473f ),
						["B6"] = new PointF( 0.642f, 0.245f ),
						["B7"] = new PointF( 0.725f, 0.605f ),
						["B8"] = new PointF( 0.810f, 0.183f ),
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

			dahan = Image.FromFile(".\\images\\Dahanicon.png");
			dahan1 = Image.FromFile( ".\\images\\Dahan1icon.png" );
			city = Image.FromFile(".\\images\\Cityicon.png");
			city2 = Image.FromFile( ".\\images\\City2icon.png" );
			city1 = Image.FromFile( ".\\images\\City1icon.png" );
			town = Image.FromFile(".\\images\\Townicon.png");
			town1 = Image.FromFile( ".\\images\\Town1icon.png" );
			explorer = Image.FromFile(".\\images\\Explorericon.png");
			presence = ResourceImages.Singleton.GetPresenceIcon( tokenColor );
			blight = Image.FromFile(".\\images\\Blighticon.png");
			defend = Image.FromFile(".\\images\\defend1orange.png");

			wilds = ResourceImages.Singleton.GetTokenIcon("wilds");
			disease = ResourceImages.Singleton.GetTokenIcon( "disease" );
			beast = ResourceImages.Singleton.GetTokenIcon( "beast" );
			strife = ResourceImages.Singleton.GetTokenIcon( "strife" );

			tokenImages = new Dictionary<Token, Image> {
				[Invader.City[3]] = city,
				[Invader.City[2]] = city2,
				[Invader.City[1]] = city1,
				[Invader.Town[2]] = town,
				[Invader.Town[1]] = town1,
				[Invader.Explorer[1]] = explorer,
				[TokenType.Dahan[2]] = dahan,
				[TokenType.Dahan[1]] = dahan1,
				[TokenType.Defend] = defend,
				[TokenType.Blight] = blight,
				[BacTokens.Beast] = beast,
				[BacTokens.Wilds] = wilds,
				[BacTokens.Disease] = disease,
			};

			this.gameState = gameState;
			this.spirit = gameState.Spirits.Single();
		}

		void OptionProvider_OptionsChanged( IDecision decision ) {
			tokenOnSpace      = decision as Decision.TokenOnSpace;
			adjacentDecision  = decision as Decision.IsGatherOrPush;
			spaceTokens       = decision as Decision.TypedDecision<SpaceToken>;
			this.activeSpaces = decision.Options.OfType<Space>().ToArray();
			fearCard          = decision.Options.OfType<DisplayFearCard>().FirstOrDefault();
		}

		DisplayFearCard fearCard;
		Decision.TokenOnSpace tokenOnSpace;
		Decision.IsGatherOrPush adjacentDecision;
		Decision.TypedDecision<SpaceToken> spaceTokens;
		
		readonly List<(Rectangle,IOption)> optionRects = new List<(Rectangle, IOption)>();

		readonly Dictionary<string,Rectangle> tokenLocations = new Dictionary<string, Rectangle>();

		Image board;

		Image dahan;
		Image dahan1;
		Image city;
		Image city2;
		Image city1;
		Image town;
		Image town1;
		Image explorer;
		Image presence;
		Image blight;
		Image defend;

		Image disease;
		Image beast;
		Image strife;
		Image wilds;

		GameState gameState;
		Spirit spirit;

		Size boardScreenSize;

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
				DrawHighlights( pe );
				DrawInvaderCards( pe.Graphics );
			}
		}

		void DrawInvaderCards( Graphics graphics ) {

			// Invaders
			const float margin = 15;
			const float textHeight = 20f;
			float height = this.Height *.33f;
			float width = height * .66f;
			using var buildRavageFont = new Font( ResourceImages.Singleton.Fonts.Families[0], textHeight );

			float x = ClientRectangle.Width-width-margin-margin;
			float y = ClientRectangle.Height-height-margin*2 - textHeight;

			// Build Metrics
			var buildRect = new RectangleF( x, y, width, height );
			float buildTextWidth = graphics.MeasureString( "Build", buildRavageFont ).Width;
			var buildTextTopLeft = new PointF( x + (width - buildTextWidth) / 2, ClientRectangle.Bottom - textHeight - margin );

			// Ravage Metrics
			float ravageX = x - (width + margin);
			var ravageRect = new RectangleF( ravageX, y, width, height );
			float textWidth = graphics.MeasureString( "Ravage", buildRavageFont ).Width;
			PointF ravageTextTopLeft = new PointF( ravageX + (width - textWidth) / 2, ClientRectangle.Bottom - textHeight - margin );

			// Fear Metrics
			var fearHeight = this.Height * .8f;
			var fearWidth = fearHeight * .66f;
			var fearRect = new RectangleF( Width - fearWidth - this.Height * .1f, (Height - fearHeight) / 2, fearWidth, fearHeight );

			// Build
			graphics.DrawInvaderCard( buildRect, gameState.InvaderDeck.Build );
			graphics.DrawString("Build", buildRavageFont,Brushes.Black, buildTextTopLeft );

			// Ravage
			graphics.DrawInvaderCard( ravageRect, gameState.InvaderDeck.Ravage );
			graphics.DrawString( "Ravage", buildRavageFont, Brushes.Black, ravageTextTopLeft );

			// Fear
			graphics.DrawFearCard( fearRect, fearCard );

		}

		void DrawHighlights( PaintEventArgs pe ) {
			using var pen = new Pen(Brushes.Aquamarine,5);

			// Space Circles
			if(activeSpaces != null)
				foreach(var space in activeSpaces) {
					var center = SpaceCenter(space);
					pe.Graphics.DrawEllipse( pen, center.X- radius, center.Y- radius, radius * 2, radius * 2 );
				}

			// adjacent
			if(adjacentDecision != null) {

				var center = SpaceCenter(adjacentDecision.Original);
				var others = adjacentDecision.Adjacent.Select(x=> SpaceCenter(x) ).ToArray();

				using Pen p = new Pen( Color.DeepSkyBlue, 7 );
				var drawer = new ArrowDrawer(pe.Graphics,p);
				switch(adjacentDecision.GatherPush) {
					case Decision.GatherPush.Gather:
						foreach(var other in others)
							drawer.Draw( other, center );
						break;
					case Decision.GatherPush.Push:
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

		void DecorateSpace( Graphics graphics, Space space ) {
			if(!spaceLookup.ContainsKey(space.Label)) return; // happens during developement

			PointF normalized = spaceLookup[space.Label];
			PointF xy = new PointF(normalized.X * boardScreenSize.Width, normalized.Y * boardScreenSize.Height);
			float iconWidth = boardScreenSize.Width * .035f; 
			float xStep = iconWidth + 10f;

			float x = xy.X - iconWidth;
			float y = xy.Y - iconWidth;

			TokenCountDictionary tokens = gameState.Tokens[space];
			DrawInvaderRow( graphics, x, ref y, iconWidth, xStep, tokens );

			// dahan & presence & blight
			int presenceCount = spirit.Presence.CountOn( space );
			bool isSS = spirit.SacredSites.Contains( space );
			DrawRow( graphics, tokens, x, ref y, iconWidth, xStep, presenceCount, isSS
				, TokenType.Dahan[2], TokenType.Dahan[1],TokenType.Defend,TokenType.Blight
			);

			DrawRow( graphics, tokens, x, ref y, iconWidth, xStep, 0, false
				, BacTokens.Beast, BacTokens.Wilds, BacTokens.Disease
			);

		}

		Dictionary<Token, Image> tokenImages;

		void DrawInvaderRow( Graphics graphics, float x, ref float y, float width, float step, TokenCountDictionary tokens ) {

			Space space = tokens.Space;

			// tokens
			if(!tokens.HasInvaders()) return;

			float maxHeight = 0;

			foreach(Token token in tokens.Invaders()) {

				// Strife
				Token imageToken;
				if(token is StrifedInvader si) {
					imageToken = si.WithStrife( 0 );

					Rectangle strifeRect = FitWidth( x, y, width, strife );
					graphics.DrawImage( strife, strifeRect );
					graphics.DrawSuperscript( strifeRect, tokens[token] );
				} else {
					imageToken = token;
				}


				// Draw Token
				Image img = tokenImages[imageToken];
				Rectangle rect = FitWidth( x, y, width, img );
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

		private static Rectangle FitWidth( float x, float y, float width, Image img ) {
			return new Rectangle( (int)x, (int)y, (int)width, (int)(width / img.Width * img.Height) );
		}

		private void DrawRow( Graphics graphics, TokenCountDictionary tokens, float x, ref float y, float width, float step, int presenceCount, bool isSacredSite, params Token[] tokenTypes ) {
			float maxHeight = 0;

			using Font countFont = new( "Arial", 7, FontStyle.Bold, GraphicsUnit.Point );

			foreach(var token in tokenTypes) {
				int count = tokens[token];
				if(count == 0) continue;
				var img = tokenImages[token];

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
				var rect = new Rectangle( (int)x, (int)y, (int)width, (int)height );
				x += step;

				if( isSacredSite ) {
					const int inflationSize = 10;
					rect.Inflate( inflationSize, inflationSize );
					Color newColor = Color.FromArgb( 100, Color.Yellow );
					using var brush = new SolidBrush(newColor);
					graphics.FillEllipse(brush,rect);
					rect.Inflate( -inflationSize, -inflationSize );
				}

				// Draw Presence
				graphics.DrawImage( presence, rect );
				graphics.DrawSubscript( rect, presenceCount );

			}

			float gap = step-width;
			y += maxHeight + gap;
		}

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

		private PointF SpaceCenter( Space s ) {
			var norm = this.spaceLookup[s.Label];
			return new PointF( norm.X * boardScreenSize.Width, norm.Y * boardScreenSize.Height );
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

	}

}
