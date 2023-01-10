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
			PresenceTrackLayout layout,
			PresenceTokenAppearance presenceColor
		) {
			_spirit = spirit;
			_layout = layout;
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
			graphics.DrawImage(_cachedBackgroundImage,_layout.Bounds);

			// Middle Layer - Hotspots
			PaintHighlights( graphics );

			// Top Layer - Presence
			PaintPresence( graphics );
			// Draw current energy
			new IconDrawer(graphics,imageDrawer).DrawTheIcon(new IconDescriptor { Text = _spirit.Energy.ToString() }, _layout.BigCoin );

		}

		void CalculateBackgroundImage() {
			_cachedBackgroundImage = new Bitmap( this._layout.Bounds.Width, this._layout.Bounds.Height );

			using Graphics g = Graphics.FromImage( _cachedBackgroundImage );
			g.TranslateTransform( -_layout.Bounds.X, -_layout.Bounds.Y );

			PaintLabels(g);
			var iconDrawer = new IconDrawer(g,imageDrawer);
			iconDrawer.DrawTheIcon(new IconDescriptor { BackgroundImg = Img.Coin }, _layout.BigCoin );

			// debug
			//foreach(var track in CardTrack.Union( EnergyTrack )) {
			//	var slot = _layout.SlotLookup[track];
			//	g.DrawRectangle( Pens.Red, slot.DebugBounds );
			//	g.DrawRectangle( Pens.Green, slot.TrackRect.ToInts() );
			//}


			foreach(var track in EnergyTrack)
				iconDrawer.DrawTheIcon( track.Icon, _layout.SlotLookup[track].TrackRect );

			foreach(var track in this.CardTrack)
				iconDrawer.DrawTheIcon( track.Icon, _layout.SlotLookup[track].TrackRect );

		}

		void PaintHighlights(Graphics graphics) {
			using Pen highlightPen = new( Color.Red, 8f );
			foreach(var track in clickableTrackOptions)
				graphics.DrawEllipse( highlightPen, _layout.SlotLookup[track].PresenceRect );
		}

		void PaintLabels(Graphics graphics) {
			using Font simpleFont = ResourceImages.Singleton.UseGameFont( 20f );
			graphics.DrawString( "Energy", simpleFont, Brushes.Black, _layout.EnergyTitleLocation );
			graphics.DrawString( "Cards", simpleFont, Brushes.Black, _layout.CardPlayTitleLocation );
		}

		void PaintPresence(Graphics graphics) {
			foreach(var track in EnergyTrack)
				if(!EnergyRevealed.Contains(track))
					graphics.DrawImage( _presenceImg, _layout.SlotLookup[track].PresenceRect );

			foreach(var track in CardTrack)
				if(!CardRevealed.Contains(track))
					graphics.DrawImage( _presenceImg, _layout.SlotLookup[track].PresenceRect );

			PaintDestroyed( graphics );

			if(_spirit is FracturedDaysSplitTheSky days)
				PaintTime( graphics, days.Time );
		}

		void PaintDestroyed( Graphics graphics ) {
			Rectangle rect = _layout.SlotLookup[Track.Destroyed].PresenceRect;
			int destroyedCount = _spirit.Presence.Destroyed;
			if(destroyedCount == 0) return;

			// Presence & Red X
			graphics.DrawImage( _presenceImg, rect );
			using var redX = ResourceImages.Singleton.RedX();
			graphics.DrawImage( redX, rect.X, rect.Y, rect.Width * 2 / 3, rect.Height * 2 / 3 );

			// count
			graphics.DrawCountIfHigherThan( rect, destroyedCount );

		}

		void PaintTime(Graphics graphics, int timeCount) {
			Rectangle rect = _layout.Time;
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

		Track[] EnergyTrack => _spirit.Presence.GetEnergyTrack().ToArray();
		Track[] EnergyRevealed => _spirit.Presence.Energy.Revealed.ToArray();

		Track[] CardTrack => _spirit.Presence.GetCardPlayTrack().ToArray();
		Track[] CardRevealed => _spirit.Presence.CardPlays.Revealed.ToArray();

		readonly Spirit _spirit;
		readonly PresenceTrackLayout _layout;
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
		public Rectangle DebugBounds;
	}

}
