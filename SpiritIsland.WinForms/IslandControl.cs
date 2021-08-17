using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms
{
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

		public void ActivateSpaces(IEnumerable<Space> spaces){
			this.activeSpaces = spaces.ToArray();
		}

		Dictionary<string,PointF> spaceLookup;

		public void InitBoard(GameState gameState){

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
			presence = Image.FromFile(".\\images\\Presenceicon.png");
			blight = Image.FromFile(".\\images\\Blighticon.png");
			defend = Image.FromFile(".\\images\\defend1orange.png");

			this.gameState = gameState;
			this.spirit = gameState.Spirits.Single();
		}

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

			if(gameState != null)
				foreach(var space in gameState.Island.Boards[0].Spaces)
					DecorateSpace(pe.Graphics,space);

			if(activeSpaces != null)
				CirleActiveSpaces( pe );

		}

		void CirleActiveSpaces( PaintEventArgs pe ) {
			using var pen = new Pen(Brushes.Aquamarine,5);
			foreach(var space in activeSpaces) {
				var center = SpaceCenter(space);
				pe.Graphics.DrawEllipse( pen, center.X- radius, center.Y- radius, radius * 2, radius * 2 );
			}
		}
		
		void DecorateSpace( Graphics graphics, Space space ) {
			if(!spaceLookup.ContainsKey(space.Label)) return; // happens during developement

			PointF normalized = spaceLookup[space.Label];
			PointF xy = new PointF(normalized.X * boardScreenSize.Width, normalized.Y * boardScreenSize.Height);
			float iconWidth = boardScreenSize.Width * .025f, xStep = iconWidth + 10f;

			float x = xy.X - iconWidth;
			float y = xy.Y - iconWidth;
			// invaders
			var grp = gameState.InvadersOn( space );
			var invaders = grp.InvaderTypesPresent_Specific.Select( k => grp[k] + ":" + k.Summary ).Join( " " );
			CountDictionary<Image> images = new();
			images[city] = grp[InvaderSpecific.City];
			images[city2] = grp[InvaderSpecific.City2];
			images[city1] = grp[InvaderSpecific.City1];
			images[town] = grp[InvaderSpecific.Town];
			images[town1] = grp[InvaderSpecific.Town1];
			images[explorer] = grp[InvaderSpecific.Explorer];;
			DrawRow( graphics, x, ref y, iconWidth, xStep, images );

			// dahan & presence & blight
			images.Clear();
			images[dahan] = gameState.GetDahanOnSpace( space );
			images[defend] = gameState.GetDefence( space );
			images[presence] = spirit.Presence.On( space );
			images[blight] = gameState.GetBlightOnSpace( space );
			DrawRow( graphics, x, ref y, iconWidth, xStep, images );
		}

		private static void DrawRow( Graphics graphics, float x, ref float y, float width, float step, CountDictionary<Image> images ) {
			if(!images.Keys.Any()) return;
			float maxHeight = 0;

			using Font simpleFont = new( "Arial", 6, FontStyle.Bold, GraphicsUnit.Point );
			const float circleDiameter = 10f;

			foreach(var img in images.Keys){
				float height = width / img.Width * img.Height;
				maxHeight = Math.Max(maxHeight,height); 
				graphics.DrawImage( img, x, y, width, height);
				int count = images[img];
				if(count > 1) {
					var numRect = new RectangleF(x-2,y+height-2, circleDiameter, circleDiameter );
					graphics.FillEllipse(Brushes.White,numRect);
					graphics.DrawString(count.ToString(),simpleFont,Brushes.Black,numRect.X+2,numRect.Y);
				}
				x += step;
			}

			float gap = step-width;
			y += maxHeight + gap;
		}

		protected override void OnClick( EventArgs e ) {

			Point clientCoords = this.PointToClient(Control.MousePosition);

			var xx = activeSpaces
				.Select(s=> {
					PointF center = SpaceCenter( s );
					float dx = clientCoords.X - center.X;
					float dy = clientCoords.Y - center.Y;
					return new { Space = s, d2 = dx * dx + dy * dy };
				} )
				.ToArray();

			var match = xx
				.Where(x=>x.d2<radius*radius)
				.OrderBy(x=>x.d2)
				.Select(x=>x.Space)
				.FirstOrDefault();

			if(match != null) {
				SpaceClicked?.Invoke(match);
				return;
			}

			// Calculate %
			if(board==null) return;
			float normalizedX = (float)clientCoords.X / (float)boardScreenSize.Width;
			float normalizedY = (float)clientCoords.Y / (float)boardScreenSize.Height;


			string msg = $"({normalizedX:0.###},{normalizedY:0.###})";
			Clipboard.SetText(msg);
			MessageBox.Show(msg);

		}

		private PointF SpaceCenter( Space s ) {
			var norm = this.spaceLookup[s.Label];
			return new PointF( norm.X * boardScreenSize.Width, norm.Y * boardScreenSize.Height );
		}

		protected override void OnMouseMove( MouseEventArgs e ) {
			base.OnMouseMove( e );

			if(activeSpaces==null) return;

			var clientCoord = this.PointToClient(Control.MousePosition);
			bool inCircle = activeSpaces
				.Select(s=>{
					var center = SpaceCenter(s);
					float dx = clientCoord.X-center.X, dy=clientCoord.Y-center.Y;
					return dx*dx+dy*dy;
				})
				.Any(distSquared => distSquared<radius*radius);
			Cursor = inCircle ? Cursors.Hand : Cursors.Default;

		}

		public event Action<Space> SpaceClicked;

	}

}
