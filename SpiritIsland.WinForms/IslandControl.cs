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

			spaceLookup = new Dictionary<string,PointF>{
				["A0"] = new PointF( 685.0f - 533,584.0f - 109),
				["A1"] = new PointF(1092.0f - 533,305.0f - 109),
				["A2"] = new PointF( 859.0f - 533,556.0f - 109),
				["A3"] = new PointF( 729.0f - 533,867.0f - 109),
				["A4"] = new PointF(1080.0f - 533,742.0f - 109),
				["A5"] = new PointF(1343.0f - 533,576.0f - 109),
				["A6"] = new PointF(1323.0f - 533,299.0f - 109),
				["A7"] = new PointF(1565.0f - 533,640.0f - 109),
				["A8"] = new PointF(1656.0f - 533,235.0f - 109),
			};
		}

		Space[] activeSpaces;

		public void ActivateSpaces(IEnumerable<Space> spaces){
			this.activeSpaces = spaces.ToArray();
		}

		readonly Dictionary<string,PointF> spaceLookup;

		public void InitBoard(GameState gameState){
			boardA = Image.FromFile(".\\images\\board a.png");

			dahan = Image.FromFile(".\\images\\Dahanicon.png");
			city = Image.FromFile(".\\images\\Cityicon.png");
			town = Image.FromFile(".\\images\\Townicon.png");
			explorer = Image.FromFile(".\\images\\Explorericon.png");
			presence = Image.FromFile(".\\images\\Presenceicon.png");
			blight = Image.FromFile(".\\images\\Blighticon.png");

			this.gameState = gameState;
			this.spirit = gameState.Spirits.Single();
		}

		Image boardA;

		Image dahan;
		Image city;
		Image town;
		Image explorer;
		Image presence;
		Image blight;

		GameState gameState;
		Spirit spirit;

		protected override void OnPaint( PaintEventArgs pe ) {

			using var brush = new SolidBrush(Color.AliceBlue);
			pe.Graphics.FillRectangle(brush,new Rectangle(0,0,400,400));

			if(boardA != null)
				pe.Graphics.DrawImage(boardA,0.0f,0.0f);

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
