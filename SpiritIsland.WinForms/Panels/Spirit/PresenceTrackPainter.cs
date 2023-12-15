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
			SpiritImageMemoryCache tip
		) {
			_spirit = spirit;
			_layout = layout;

			_presenceImg = tip._presenceImg;
		}
		public void Paint( Graphics graphics, ImgMemoryCache imageDrawer ) {
			// Set single-thread variables

			// Bottom Layer - cache it
			if(_cachedBackgroundImage == null)
				CalculateBackgroundImage();
			graphics.DrawImage(_cachedBackgroundImage,_layout.Bounds);

			// Top Layer - Presence
			PaintPresence( graphics );

			// Draw current energy
			new IconDrawer(graphics,imageDrawer).DrawTheIcon(new IconDescriptor { Text = _spirit.Energy.ToString() }, _layout.BigCoin ); // !!! simplify

		}

		void CalculateBackgroundImage() {
			_cachedBackgroundImage = new Bitmap( this._layout.Bounds.Width, this._layout.Bounds.Height );

			using Graphics graphics = Graphics.FromImage( _cachedBackgroundImage );
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			graphics.TranslateTransform( -_layout.Bounds.X, -_layout.Bounds.Y );

			PaintLabels(graphics);

			using(var coin = ResourceImages.Singleton.GetTrack( new IconDescriptor { BackgroundImg = Img.Coin } ))
				graphics.DrawImage(coin, _layout.BigCoin );

			foreach(Track track in EnergySlots)
				((PresenceSlotButton)_layout.SlotLookup[track]).PaintBackground(graphics);

			foreach(Track track in this.CardPlaySlots)
				((PresenceSlotButton)_layout.SlotLookup[track]).PaintBackground( graphics );

		}

		void PaintLabels( Graphics graphics ) {
			using Font simpleFont = ResourceImages.Singleton.UseGameFont( 20f );
			graphics.DrawString( "Energy", simpleFont, Brushes.Black, _layout.EnergyTitleLocation );
			graphics.DrawString( "Cards", simpleFont, Brushes.Black, _layout.CardPlayTitleLocation );
		}


		void PaintPresence(Graphics graphics) {
			PaintDestroyed( graphics );

			if(_spirit is FracturedDaysSplitTheSky days)
				PaintTime( graphics, days.Time );
		}

		void PaintDestroyed( Graphics graphics ) {
			Rectangle rect = _layout.SlotLookup[Track.Destroyed].PresenceRect;
			int destroyedCount = _spirit.Presence.Destroyed.Count;
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
		}

		#region private 

		Track[] EnergySlots => _spirit.Presence.Energy.Slots.ToArray();

		Track[] CardPlaySlots => _spirit.Presence.CardPlays.Slots.ToArray();

		readonly Spirit _spirit;
		readonly PresenceTrackLayout _layout;
		readonly Image _presenceImg;

		Bitmap _cachedBackgroundImage;

		#endregion

	}

}
