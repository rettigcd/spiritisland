using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	/// <summary>
	/// Binds together for a single painting: Graphics, Spirit(info), Layout, Clickable-Options
	/// </summary>
	class PresenceTrackPainter {

		readonly Graphics graphics;

		readonly Track[] energyTrackStatii;
		readonly Track[] cardTrackStatii;
		readonly Track[] energyRevealed;
		readonly Track[] cardRevealed;

		readonly int energy;
		readonly int destroyed;

		readonly Track[] clickableTrackOptions;
		readonly PresenceTrackLayout metrics;

		public PresenceTrackPainter( 
			Spirit spirit, 
			PresenceTrackLayout metrics, 
			Track[] clickableTrackOptions,
			Graphics graphics
		) {
			this.metrics = metrics;
			this.clickableTrackOptions = clickableTrackOptions;
			this.graphics = graphics;

			var presence = spirit.Presence;
			this.energy = spirit.Energy;
			this.destroyed = spirit.Presence.Destroyed;

			energyTrackStatii = presence.GetEnergyTrackStatus().ToArray();
			energyRevealed = presence.Energy.Revealed.ToArray();
			cardTrackStatii = presence.GetCardPlayTrackStatus().ToArray();
			cardRevealed = presence.CardPlays.Revealed.ToArray();

		}

		public void Paint(string presenceColor) {

			// !!! Cached all of the loaded images for the duration of the paint, then release them at the end.

			// Bottom Layer
			PaintLabels();
			PaintCurrentEnergy();

			foreach(var track in energyTrackStatii)
				DrawTheIcon( track.Icon, metrics.SlotLookup[track].TrackRect );

			foreach(var track in this.cardTrackStatii)
				DrawTheIcon( track.Icon, metrics.SlotLookup[track].TrackRect );

			// Middle Layer
			PaintHighlights();

			// Top Layer
			PaintPresence(presenceColor);

		}

		void PaintHighlights() {
			using Pen highlightPen = new( Color.Red, 8f );
			foreach(var track in clickableTrackOptions)
				graphics.DrawEllipse( highlightPen, metrics.SlotLookup[track].PresenceRect );
		}

		void PaintLabels() {
			using Font simpleFont = new( "Arial", 8, FontStyle.Bold, GraphicsUnit.Point );
			graphics.DrawString( "Energy", simpleFont, SystemBrushes.ControlDarkDark, metrics.EnergyTitleLocation );
			graphics.DrawString( "Cards", simpleFont, SystemBrushes.ControlDarkDark, metrics.CardPlayTitleLocation );
		}

		void PaintPresence(string presenceColor) {
			using Bitmap presenceImg = ResourceImages.Singleton.GetPresenceIcon( presenceColor );

			foreach(var track in energyTrackStatii)
				if(!energyRevealed.Contains(track))
					graphics.DrawImage( presenceImg, metrics.SlotLookup[track].PresenceRect );

			foreach(var track in cardTrackStatii)
				if(!cardRevealed.Contains(track))
					graphics.DrawImage( presenceImg, metrics.SlotLookup[track].PresenceRect );

			PaintDestroyed( presenceImg );
		}

		void DrawTheIcon( IconDescriptor icon, RectangleF bounds ) {

			if(icon == null) {
				graphics.FillRectangle( Brushes.Black, bounds );
				return;
			}

			// if we have a sub-image, reduce the main bounds to accommodate it
			const float subImageReduction = .8f;
			RectangleF mainBounds = (icon.Sub == null) ? bounds
				: new RectangleF(bounds.X,bounds.Y,bounds.Width* subImageReduction, bounds.Height* subImageReduction );

			// -- Main - Background
			if(icon.BackgroundImg != null) {
				using var img = ResourceImages.Singleton.GetResourceImage(icon.BackgroundImg);
				graphics.DrawImageFitBoth(img,mainBounds);
			}

			// -- Main - Content --
			if(icon.Text != null || icon.ContentImg != null) {

				var contentBounds = new RectangleF(
					mainBounds.X, 
					mainBounds.Y + mainBounds.Height * 0.22f,  // should be 20% but it looks a little high
					mainBounds.Width, 
					mainBounds.Height*.6f // 60%
				);
				// Content - Text
				if(icon.Text != null) {
					Font font = new Font( ResourceImages.Singleton.Fonts.Families[0], contentBounds.Height, GraphicsUnit.Pixel );
					StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
					graphics.DrawString( icon.Text, font, Brushes.Black, contentBounds, center );
				}

				// Content - Images
				if(icon.ContentImg != null) {
					using var img = ResourceImages.Singleton.GetResourceImage( icon.ContentImg );
					if(icon.ContentImg2 != null) {
						const float scale = .75f;
						float w = contentBounds.Width * scale, h = contentBounds.Height * scale;
						var cb1 = new RectangleF(contentBounds.X,contentBounds.Y,w,h);
						var cb2 = new RectangleF( contentBounds.X+ contentBounds.Width*(1f - scale), contentBounds.Y+contentBounds.Height* (1f - scale), w, h );
						using var img2 = ResourceImages.Singleton.GetResourceImage( icon.ContentImg2 );
						graphics.DrawImageFitBoth( img2, cb2 );
						graphics.DrawImageFitBoth( img, cb1 );
					} else
						graphics.DrawImageFitBoth( img, contentBounds );
				}

			}

			// -- Super --
			if(icon.Super != null) {
				float dim = bounds.Width * .4f;
				// drawing this at the bottom instead of the top because presence is covering top
				var superRect = new RectangleF(bounds.X,bounds.Bottom-dim, dim, dim );
				DrawTheIcon( icon.Super, superRect );
			}

			// -- Sub - (additional action) --
			if(icon.Sub != null) {
				// put the subRect in the bottom right corner
				float subDim = bounds.Width * .5f;
				var subRect = new RectangleF( bounds.Right - subDim, bounds.Bottom - subDim, subDim, subDim );
				DrawTheIcon( icon.Sub, subRect );
			}
		}

		void PaintCurrentEnergy() {
			RectangleF bigCoinBounds = metrics.BigCoin;
			float slotWidth = bigCoinBounds.Width/2;
			float bigCoinWidth = slotWidth * 1.7f;
			var energyRect = new RectangleF(
				bigCoinBounds.Right - bigCoinWidth,
				bigCoinBounds.Y,
				bigCoinWidth,
				bigCoinWidth
			);
			DrawTheIcon(new IconDescriptor { BackgroundImg = ImageNames.Coin, Text = energy.ToString() }, energyRect );
		}

		void PaintDestroyed(Bitmap presenceImg) {
			Rectangle rect = metrics.Destroyed;
			int destroyedCount = destroyed;
			if(destroyedCount == 0) return;

			// Presence & Red X
			graphics.DrawImage( presenceImg, rect );
			using var redX = ResourceImages.Singleton.GetIcon( "red-x" );
			graphics.DrawImage( redX, rect.X, rect.Y, rect.Width * 2 / 3, rect.Height * 2 / 3 );

			// count
			graphics.DrawCountIfHigherThan( rect, destroyedCount );

		}

	}

	public struct SlotMetrics {
		public Rectangle PresenceRect;
		public RectangleF TrackRect;
	}

}
