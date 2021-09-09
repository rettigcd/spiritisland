using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	class GrowthPainter {

		readonly Graphics graphics;

		public GrowthPainter(Graphics graphics) {
			this.graphics = graphics;
		}

		public GrowthLayout layout;

		public float Paint(GrowthOption[] options, float x, float y, float width ) {
			layout = new GrowthLayout(options);
			layout.ScaleToWidth(width);
			layout.Translate(x,y);

			using var optionPen = new Pen( Color.Blue, 6f );

			// Growth Options
			foreach(var (_, rect) in layout.EachGrowth().Skip(1)) {
				graphics.DrawLine(optionPen,rect.Left,rect.Top,rect.Left,rect.Bottom);
			}

			// Actions
			foreach(var (action,rect) in layout.EachAction())
				DrawAction( action, rect );

			return layout.Size.Height;
		}

		private void DrawAction( GrowthActionFactory action, RectangleF rect ) {

			switch(action.Text) {
				case "ReclaimAll": ReclaimAll( rect ); break;
				case "Reclaim(1)": Reclaim1( rect ); break;

				case "DrawPowerCard": DrawPowerCard( rect ); break;
				case "GainEnergy(1)": GainEnergy( rect, 1 ); break;
				case "GainEnergy(2)": GainEnergy( rect, 2 ); break;
				case "GainEnergy(3)": GainEnergy( rect, 3 ); break;
				case "GainEnergy(4)": GainEnergy( rect, 4 ); break;
				case "PlacePresence(0)": PlacePresence( rect, 0 ); break;
				case "PlacePresence(1)": PlacePresence( rect, 1 ); break;
				case "PlacePresence(2)": PlacePresence( rect, 2 ); break;
				case "PlacePresence(3)": PlacePresence( rect, 3 ); break;
				// thunderspeaker
				case "PlacePresence(1,dahan)": PlacePresence( rect, 1, "dahan" ); break;
				case "PlacePresence(2,dahan)": PlacePresence( rect, 2, "dahan" ); break;
				// rampant green
				case "PlacePresence(2,W / J)": PlacePresence( rect, 2, "j/w" ); break;
				case "PlayExtraCardThisTurn": AdditionalPlay( rect ); break;
				// bringger
				case "PlacePresence(4,dahan or invaders)": PlacePresence( rect, 4 ); break; // !!! missing dahan or invaders
				// ocean
				case "PlacePresence(1,coatal)": PlacePresence( rect, 1 ); break; //            !!! missing costal
				case "GatherPresenceIntoOcean": GatherToOcean(rect); break;
				case "PlaceInOcean": PlaceInOcean( rect ); break;
				case "PushPresenceFromOcean": PushFromOcean( rect ); break;
				// Keeper
				case "PlacePresence(3,presence or wilds)": PlacePresence( rect, 3); break; //  !!!
				case "GainEnergy(-3)": //                                                      !!!
				case "PlacePresence(3,no blight)": //                                          !!!
				// Sharp Fangs
				case "GainEnergy(-1)":
				case "PlacePresence(3,beast or jungle)":
					graphics.FillRectangle( Brushes.SeaGreen, Rectangle.Inflate( rect.ToInts(), -5, -5 ) );
					break;
				default:
					graphics.FillRectangle( Brushes.Goldenrod, Rectangle.Inflate( rect.ToInts(), -5, -5 ) );
					break;
			}

		}

		void PushFromOcean( RectangleF rect ) => DrawTokenInCenter(rect, "Pushfromocean");
		void PlaceInOcean( RectangleF rect ) => DrawTokenInCenter( rect, "Ocean" );
		void AdditionalPlay( RectangleF rect )         => DrawTokenInCenter( rect, "Cardplayplusone");
		void Reclaim1( RectangleF rect )               => DrawTokenInCenter( rect, "reclaim 1");
		void DrawPowerCard( RectangleF rect )          => DrawTokenInCenter( rect, "GainCard" );
		void GatherToOcean( RectangleF rect )          => DrawTokenInCenter( rect, "Gathertoocean" );
		void ReclaimAll( RectangleF rect )             => DrawTokenInCenter( rect, "ReclaimAll" );
		void GainEnergy( RectangleF rect, int delta )  => DrawTokenInCenter( rect, "Energy_Plus_"+delta);

		void DrawTokenInCenter( RectangleF rect, string file ) {
			var img = ResourceImages.Singleton.GetTokenIcon( file );
			float imgWidth = rect.Width, imgHeight = img.Height * imgWidth / img.Width;
			graphics.DrawImage( img, rect.X, rect.Y + (rect.Height - imgHeight) / 2, imgWidth, imgHeight );
		}



		void PlacePresence( RectangleF rect, int range, string icon = "" ) {

			var font = SystemFonts.IconTitleFont;

			// + presence
			float presencePercent = icon == "" ? .3f : .2f;
			float plusY = rect.Y + rect.Height * presencePercent; // top of presence
			graphics.DrawString("+",font,Brushes.Black,rect.X+rect.Width*0.25f,plusY);
			using var presenceIcon = ResourceImages.Singleton.GetBlackIcon( "Presenceicon" );
			graphics.DrawImage(presenceIcon, rect.X + rect.Width * 0.4f, plusY-rect.Height*.1f, rect.Width*.5f, rect.Height*.2f );

			// icon
			if(icon != "") {
				string filename = icon switch {
					"dahan" => ".\\images\\Dahanicon.png",
					"j/w" => ".\\images\\Junglewetland.png",
					_ => "(error)"
				};
				using var image = Image.FromFile( filename );
				float iconPercentage = .4f;
				float iconHeight = rect.Height * .3f;
				float iconWidth = iconHeight * image.Width / image.Height;
				graphics.DrawImage( image, 
					rect.X + (rect.Width - iconWidth)/2, 
					rect.Y + rect.Height * iconPercentage,
					iconWidth,
					iconHeight
				);
			}

			// range # text
			float rangeTextTop = rect.Y + rect.Height * .7f;
			string txt = range.ToString();
			SizeF rangeTextSize = graphics.MeasureString(txt,font);
			graphics.DrawString(txt,font,Brushes.Black,rect.X+(rect.Width-rangeTextSize.Width)/2,rangeTextTop);

			// range arrow
			float rangeArrowTop = rect.Y + rect.Height * .85f;
			using var rangeIcon = ResourceImages.Singleton.GetTokenIcon( "Range" );
			float arrowWidth = rect.Width * .8f, arrowHeight = arrowWidth * rangeIcon.Height / rangeIcon.Width;
			graphics.DrawImage( rangeIcon, rect.X + (rect.Width-arrowWidth)/2, rangeArrowTop, arrowWidth, arrowHeight );

			// graphics.FillRectangle( Brushes.Goldenrod, Rectangle.Inflate( rect.ToInts(), -5, -5 ) );

		}
	}
}
