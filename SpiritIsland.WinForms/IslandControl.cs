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

		public IslandControl() {
			InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint 
				| ControlStyles.UserPaint 
				| ControlStyles.OptimizedDoubleBuffer 
				| ControlStyles.ResizeRedraw, true
			);

			spaceLookup = new Dictionary<string,PointF>{
				["A0"] = new PointF( 685.0f,584.0f),
				["A1"] = new PointF(1092.0f,305.0f),
				["A2"] = new PointF( 859.0f,556.0f),
				["A3"] = new PointF( 729.0f,867.0f),
				["A4"] = new PointF(1080.0f,742.0f),
				["A5"] = new PointF(1343.0f,576.0f),
				["A6"] = new PointF(1323.0f,299.0f),
				["A7"] = new PointF(1565.0f,640.0f),
				["A8"] = new PointF(1656.0f,235.0f),
			};
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

			base.OnPaint( pe );
		}

		void DecorateSpace( Graphics graphics, Space space ) {
			var xy = spaceLookup[space.Label];
			float x = xy.X - 533;
			float y = xy.Y - 109;
			float dimension = 40.0f, step = 55.0f;
			// invaders
			var grp = gameState.InvadersOn(space);
			var invaders = grp.InvaderTypesPresent.Select(k=>grp[k]+":"+k.Summary).Join(" ");
			if(invaders.Length>0){
				float tempx = x;
				for(int i=0;i<grp[Invader.City];++i){
					graphics.DrawImage(city,tempx,y,dimension,dimension);
					tempx += step;
				}
				for(int i=0;i<grp[Invader.Town];++i){
					graphics.DrawImage(town,tempx,y,dimension,dimension);
					tempx += step;
				}
				for(int i=0;i<grp[Invader.Explorer];++i){
					graphics.DrawImage(explorer,tempx,y,dimension,dimension);
					tempx += step;
				}
				// !!! missing damaged towns and cities
				y+=step;
			}
			// dahan
			int dahanCount = gameState.GetDahanOnSpace(space);
			if(dahanCount>0){
				float tempx = x;
				while(dahanCount-->0){
					graphics.DrawImage(dahan,tempx,y,dimension,dimension);
					tempx += step;
				}
				y+=step;
			}
			// presence
			int presenceCount = spirit.Presence.Count(p=>p==space);
			if(presenceCount>0){
				float tempx = x;
				while(presenceCount-->0){
					graphics.DrawImage(presence,tempx,y,dimension,dimension);
					tempx += step;
				}
				y+=step;
			}
			// blight
			int blightCount = gameState.GetBlightOnSpace(space);
			if(presenceCount>0){
				float tempx = x;
				while(presenceCount-->0){
					graphics.DrawImage(blight,tempx,y,dimension,dimension);
					tempx += step;
				}
				y+=step;
			}

		}

		protected override void OnClick( EventArgs e ) {
			base.OnClick( e );
			MessageBox.Show($"{MousePosition.X},{MousePosition.Y}","Mouse Position");
		}

	}

}
