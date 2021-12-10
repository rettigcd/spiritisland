﻿using SpiritIsland.JaggedEarth;
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

		// ? What does all of this really accomplish?
		Track[] EnergyTrack => spirit.Presence.GetEnergyTrack().ToArray();
		Track[] EnergyRevealed => spirit.Presence.Energy.Revealed.ToArray();

		Track[] CardTrack => spirit.Presence.GetCardPlayTrack().ToArray();
		Track[] CardRevealed => spirit.Presence.CardPlays.Revealed.ToArray();

		readonly Track[] clickableTrackOptions;
		readonly PresenceTrackLayout metrics;

		readonly Spirit spirit;

		public PresenceTrackPainter( 
			Spirit spirit, 
			PresenceTrackLayout metrics, 
			Track[] clickableTrackOptions,
			Graphics graphics
		) {
			this.metrics = metrics;
			this.clickableTrackOptions = clickableTrackOptions;
			this.graphics = graphics;
			this.spirit = spirit;
		}

		public void Paint(string presenceColor) {

			// !!! Cached all of the loaded images for the duration of the paint, then release them at the end.

			// Bottom Layer
			PaintLabels();
			PaintCurrentEnergy();

			foreach(var track in EnergyTrack)
				DrawTheIcon( track.Icon, metrics.SlotLookup[track].TrackRect );

			foreach(var track in this.CardTrack)
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

			foreach(var track in EnergyTrack)
				if(!EnergyRevealed.Contains(track))
					graphics.DrawImage( presenceImg, metrics.SlotLookup[track].PresenceRect );

			foreach(var track in CardTrack)
				if(!CardRevealed.Contains(track))
					graphics.DrawImage( presenceImg, metrics.SlotLookup[track].PresenceRect );

			PaintDestroyed( presenceImg );

			if(spirit is FracturedDaysSplitTheSky days)
				PaintTime( presenceImg, days.Time );
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
			if(icon.BackgroundImg != default) {
				using var img = ResourceImages.Singleton.GetImage(icon.BackgroundImg);
				graphics.DrawImageFitBoth(img,mainBounds);
			}

			// -- Main - Content --
			if(icon.Text != null || icon.ContentImg != default) {

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
				if(icon.ContentImg != default) {
					using var img = ResourceImages.Singleton.GetImage( icon.ContentImg );
					if(icon.ContentImg2 != default) {
						const float scale = .75f;
						float w = contentBounds.Width * scale, h = contentBounds.Height * scale;
						var cb1 = new RectangleF(contentBounds.X,contentBounds.Y,w,h);
						var cb2 = new RectangleF( contentBounds.X+ contentBounds.Width*(1f - scale), contentBounds.Y+contentBounds.Height* (1f - scale), w, h );
						using var img2 = ResourceImages.Singleton.GetImage( icon.ContentImg2 );
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
			// -- Big Sub --
			if(icon.BigSub != null) {
				// put the subRect in the bottom right corner
				float subDim = bounds.Width;
				var subRect = new RectangleF( bounds.Right - subDim/2, bounds.Bottom - subDim*3/4, subDim, subDim );
				DrawTheIcon( icon.BigSub, subRect );
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
			DrawTheIcon(new IconDescriptor { BackgroundImg = Img.Coin, Text = spirit.Energy.ToString() }, energyRect );
		}

		void PaintDestroyed(Bitmap presenceImg) {
			Rectangle rect = metrics.Destroyed;
			int destroyedCount = spirit.Presence.Destroyed;
			if(destroyedCount == 0) return;

			// Presence & Red X
			graphics.DrawImage( presenceImg, rect );
			using var redX = ResourceImages.Singleton.RedX();
			graphics.DrawImage( redX, rect.X, rect.Y, rect.Width * 2 / 3, rect.Height * 2 / 3 );

			// count
			graphics.DrawCountIfHigherThan( rect, destroyedCount );

		}

		void PaintTime(Bitmap presenceImg, int timeCount) {
			Rectangle rect = metrics.Time;
			if(timeCount== 0) return;

			// Presence
			graphics.DrawImage( presenceImg, rect );

			// Hour glass
			var hgRect = new Rectangle( rect.X, rect.Y, rect.Width * 2 / 3, rect.Height * 2 / 3 );
			using var hourglass = ResourceImages.Singleton.Hourglass();
			graphics.DrawImageFitBoth( hourglass, hgRect );

			// count
			graphics.DrawCountIfHigherThan( rect, timeCount );
		}

	}

	public struct SlotMetrics {
		public Rectangle PresenceRect;
		public RectangleF TrackRect;
	}

}
