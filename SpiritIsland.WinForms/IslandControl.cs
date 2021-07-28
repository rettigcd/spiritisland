using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpiritIsland.WinForms {
	public partial class IslandControl : Control {

		const float radius = 80f;

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
						["A0"] = new PointF( 152f, 475f ),
						["A1"] = new PointF( 559f, 296f ),
						["A2"] = new PointF( 326f, 447f ),
						["A3"] = new PointF( 196f, 758f ),
						["A4"] = new PointF( 547f, 633f ),
						["A5"] = new PointF( 810f, 467f ),
						["A6"] = new PointF( 790f, 190f ),
						["A7"] = new PointF( 1032f, 531f ),
						["A8"] = new PointF( 1123f, 126f ),
					};
					break;
                case "B":
					this.board = Image.FromFile( ".\\images\\board b.png" );
					spaceLookup = new Dictionary<string, PointF> {
						["B0"] = new PointF( 185f, 593f ),
						["B1"] = new PointF( 592f, 226f ),
						["B2"] = new PointF( 386f, 566f ),
						["B3"] = new PointF( 265f, 867f ),
						["B4"] = new PointF( 696f, 660f ),
						["B5"] = new PointF( 876f, 455f ),
						["B6"] = new PointF( 996f, 212f ),
						["B7"] = new PointF( 1134f, 631f ),
						["B8"] = new PointF( 1307f, 152f ),
					};
					break;
                case "C":
					this.board = Image.FromFile( ".\\images\\board c.png" );
					spaceLookup = new Dictionary<string, PointF> {
						["C0"] = new PointF( 121f, 519f ),
						["C1"] = new PointF( 213f, 770f ),
						["C2"] = new PointF( 333f, 502f ),
						["C3"] = new PointF( 445f, 186f ),
						["C4"] = new PointF( 644f, 751f ),
						["C5"] = new PointF( 638f, 475f ),
						["C6"] = new PointF( 776f, 175f ),
						["C7"] = new PointF( 927f, 453f ),
						["C8"] = new PointF( 1074f, 96f ),
					};
					break;
                case "D":
					this.board = Image.FromFile( ".\\images\\board d.png" );
					spaceLookup = new Dictionary<string, PointF> {
						["D0"] = new PointF( 188f, 408f ),
						["D1"] = new PointF( 609f, 118f ),
						["D2"] = new PointF( 349f, 479f ),
						["D3"] = new PointF( 182f, 840f ),
						["D4"] = new PointF( 536f, 829f ),
						["D5"] = new PointF( 692f, 421f ),
						["D6"] = new PointF( 0877, 708f ),
						["D7"] = new PointF( 1049f, 437f ),
						["D8"] = new PointF( 1235, 181f ),
					};
					break;
			}
			//			boardA = Image.FromFile(".\\images\\board a.png");

			dahan = Image.FromFile(".\\images\\Dahanicon.png");
			city = Image.FromFile(".\\images\\Cityicon.png");
			town = Image.FromFile(".\\images\\Townicon.png");
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
		Image town;
		Image explorer;
		Image presence;
		Image blight;
		Image defend;

		GameState gameState;
		Spirit spirit;

		protected override void OnPaint( PaintEventArgs pe ) {

			using var brush = new SolidBrush(Color.AliceBlue);
			pe.Graphics.FillRectangle(brush,new Rectangle(0,0,400,400));

			if(board != null)
				pe.Graphics.DrawImage( board, 0.0f, 0.0f );

			if(gameState != null)
				foreach(var space in gameState.Island.Boards[0].Spaces)
					DecorateSpace(pe.Graphics,space);

			if(activeSpaces != null)
				CirleActiveSpaces( pe );


			base.OnPaint( pe );
		}

		void CirleActiveSpaces( PaintEventArgs pe ) {
			using var pen = new Pen(Brushes.Aquamarine,5);
			foreach(var space in activeSpaces) {
				var center = spaceLookup[space.Label];
				pe.Graphics.DrawEllipse( pen, center.X, center.Y, radius * 2, radius * 2 );
			}
		}
		
		void DecorateSpace( Graphics graphics, Space space ) {
			if(!spaceLookup.ContainsKey(space.Label)) return; // happens during developement

			var xy = spaceLookup[space.Label];
			float x = xy.X;
			float y = xy.Y;
			float dimension = 40.0f, step = 55.0f;
			// invaders
			var grp = gameState.InvadersOn( space );
			var invaders = grp.InvaderTypesPresent.Select( k => grp[k] + ":" + k.Summary ).Join( " " );
			List<Image> images = new();
			images.AddCount( grp[Invader.City], city );
			images.AddCount( grp[Invader.City2], city );
			images.AddCount( grp[Invader.City1], city );
			images.AddCount( grp[Invader.Town], town );
			images.AddCount( grp[Invader.Town1], town );
			images.AddCount( grp[Invader.Explorer], explorer );
			DrawRow( graphics, x, ref y, dimension, step, images );

			// dahan
			images.Clear();
			images.AddCount( gameState.GetDahanOnSpace( space ), dahan );
			images.AddCount( gameState.GetDefence( space ), defend);
			DrawRow( graphics, x, ref y, dimension, step, images );

			// presence
			images.Clear();
			images.AddCount(spirit.PresenceOn(space), presence);
			DrawRow( graphics, x, ref y, dimension, step, images );

			// blight
			images.Clear();
			images.AddCount(gameState.GetBlightOnSpace( space ), blight);
			DrawRow( graphics, x, ref y, dimension, step, images );
		}

		private static void DrawRow( Graphics graphics, float x, ref float y, float width, float step, List<Image> images ) {
			if(images.Count == 0) return;
			float maxHeight = 0;
			for(int i = 0; i < images.Count; ++i){
				var img = images[i];
				float height = width / img.Width * img.Height;
				maxHeight = Math.Max(maxHeight,height); 
				graphics.DrawImage( img, x + i * step, y, width, height);
			}
			float gap = step-width;
			y += maxHeight + gap;
		}

		protected override void OnClick( EventArgs e ) {

			var mp = this.PointToClient(Control.MousePosition);

			var xx = activeSpaces
				.Select(s=>{
					var tl = this.spaceLookup[s.Label];
					float dx = mp.X-tl.X-radius, dy=mp.Y-tl.Y-radius;
					return new {Space=s,d2=dx*dx+dy*dy};
				})
				.ToArray();

			var match = xx
				.Where(x=>x.d2<radius*radius)
				.OrderBy(x=>x.d2)
				.Select(x=>x.Space)
				.FirstOrDefault();

			if(match != null)
				SpaceClicked?.Invoke(match);
			else
				MessageBox.Show(mp.ToString());

		}

		protected override void OnMouseMove( MouseEventArgs e ) {
			base.OnMouseMove( e );

			var mp = this.PointToClient(Control.MousePosition);
			bool inCircle = activeSpaces
				.Select(s=>{
					var tl = this.spaceLookup[s.Label];
					float dx = mp.X-tl.X-radius, dy=mp.Y-tl.Y-radius;
					return dx*dx+dy*dy;
				})
				.Any(x=>x<radius*radius);
			Cursor = inCircle ? Cursors.Hand : Cursors.Default;

		}

		public event Action<Space> SpaceClicked;

	}

}
