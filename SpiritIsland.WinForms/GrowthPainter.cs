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
			if(action is GainEnergy ge) { GainEnergy( rect, ge.Delta ); return; }

			if(action is ReclaimAll) { ReclaimAll( rect ); return; }

			if(action is Reclaim1) { Reclaim1( rect ); return; }

			if(action is DrawPowerCard) { DrawPowerCard( rect ); return; }

			if(action is PlacePresence pp ) {
				PlacePresence( rect, pp.Range, pp.FilterEnum );
				return;
			}

			switch(action.Name) {

				case "PlayExtraCardThisTurn": AdditionalPlay( rect ); break;
				// Ocean
				case "PlaceInOcean":          PlacePresence( rect, null, Target.Ocean ); break;
				case "GatherPresenceIntoOcean": GatherToOcean(rect); break;
				case "PushPresenceFromOcean": PushFromOcean( rect ); break;
				// Heart of the WildFire
				case "EnergyForFire": EnergyForFire( rect ); break;
				// Serpent
				case "MovePresence": MovePresence( rect, 1 ); break;
				// Lure of the Deep Wilderness
				case "GainElement(Moon,Air,Plant)": GainElement( rect, Element.Moon, Element.Air, Element.Plant ); break;
				// Grinning Trickster
				case "GainEnergyEqualToCardPlays": DrawIconInCenter( rect, "GainEnergyEqualToCardPlays"); break;
				// Stones Unyielding Defiance
				case "GainElements(Earth,Earth)": GainElement( rect, Element.Earth, Element.Earth ); break; // !!! this is drawn as an OR, layer them and make them an AND
				// Many Minds
				case "Gather1Beast": LandGatherBeasts( rect ); break;
				case "PlacePresenceAndBeast": 
					DrawIconInCenter( rect.InflateBy(-rect.Width*.2f), "Beasticon");
					break;
				default:
					graphics.FillRectangle( Brushes.Goldenrod, Rectangle.Inflate( rect.ToInts(), -5, -5 ) );
					break;
			}

		}

		void PushFromOcean( RectangleF rect )          => DrawIconInCenter(rect, "Pushfromocean");

		void AdditionalPlay( RectangleF rect )         => DrawIconInCenter( rect, "Cardplayplusone");
		void Reclaim1( RectangleF rect )               => DrawIconInCenter( rect, "reclaim 1");
		void DrawPowerCard( RectangleF rect )          => DrawIconInCenter( rect, "GainCard" );
		void GatherToOcean( RectangleF rect )          => DrawIconInCenter( rect, "Gathertoocean" );
		void ReclaimAll( RectangleF rect )             => DrawIconInCenter( rect, "ReclaimAll" );
		void EnergyForFire( RectangleF rect )          => DrawIconInCenter( rect, "Oneenergyfire");

		void LandGatherBeasts( RectangleF rect)						=> DrawIconInCenter( rect, "Land_Gather_Beasts");

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

		void DrawIconInCenter( RectangleF rect, string file ) {
			var img = ResourceImages.Singleton.GetIcon( file );
			float imgWidth = rect.Width, imgHeight = img.Height * imgWidth / img.Width;
			graphics.DrawImage( img, rect.X, rect.Y + (rect.Height - imgHeight) / 2, imgWidth, imgHeight );
		}

		void GainElement( RectangleF rect, params Element[] elements ) {
			var parts = rect.SplitHorizontally(elements.Length);
			for(int i = 0; i < elements.Length; ++i) {
				using var img = ResourceImages.Singleton.GetToken(elements[i]);
				graphics.DrawImageFitWidth(img, parts[i]);
			}
		}


		void PlacePresence( RectangleF rect, int? range, string filterEnum ) {
			using var image = ResourceImages.Singleton.GetTargetFilterIcon( filterEnum );

			using Font font = new Font( ResourceImages.Singleton.Fonts.Families[0], rect.Height * .15f, GraphicsUnit.Pixel  );

			// + presence
			float presencePercent = image == null ? .3f : .2f;
			float plusY = rect.Y + rect.Height * presencePercent; // top of presence
			graphics.DrawString("+",font,Brushes.Black,rect.X+rect.Width*0.25f,plusY);
			using var presenceIcon = ResourceImages.Singleton.GetIcon( "Presenceicon" );
			graphics.DrawImage(presenceIcon, rect.X + rect.Width * 0.4f, plusY-rect.Height*.1f, rect.Width*.5f, rect.Height*.2f );

			// icon
			if(image != null) {
				// using var image = Image.FromFile( ".\\images\\" + iconFilename + ".png" );
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

			if(range.HasValue) { // no range for ocean
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
