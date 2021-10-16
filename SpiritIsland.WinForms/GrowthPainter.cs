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
				case "GainEnergy(-3)": GainEnergy( rect, -3 ); break;
				case "GainEnergy(-1)": GainEnergy( rect, -1 ); break;

				case "PlacePresence(0)": PlacePresence( rect, 0 ); break;
				case "PlacePresence(1)": PlacePresence( rect, 1 ); break;
				case "PlacePresence(2)": PlacePresence( rect, 2 ); break;
				case "PlacePresence(3)": PlacePresence( rect, 3 ); break;
				// thunderspeaker
				case "PlacePresence(1,dahan)": PlacePresence( rect, 1, "Dahanicon" ); break;
				case "PlacePresence(2,dahan)": PlacePresence( rect, 2, "Dahanicon" ); break;
				// rampant green
				case "PlacePresence(2,W / J)": PlacePresence( rect, 2, "Junglewetland" ); break;
				case "PlayExtraCardThisTurn": AdditionalPlay( rect ); break;

				// bringger
				case "PlacePresence(4,dahan or invaders)": PlacePresence( rect, 4, "DahanOrInvaders" ); break;

				// ocean
				case "PlacePresence(1,coastal)": PlacePresence( rect, 1, "Coastal" ); break;
				case "GatherPresenceIntoOcean": GatherToOcean(rect); break;
				case "PlaceInOcean": PlaceInOcean( rect ); break;
				case "PushPresenceFromOcean": PushFromOcean( rect ); break;
				// Keeper
				case "PlacePresence(3,presence or wilds)": PlacePresence( rect, 3, "wildsorpresence"); break;
				case "PlacePresence(3,no blight)": PlacePresence( rect, 3, "Noblight" ); break;
				// Sharp Fangs
				case "PlacePresence(3,beast or jungle)": PlacePresence( rect, 3, "JungleOrBeast" ); break;
				// Heart of the WildFire
				case "EnergyForFire": EnergyForFire( rect ); break;
				// Serpent
				case "MovePresence": MovePresence( rect, 1 ); break;
				// Lure of the Deep Wilderness
				case "PlacePresence(4,Inland)": PlacePresence( rect, 4 ); break;
				case "GainElement(Moon,Air,Plant)": GainElement( rect, Element.Moon, Element.Air, Element.Plant ); break;
				default:
					graphics.FillRectangle( Brushes.Goldenrod, Rectangle.Inflate( rect.ToInts(), -5, -5 ) );
					break;
			}

		}

		void PushFromOcean( RectangleF rect )          => DrawTokenInCenter(rect, "Pushfromocean");

		void AdditionalPlay( RectangleF rect )         => DrawTokenInCenter( rect, "Cardplayplusone");
		void Reclaim1( RectangleF rect )               => DrawTokenInCenter( rect, "reclaim 1");
		void DrawPowerCard( RectangleF rect )          => DrawTokenInCenter( rect, "GainCard" );
		void GatherToOcean( RectangleF rect )          => DrawTokenInCenter( rect, "Gathertoocean" );
		void ReclaimAll( RectangleF rect )             => DrawTokenInCenter( rect, "ReclaimAll" );
		void EnergyForFire( RectangleF rect )          => DrawTokenInCenter( rect, "Oneenergyfire");

		void GainEnergy( RectangleF bounds, int delta ){
			// DrawTokenInCenter( rect, "Energy_Plus_"+delta);
			using var img = ResourceImages.Singleton.GetToken( "coin" );
			float imgWidth = bounds.Width, imgHeight = img.Height * imgWidth / img.Width; // assuming width limited
			graphics.DrawImage( img, bounds.X, bounds.Y + (bounds.Height - imgHeight) / 2, imgWidth, imgHeight );

			using Font coinFont = new Font( ResourceImages.Singleton.Fonts.Families[0], imgHeight * .5f, GraphicsUnit.Pixel );
			string txt = delta > 0 
				? ("+" + delta.ToString())
				: ("\u2014" + (-delta).ToString());
			SizeF textSize = graphics.MeasureString( txt, coinFont );
			PointF textTopLeft = new PointF(
				bounds.X + (bounds.Width - textSize.Width) * .35f,
				bounds.Y + (bounds.Height - textSize.Height) * .60f
			);
			graphics.DrawString( txt, coinFont, Brushes.Black, textTopLeft );
			// graphics.DrawRectangle( Pens.Red, textTopLeft.X, textTopLeft.Y, textSize.Width, textSize.Height );

		}

		void DrawTokenInCenter( RectangleF rect, string file ) {
			var img = ResourceImages.Singleton.GetIcon( file );
			float imgWidth = rect.Width, imgHeight = img.Height * imgWidth / img.Width;
			graphics.DrawImage( img, rect.X, rect.Y + (rect.Height - imgHeight) / 2, imgWidth, imgHeight );
		}

		void PlaceInOcean( RectangleF rect ) {
			PlacePresence( rect, null, "Ocean" );
		}

		void GainElement( RectangleF rect, params Element[] elements ) {
			var parts = rect.SplitHorizontally(elements.Length);
			for(int i = 0; i < elements.Length; ++i) {
				using var img = ResourceImages.Singleton.GetToken(elements[i]);
				graphics.DrawImageFitWidth(img, parts[i]);
			}
		}


		void PlacePresence( RectangleF rect, int? range, string iconFilename = "" ) {

			using Font font = new Font( ResourceImages.Singleton.Fonts.Families[0], rect.Height * .15f, GraphicsUnit.Pixel  );
			// var font = SystemFonts.IconTitleFont;

			// + presence
			float presencePercent = iconFilename == "" ? .3f : .2f;
			float plusY = rect.Y + rect.Height * presencePercent; // top of presence
			graphics.DrawString("+",font,Brushes.Black,rect.X+rect.Width*0.25f,plusY);
			using var presenceIcon = ResourceImages.Singleton.GetIcon( "Presenceicon" );
			graphics.DrawImage(presenceIcon, rect.X + rect.Width * 0.4f, plusY-rect.Height*.1f, rect.Width*.5f, rect.Height*.2f );

			// icon
			if(iconFilename != "") {
				// using var image = Image.FromFile( ".\\images\\" + iconFilename + ".png" );
				using var image = ResourceImages.Singleton.GetIcon( iconFilename );
				float iconPercentage = .4f;
				float iconHeight = rect.Height * .3f;
				float iconWidth = iconHeight * image.Width / image.Height;

				if(iconWidth > rect.Width) { // too wide, switch scaling to width limited
					iconWidth = rect.Width;
					iconHeight = iconWidth * image.Height / image.Width;
				}

				graphics.DrawImage( image, 
					rect.X + (rect.Width - iconWidth)/2, 
					rect.Y + rect.Height * iconPercentage,
					iconWidth,
					iconHeight
				);
			}

			if(range.HasValue) {
				// range # text
				float rangeTextTop = rect.Y + rect.Height * .65f;
				string txt = range.Value.ToString();
				SizeF rangeTextSize = graphics.MeasureString(txt,font);
				graphics.DrawString(txt,font,Brushes.Black,rect.X+(rect.Width-rangeTextSize.Width)/2,rangeTextTop);

				// range arrow
				float rangeArrowTop = rect.Y + rect.Height * .85f;
				using var rangeIcon = ResourceImages.Singleton.GetIcon( "Range" );
				float arrowWidth = rect.Width * .8f, arrowHeight = arrowWidth * rangeIcon.Height / rangeIcon.Width;
				graphics.DrawImage( rangeIcon, rect.X + (rect.Width-arrowWidth)/2, rangeArrowTop, arrowWidth, arrowHeight );
			}

		}

		void MovePresence( RectangleF rect, int range, string iconFilename = "" ) {

			using Font font = new Font( ResourceImages.Singleton.Fonts.Families[0], rect.Height * .15f, GraphicsUnit.Pixel  );
			// var font = SystemFonts.IconTitleFont;

			// + presence
			float presencePercent = iconFilename == "" ? .3f : .2f;
			float plusY = rect.Y + rect.Height * presencePercent; // top of presence
			graphics.DrawString("+",font,Brushes.Black,rect.X+rect.Width*0.25f,plusY);
			using var presenceIcon = ResourceImages.Singleton.GetIcon( "Presenceicon" );
			graphics.DrawImage(presenceIcon, rect.X + rect.Width * 0.4f, plusY-rect.Height*.1f, rect.Width*.5f, rect.Height*.2f );

			// icon
			if(iconFilename != "") {
				// using var image = Image.FromFile( ".\\images\\" + iconFilename + ".png" );
				using var image = ResourceImages.Singleton.GetIcon( iconFilename );
				float iconPercentage = .4f;
				float iconHeight = rect.Height * .3f;
				float iconWidth = iconHeight * image.Width / image.Height;

				if(iconWidth > rect.Width) { // too wide, switch scaling to width limited
					iconWidth = rect.Width;
					iconHeight = iconWidth * image.Height / image.Width;
				}

				graphics.DrawImage( image, 
					rect.X + (rect.Width - iconWidth)/2, 
					rect.Y + rect.Height * iconPercentage,
					iconWidth,
					iconHeight
				);
			}

			// range # text
			float rangeTextTop = rect.Y + rect.Height * .65f;
			string txt = range.ToString();
			SizeF rangeTextSize = graphics.MeasureString(txt,font);
			graphics.DrawString(txt,font,Brushes.Black,rect.X+(rect.Width-rangeTextSize.Width)/2,rangeTextTop);

			// range arrow
			float rangeArrowTop = rect.Y + rect.Height * .85f;
			using var rangeIcon = ResourceImages.Singleton.GetIcon( "Moveicon" );
			float arrowWidth = rect.Width * .8f, arrowHeight = arrowWidth * rangeIcon.Height / rangeIcon.Width;
			graphics.DrawImage( rangeIcon, rect.X + (rect.Width-arrowWidth)/2, rangeArrowTop, arrowWidth, arrowHeight );

		}


	}
}
