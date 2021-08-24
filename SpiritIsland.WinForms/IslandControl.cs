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
			city = Image.FromFile(".\\images\\Cityicon.png");
			city2 = Image.FromFile( ".\\images\\City2icon.png" );
			city1 = Image.FromFile( ".\\images\\City1icon.png" );
			town = Image.FromFile(".\\images\\Townicon.png");
			town1 = Image.FromFile( ".\\images\\Town1icon.png" );
			explorer = Image.FromFile(".\\images\\Explorericon.png");
			presence = ResourceImages.Singleton.GetPresenceIcon( tokenColor );
//			presence = Image.FromFile( ".\\images\\Presenceicon.png" );
			blight = Image.FromFile(".\\images\\Blighticon.png");
			defend = Image.FromFile(".\\images\\defend1orange.png");

			invaderImages = new Dictionary<InvaderSpecific, Image> {
				[InvaderSpecific.City] = city,
				[InvaderSpecific.City2] = city2,
				[InvaderSpecific.City1] = city1,
				[InvaderSpecific.Town] = town,
				[InvaderSpecific.Town1] = town1,
				[InvaderSpecific.Explorer] = explorer,
			};

			this.gameState = gameState;
			this.spirit = gameState.Spirits.Single();
		}

		void OptionProvider_OptionsChanged( IDecision decision ) {
			ios = decision as InvadersOnSpaceDecision;
			this.activeSpaces = decision.Options.OfType<Space>().ToArray();
		}
		InvadersOnSpaceDecision ios;
		readonly List<(Rectangle,IOption)> optionRects = new List<(Rectangle, IOption)>();

		Image board;

		Image dahan;
		Image city;
		Image city2;
		Image city1;
		Image town;
		Image town1;
		Image explorer;
		Image presence;
		Image blight;
		Image defend;

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

			if(gameState != null)
				foreach(var space in gameState.Island.Boards[0].Spaces)
					DecorateSpace(pe.Graphics,space);

			DrawHighlights( pe );

			DrawInvaderCards( pe.Graphics );

		}

		void DrawInvaderCards( Graphics graphics ) {
			const float margin = 15;
			const float textHeight = 20f;
			const float width = 160f;
			const float height = 240f;

			float x = ClientRectangle.Width-width-margin-margin;
			float y = ClientRectangle.Height-height-margin*2;

			using var myFont = new Font( ResourceImages.Singleton.Fonts.Families[0], textHeight );
			graphics.DrawInvaderCard(new RectangleF(x,y-textHeight,width,height),gameState.InvaderDeck.Build);
			float textWidth = graphics.MeasureString("Build",myFont).Width;
			graphics.DrawString("Build", myFont,Brushes.Black, x+(width-textWidth)/2, ClientRectangle.Bottom-textHeight-margin);

			x-=width;
			x-=margin;
			graphics.DrawInvaderCard( new RectangleF( x, y-textHeight, width, height ), gameState.InvaderDeck.Ravage );
			textWidth = graphics.MeasureString( "Ravage", myFont ).Width;
			graphics.DrawString( "Ravage", myFont, Brushes.Black, x+(width-textWidth)/2, ClientRectangle.Bottom - textHeight-margin );
		}

		void DrawHighlights( PaintEventArgs pe ) {
			using var pen = new Pen(Brushes.Aquamarine,5);
			if(activeSpaces != null)
				foreach(var space in activeSpaces) {
					var center = SpaceCenter(space);
					pe.Graphics.DrawEllipse( pen, center.X- radius, center.Y- radius, radius * 2, radius * 2 );
				}
			foreach(var (rect,_) in optionRects)
				pe.Graphics.DrawRectangle(pen,rect);

		}
		
		void DecorateSpace( Graphics graphics, Space space ) {
			if(!spaceLookup.ContainsKey(space.Label)) return; // happens during developement

			PointF normalized = spaceLookup[space.Label];
			PointF xy = new PointF(normalized.X * boardScreenSize.Width, normalized.Y * boardScreenSize.Height);
			float iconWidth = boardScreenSize.Width * .035f; 
			float xStep = iconWidth + 10f;

			float x = xy.X - iconWidth;
			float y = xy.Y - iconWidth;

			DrawInvaderRow( graphics, x, ref y, iconWidth, xStep, space );

			// dahan & presence & blight
			CountDictionary<Image> images = new();
			images.Clear();
			images[dahan] = gameState.DahanCount( space );
			images[defend] = gameState.GetDefence( space );
			images[presence] = spirit.Presence.CountOn( space );
			images[blight] = gameState.GetBlightOnSpace( space );
			DrawRow( graphics, x, ref y, iconWidth, xStep, images );
		}

		Dictionary<InvaderSpecific, Image> invaderImages;

		void DrawInvaderRow( Graphics graphics, float x, ref float y, float width, float step, Space space ) {
			bool isInvaderSpace = ios!=null && ios.Space == space;

			// invaders
			var grp = gameState.InvadersOn( space );
			if(grp.TotalCount==0) return;

			float maxHeight = 0;

			foreach(var specific in grp.InvaderTypesPresent_Specific) {
				var img = invaderImages[specific];

				// Draw Invaders
				float height = width / img.Width * img.Height;
				var rect = new Rectangle( (int)x, (int)y, (int)width, (int)height );
				maxHeight = Math.Max( maxHeight, height );
				graphics.DrawImage( img, rect );
				if(isInvaderSpace)
					optionRects.Add( (rect, specific) );

				// Count
				graphics.DrawCount( rect, grp[specific] );

				x += step;
			}

			float gap = step - width;
			y += maxHeight + gap;
		}

		private static void DrawRow( Graphics graphics, float x, ref float y, float width, float step, CountDictionary<Image> images ) {
			if(!images.Keys.Any()) return;
			float maxHeight = 0;

			using Font countFont = new( "Arial", 7, FontStyle.Bold, GraphicsUnit.Point );

			foreach(var img in images.Keys){

				// Draw Tokens
				float height = width / img.Width * img.Height;
				maxHeight = Math.Max(maxHeight,height); 
				var rect = new Rectangle((int)x, (int)y, (int)width, (int)height );
				graphics.DrawImage( img, rect );

				graphics.DrawCount( rect, images[img] );

				x += step;
			}

			float gap = step-width;
			y += maxHeight + gap;
		}

		protected override void OnClick( EventArgs e ) {

			Point clientCoords = this.PointToClient(Control.MousePosition);

			var match = FindOption();
			if(match is Space space)
				SpaceClicked?.Invoke(space);
			else if(match is InvaderSpecific invader)
				InvaderClicked?.Invoke( invader );

			// Calculate %
			//if(board==null) return;
			//float normalizedX = (float)clientCoords.X / (float)boardScreenSize.Width;
			//float normalizedY = (float)clientCoords.Y / (float)boardScreenSize.Height;
			//string msg = $"({normalizedX:0.###},{normalizedY:0.###})";
			//Clipboard.SetText(msg);
			//MessageBox.Show(msg);

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
		public event Action<InvaderSpecific> InvaderClicked;

	}

}
