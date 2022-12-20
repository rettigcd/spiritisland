using SpiritIsland.JaggedEarth;
using System;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	/// <summary>
	/// Binds together for a single painting: Graphics, Spirit(info), Layout, Clickable-Options
	/// </summary>
	class PresenceTrackPainter : IDisposable {

		public PresenceTrackPainter( 
			Spirit spirit, 
			PresenceTrackLayout metrics,
			PresenceTokenAppearance presenceColor
		) {
			this.spirit = spirit;
			this.metrics = metrics;
			_presenceImg = ResourceImages.Singleton.GetPresenceImage( presenceColor.BaseImage );
			presenceColor.Adjustment?.Adjust( _presenceImg );
		}

		public void Paint( Graphics graphics, Track[] clickableTrackOptions, CachedImageDrawer imageDrawer ) {
			// Set single-thread variables
			this.clickableTrackOptions = clickableTrackOptions;
			this.imageDrawer = imageDrawer;

			// Bottom Layer - cache it
			if(_cachedBackgroundImage == null)
				CalculateBackgroundImage();
			graphics.DrawImage(_cachedBackgroundImage,metrics.OutterBounds);

			// Middle Layer - Hotspots
			PaintHighlights( graphics );

			// Top Layer - Presence
			PaintPresence( graphics );
			// Draw current energy
			new IconDrawer(graphics,imageDrawer).DrawTheIcon(new IconDescriptor { Text = spirit.Energy.ToString() }, metrics.BigCoin );

		}

		void CalculateBackgroundImage() {
			_cachedBackgroundImage = new Bitmap( this.metrics.OutterBounds.Width, this.metrics.OutterBounds.Height );

			using Graphics g = Graphics.FromImage( _cachedBackgroundImage );
			g.TranslateTransform( -metrics.OutterBounds.X, -metrics.OutterBounds.Y );

			PaintLabels(g);
			var iconDrawer = new IconDrawer(g,imageDrawer);
			iconDrawer.DrawTheIcon(new IconDescriptor { BackgroundImg = Img.Coin }, metrics.BigCoin );

			foreach(var track in EnergyTrack)
				iconDrawer.DrawTheIcon( track.Icon, metrics.SlotLookup[track].TrackRect );

			foreach(var track in this.CardTrack)
				iconDrawer.DrawTheIcon( track.Icon, metrics.SlotLookup[track].TrackRect );
		}

		void PaintHighlights(Graphics graphics) {
			using Pen highlightPen = new( Color.Red, 8f );
			foreach(var track in clickableTrackOptions)
				graphics.DrawEllipse( highlightPen, metrics.SlotLookup[track].PresenceRect );
		}

		void PaintLabels(Graphics graphics) {
			using Font simpleFont = IslandControl.UseGameFont( 12f );
			graphics.DrawString( "Energy", simpleFont, SystemBrushes.ControlDarkDark, metrics.EnergyTitleLocation );
			graphics.DrawString( "Cards", simpleFont, SystemBrushes.ControlDarkDark, metrics.CardPlayTitleLocation );
		}

		void PaintPresence(Graphics graphics) {
			foreach(var track in EnergyTrack)
				if(!EnergyRevealed.Contains(track))
					graphics.DrawImage( _presenceImg, metrics.SlotLookup[track].PresenceRect );

			foreach(var track in CardTrack)
				if(!CardRevealed.Contains(track))
					graphics.DrawImage( _presenceImg, metrics.SlotLookup[track].PresenceRect );

			PaintDestroyed( graphics );

			if(spirit is FracturedDaysSplitTheSky days)
				PaintTime( graphics, days.Time );
		}

		void PaintDestroyed( Graphics graphics ) {
			Rectangle rect = metrics.Destroyed;
			int destroyedCount = spirit.Presence.Destroyed;
			if(destroyedCount == 0) return;

			// Presence & Red X
			graphics.DrawImage( _presenceImg, rect );
			using var redX = ResourceImages.Singleton.RedX();
			graphics.DrawImage( redX, rect.X, rect.Y, rect.Width * 2 / 3, rect.Height * 2 / 3 );

			// count
			graphics.DrawCountIfHigherThan( rect, destroyedCount );

		}

		void PaintTime(Graphics graphics, int timeCount) {
			Rectangle rect = metrics.Time;
			if(timeCount== 0) return;

			// Presence
			graphics.DrawImage( _presenceImg, rect );

			// Hour glass
			var hgRect = new Rectangle( rect.X, rect.Y, rect.Width * 2 / 3, rect.Height * 2 / 3 );
			using var hourglass = ResourceImages.Singleton.Hourglass();
			graphics.DrawImageFitBoth( hourglass, hgRect );

			// count
			graphics.DrawCountIfHigherThan( rect, timeCount );
		}

		public void Dispose() {
			_cachedBackgroundImage?.Dispose();
			_cachedBackgroundImage = null;
			_presenceImg?.Dispose();
			_presenceImg = null;
		}

		#region private 

		Track[] EnergyTrack => spirit.Presence.GetEnergyTrack().ToArray();
		Track[] EnergyRevealed => spirit.Presence.Energy.Revealed.ToArray();

		Track[] CardTrack => spirit.Presence.GetCardPlayTrack().ToArray();
		Track[] CardRevealed => spirit.Presence.CardPlays.Revealed.ToArray();

		readonly Spirit spirit;
		readonly PresenceTrackLayout metrics;
		Bitmap _presenceImg;

		// Single threaded / instance methods
		CachedImageDrawer imageDrawer;
		Track[] clickableTrackOptions;

		Bitmap _cachedBackgroundImage;

		#endregion

	}

	public struct SlotMetrics {
		public Rectangle PresenceRect;
		public RectangleF TrackRect;
	}

}
