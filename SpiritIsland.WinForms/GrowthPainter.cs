using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	class GrowthPainter {

		readonly Graphics graphics;

		public GrowthPainter( Graphics graphics ) {
			this.graphics = graphics;
		}

		public void Paint(GrowthLayout layout, IList<GrowthOption> clickableGrowthOptions, IList<GrowthActionFactory> clickableGrowthActions ) {

			using var optionPen = new Pen( Color.Blue, 6f );
			using var highlightPen = new Pen( Color.Red, 4f );

			// Growth Options
			bool first = true;
			foreach(var (opt, rect) in layout.EachGrowth()) {
				// Divider
				if(first) first = false; else graphics.DrawLine(optionPen,rect.Left,rect.Top,rect.Left,rect.Bottom);
				// Active / Clickable
				if(clickableGrowthOptions.Contains( opt ))
					graphics.DrawRectangle( highlightPen, rect.ToInts() );
			}

			// Growth Actions
			foreach(var (action,rect) in layout.EachAction()) {
				DrawAction( action, rect );
				// Active / Clickable
				if(clickableGrowthActions.Contains( action ))
					graphics.DrawRectangle( highlightPen, rect.ToInts() );
			}

		}

		void DrawAction( GrowthActionFactory action, RectangleF rect ) {
			if(action is GainEnergy ge) { GainEnergy( rect, ge.Delta ); return; }

			if(action is ReclaimAll) { ReclaimAll( rect ); return; }

			if(action is Reclaim1) { Reclaim1( rect ); return; }

			if(action is ReclaimHalf) {  ReclaimHalf( rect ); return; }

			if(action is DrawPowerCard) { DrawPowerCard( rect ); return; }

			if(action is PlacePresence pp ) { PlacePresence( rect, pp.Range, pp.FilterEnum ); return; }

			if(action is MovePresence mp) { MovePresence( rect, mp.Range ); return; }

			switch(action.Name) {

				case "PlayExtraCardThisTurn": AdditionalPlay( rect ); break;
				// Ocean
				case "PlaceInOcean":          PlacePresence( rect, null, Target.Ocean ); break;
				case "GatherPresenceIntoOcean": GatherToOcean(rect); break;
				case "PushPresenceFromOcean": PushFromOcean( rect ); break;
				// Heart of the WildFire
				case "EnergyForFire": EnergyForFire( rect ); break;
				// Lure of the Deep Wilderness
				case "GainElement(Moon,Air,Plant)": GainElement( rect, Element.Moon, Element.Air, Element.Plant ); break;
				// Grinning Trickster
				case "GainEnergyEqualToCardPlays": DrawIconInCenter( rect, Img.GainEnergyEqualToCardPlays ); break;
				// Stones Unyielding Defiance
				case "GainElements(Earth,Earth)": GainElement( rect, Element.Earth, Element.Earth ); break; // !!! this is drawn as an OR, layer them and make them an AND
				// Many Minds
				case "Gather1Beast": LandGatherBeasts( rect ); break;
				case "PlacePresenceAndBeast": 
					PlacePresence( rect, 3, Target.Any );
					DrawIconInCenter( rect.InflateBy(-rect.Width*.2f), Img.Icon_Beast );
					break;
				case "ApplyDamage": ApplyDamage( rect ); break;
				default:
					graphics.FillRectangle( Brushes.Goldenrod, Rectangle.Inflate( rect.ToInts(), -5, -5 ) );
					break;
			}

		}

		void AdditionalPlay( RectangleF rect )      => DrawIconInCenter( rect, Img.Plus1CardPlay );
		void Reclaim1( RectangleF rect )            => DrawIconInCenter( rect, Img.Reclaim1 );
		void ReclaimHalf( RectangleF rect )         => DrawIconInCenter( rect, Img.ReclaimHalf );
		void ReclaimAll( RectangleF rect )          => DrawIconInCenter( rect, Img.ReclaimAll );
		void DrawPowerCard( RectangleF rect )       => DrawIconInCenter( rect, Img.GainCard );
		void PushFromOcean( RectangleF rect )       => DrawIconInCenter(rect, Img.Pushfromocean );
		void ApplyDamage( RectangleF rect )			=> DrawIconInCenter( rect, Img.Damage_2 );
		void GatherToOcean( RectangleF rect )       => DrawIconInCenter( rect, Img.Gathertoocean );
		void EnergyForFire( RectangleF rect )       => DrawIconInCenter( rect, Img.Oneenergyfire );
		void LandGatherBeasts( RectangleF rect)		=> DrawIconInCenter( rect, Img.Land_Gather_Beasts );

		void GainEnergy( RectangleF bounds, int delta ){
			// DrawTokenInCenter( rect, "Energy_Plus_"+delta);
			using var img = ResourceImages.Singleton.GetImage( Img.Coin );
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

		}

		void DrawIconInCenter( RectangleF rect, Img img ) {
			var image = ResourceImages.Singleton.GetImage( img );
			float imgWidth = rect.Width, imgHeight = image.Height * imgWidth / image.Width;
			graphics.DrawImage( image, rect.X, rect.Y + (rect.Height - imgHeight) / 2, imgWidth, imgHeight );
		}

		void GainElement( RectangleF rect, params Element[] elements ) {
			var parts = rect.SplitHorizontally(elements.Length);
			for(int i = 0; i < elements.Length; ++i) {
				using var img = ResourceImages.Singleton.GetImage( elements[i].GetTokenImg() );
				graphics.DrawImageFitWidth(img, parts[i]);
			}
		}


		static Bitmap GetTargetFilterIcon( string filterEnum ) {
			// !!! Move the filterEnum closer to where the filter is defined, not here.
			Img img = filterEnum switch {
				Target.Dahan              => Img.Icon_Dahan,
				Target.JungleOrWetland    => Img.Icon_JungleOrWetland,
				Target.DahanOrInvaders    => Img.Icon_DahanOrInvaders,
				Target.Coastal            => Img.Icon_Coastal,
				Target.PresenceOrWilds    => Img.Icon_PresenceOrWilds,
				Target.NoBlight           => Img.Icon_NoBlight,
				Target.BeastOrJungle      => Img.Icon_BeastOrJungle,
				Target.Ocean              => Img.Icon_Ocean,
				Target.MountainOrPresence => Img.Icon_MountainOrPresence,
				Target.TownCityOrBlight   => Img.Icon_TownCityOrBlight,
				_                         => Img.None, // Inland, Any
			};
			return img != default ? ResourceImages.Singleton.GetImage(img) : null;
		}


		void MovePresence( RectangleF rect, int range ) {

			using Font font = new Font( ResourceImages.Singleton.Fonts.Families[0], rect.Height * .25f, GraphicsUnit.Pixel  );
			using var presenceIcon = ResourceImages.Singleton.GetImage( Img.Icon_Presence );

			// + presence
			float iconCenterY = rect.Y + rect.Height *.3f; // top of presence
			float presenceWidth = rect.Width*.6f;
			float presenceHeight = presenceIcon.Height * presenceWidth / presenceIcon.Width;
			graphics.DrawImage(presenceIcon, rect.X + (rect.Width-presenceWidth)/2, iconCenterY-presenceHeight*.5f, presenceWidth, presenceHeight );

			// range # text
			float rangeTextTop = rect.Y + rect.Height * .55f;
			string txt = range.ToString();
			SizeF rangeTextSize = graphics.MeasureString(txt,font);
			graphics.DrawString(txt,font,Brushes.Black,rect.X+(rect.Width-rangeTextSize.Width)/2,rangeTextTop);

			// range arrow
			float rangeArrowTop = rect.Y + rect.Height * .85f;
			using var rangeIcon = ResourceImages.Singleton.GetImage( Img.MoveArrow );
			float arrowWidth = rect.Width * .8f, arrowHeight = arrowWidth * rangeIcon.Height / rangeIcon.Width;
			graphics.DrawImage( rangeIcon, rect.X + (rect.Width-arrowWidth)/2, rangeArrowTop, arrowWidth, arrowHeight );

		}

		void PlacePresence( RectangleF rect, int? range, string filterEnum ) {
			using var presenceIcon = ResourceImages.Singleton.GetImage( Img.Icon_Presence );
			using var image = GetTargetFilterIcon( filterEnum );

			float fontScale       = image == null ? .25f : .15f;
			float presenceYPercent = image == null ? .3f  : .2f;
			float textTopScale    = image == null ? .55f : .7f;

			using Font font = new Font( ResourceImages.Singleton.Fonts.Families[0], rect.Height * fontScale, GraphicsUnit.Pixel  );

			// + presence
			float iconCenterY = rect.Y + rect.Height * presenceYPercent; // top of presence
			float presenceWidth = rect.Width*.6f;
			float presenceHeight = presenceIcon.Height * presenceWidth / presenceIcon.Width;
			float presenceX = rect.X + (rect.Width-presenceWidth)/2 + rect.Width*.1f;

			graphics.DrawString( "+", font, Brushes.Black, presenceX - rect.Width*.3f, iconCenterY-rect.Height*fontScale*.5f );
			graphics.DrawImage(presenceIcon, presenceX, iconCenterY-presenceHeight*.5f, presenceWidth, presenceHeight );


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
				float rangeTextTop = rect.Y + rect.Height * textTopScale;
				string txt = range.Value.ToString();
				SizeF rangeTextSize = graphics.MeasureString(txt,font);
				graphics.DrawString(txt,font,Brushes.Black, rect.X+(rect.Width-rangeTextSize.Width)/2, rangeTextTop);

				// range arrow
				float rangeArrowTop = rect.Y + rect.Height * .85f;
				using var rangeIcon = ResourceImages.Singleton.GetImage( Img.RangeArrow );
				float arrowWidth = rect.Width * .8f, arrowHeight = arrowWidth * rangeIcon.Height / rangeIcon.Width;
				graphics.DrawImage( rangeIcon, rect.X + (rect.Width-arrowWidth)/2, rangeArrowTop, arrowWidth, arrowHeight );
			}

		}

	}

}
