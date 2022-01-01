using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	class GrowthPainter : IDisposable{

		readonly GrowthLayout layout;

		Graphics graphics; // single-threaded variables
		Bitmap cachedImageLayer;

		public GrowthPainter( GrowthLayout layout ) {
			this.layout = layout;
		}

		public void Paint( Graphics graphics, IList<GrowthOption> clickableGrowthOptions, IList<GrowthActionFactory> clickableGrowthActions ) {
			this.graphics = graphics;

			using var optionPen = new Pen( Color.Blue, 6f );
			using var highlightPen = new Pen( Color.Red, 4f );

			if(cachedImageLayer == null) {

				cachedImageLayer = new Bitmap( layout.Bounds.Width, layout.Bounds.Height );
				using var g = Graphics.FromImage( cachedImageLayer );
				g.TranslateTransform( -layout.Bounds.X, -layout.Bounds.Y );
				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

				// Growth - Dividers
				bool first = true;
				foreach(var (opt, rect) in layout.EachGrowth())
					if(first)
						first = false;
					else
						g.DrawLine( optionPen, rect.Left, rect.Top, rect.Left, rect.Bottom );

				this.graphics = g;

				// Growth Actions
				foreach(var (action, rect) in layout.EachAction())
					DrawAction( action, rect );

			}
			graphics.DrawImage( cachedImageLayer, layout.Bounds );


			DrawHotspots( graphics, clickableGrowthOptions, clickableGrowthActions, highlightPen );

		}

		private void DrawHotspots( Graphics graphics, IList<GrowthOption> clickableGrowthOptions, IList<GrowthActionFactory> clickableGrowthActions, Pen highlightPen ) {
			// Growth Options
			foreach(var (opt, rect) in layout.EachGrowth())
				if(clickableGrowthOptions.Contains( opt ))
					graphics.DrawRectangle( highlightPen, rect.ToInts() );

			// Growth Actions
			foreach(var (action, rect) in layout.EachAction())
				if(clickableGrowthActions.Contains( action ))
					graphics.DrawRectangle( highlightPen, rect.ToInts() );
		}

		void DrawAction( GrowthActionFactory action, RectangleF rect ) {

			if(action is JaggedEarth.RepeatableActionFactory raf && raf.Inner is not JaggedEarth.GainTime )
				action = raf.Inner;

			if(action is GainEnergy ge) { GainEnergy( rect, ge.Delta ); return; }

			if(action is ReclaimAll) { ReclaimAll( rect ); return; }

			if(action is Reclaim1) { Reclaim1( rect ); return; }

			if(action is ReclaimHalf) {  ReclaimHalf( rect ); return; }

			if(action is DrawPowerCard) { DrawPowerCard( rect ); return; }

			if(action is PlacePresence pp ) { PlacePresence( rect, pp.Range, pp.FilterEnum ); return; }

			if(action is MovePresence mp) { MovePresence( rect, mp.Range ); return; }

			switch(action.Name) {

				case "PlayExtraCardThisTurn(2)":
				case "PlayExtraCardThisTurn(1)":
													AdditionalPlay( rect ); break;
				// Ocean
				case "PlaceInOcean":          PlacePresence( rect, null, Target.Ocean ); break;
				case "GatherPresenceIntoOcean": GatherToOcean(rect); break;
				case "PushPresenceFromOcean": PushFromOcean( rect ); break;
				// Heart of the WildFire
				case "EnergyForFire": EnergyForFire( rect ); break;
				// Lure of the Deep Wilderness
				case "GainElement(Moon,Air,Plant)": GainElement( rect, Element.Moon, Element.Air, Element.Plant ); break;
				// Fractured Dates
				case "GainElement(Air)": GainElement( rect, Element.Air ); break;
				case "GainElement(Moon)": GainElement( rect, Element.Moon ); break;
				case "GainElement(Sun)": GainElement( rect, Element.Sun ); break;
				case "GainTime(2)":    GainTime( rect ); break;
				case "GainTime(1)x2":  Gain1TimeOr2CardPlaysX2( rect ); break;
				case "GainTime(1)x3":  Gain1TimeOr2EnergyX3( rect ); break;
				case "DrawPowerCardFromDaysThatNeverWere": DrawImage( rect, Img.FracturedDays_DrawDtnw ); break; 
				// Grinning Trickster
				case "GainEnergyEqualToCardPlays": DrawIconInCenter( rect, Img.GainEnergyEqualToCardPlays ); break;
				// Stones Unyielding Defiance
				case "GainElements(Earth,Earth)": 
					GainElement( rect, Element.Earth, Element.Earth ); 
					break; // !!! this is drawn as an OR, layer them and make them an AND
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

		void AdditionalPlay( RectangleF rect )      => DrawIconInCenter( rect, Img.Plus1CardPlay ); // !!!!!!!! show the correct # of plays (Fractured Day...)
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

			graphics.DrawImageFitBoth( img, bounds );

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

		void GainTime( RectangleF rect ) {
			using var img = ResourceImages.Singleton.GetImage( Img.FracturedDays_Gain2Time );
			graphics.DrawImageFitWidth(img, rect );
		}

		void Gain1TimeOr2CardPlaysX2( RectangleF rect ) {
			DrawImage( rect, Img.FracturedDays_Gain1Timex2 );
		}

		void Gain1TimeOr2EnergyX3( RectangleF rect ) {
			DrawImage(rect, Img.FracturedDays_Gain1Timex3 );
		}


		void DrawImage( RectangleF rect, Img img ) {
			using var image = ResourceImages.Singleton.GetImage( img );
			graphics.DrawImageFitBoth(image, rect );
		}

		static Bitmap GetTargetFilterIcon( string filterEnum ) {
			Img img = FilterEnumExtension.GetImgEnum( filterEnum );
			return img == default ? null : ResourceImages.Singleton.GetImage( img );
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

		public void Dispose() {
			if(cachedImageLayer != null)
				cachedImageLayer.Dispose();
		}
	}

}
