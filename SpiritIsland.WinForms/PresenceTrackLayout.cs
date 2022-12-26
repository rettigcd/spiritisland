using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	public class PresenceTrackLayout {

		public PresenceTrackLayout(Rectangle bounds, Spirit spirit, int margin ) {
			this.Bounds = bounds;

			var energySlots = spirit.Presence.GetEnergyTrack();
			var cardSlots = spirit.Presence.GetCardPlayTrack();

			// Slot-Size
			int slotsNeeded = Math.Max( cardSlots.Count, energySlots.Count ) + 2;// +2 for energy & Destroyed;
			Size slotSize = new Size( bounds.Width / slotsNeeded, bounds.Height / 2 );


			var energyBoundsRect = new Rectangle( bounds.X, bounds.Y, slotSize.Width * energySlots.Count, slotSize.Height );
			var cardBoundsRect = new Rectangle( energyBoundsRect.X, energyBoundsRect.Bottom, slotSize.Width * cardSlots.Count, slotSize.Height );

			// Presence-Size
			int presenceWidth = (int)(slotSize.Width * 0.8f);
			Size presenceSize = new Size( presenceWidth, presenceWidth * 112 / 126 ); // presence icons are 126W x 112H

			CalcEnergyTrackLayout( spirit, energyBoundsRect, slotSize, presenceSize );
			CalculateCardPlaySlots( spirit, cardBoundsRect, slotSize, presenceSize );

			CalcDestroyedAndTimeLayout( bounds, margin, slotSize, presenceSize );

			CalcBigCoinLayout( bounds, slotSize.Width );

			EnergyTitleLocation = bounds.Location;
		}

		public Rectangle Bounds { get; }

		public Point EnergyTitleLocation;
		public Point CardPlayTitleLocation;

		public Dictionary<Track,SlotMetrics> SlotLookup = new Dictionary<Track, SlotMetrics>();

		public Rectangle Time;
		public RectangleF BigCoin; // Why is this float?

		public Rectangle ClickRectFor(Track track) => SlotLookup[track].PresenceRect;

		#region private

		void CalcEnergyTrackLayout( Spirit spirit, Rectangle bounds, Size slotSize, Size presenceSize ) {

			int currentX = bounds.X;

			// Energy Slots
			float coinWidth = slotSize.Width * 0.8f;
			float coinMargin = (slotSize.Width - coinWidth) / 2;
			foreach(var energySlot in spirit.Presence.GetEnergyTrack()) {
				SlotLookup.Add( energySlot, new SlotMetrics {
					DebugBounds = new Rectangle( currentX, bounds.Y, slotSize.Width, slotSize.Height ),
					PresenceRect = new Rectangle( currentX + (slotSize.Width - presenceSize.Width) / 2, bounds.Y, presenceSize.Width, presenceSize.Height ),
					TrackRect = new RectangleF( currentX + coinMargin, bounds.Bottom - coinWidth-coinMargin, coinWidth, coinWidth )
				} );
				currentX += slotSize.Width;
			}

		}

		void CalculateCardPlaySlots( Spirit spirit, Rectangle bounds, Size slotSize, Size presenceSize ) {
			int slotWidth = slotSize.Width;
			int x=bounds.X;
			int y=bounds.Y;
			// Add Card-Play title
			CardPlayTitleLocation = new Point(x,y);

			// Add Card-Play slots
			float margin = slotWidth *.05f; 
			float usableWidth = slotSize.Width - 2 * margin;
			int cardY = (int)(bounds.Bottom - margin - usableWidth);

			foreach(var status in spirit.Presence.GetCardPlayTrack()) {
				SlotLookup.Add( status, new SlotMetrics {
					DebugBounds = new Rectangle( x, y, slotSize.Width, slotSize.Height ),
					PresenceRect = new Rectangle( x + (int)((slotWidth - presenceSize.Width) / 2), y, presenceSize.Width, presenceSize.Height ),
					TrackRect    = new RectangleF( x + slotWidth * .1f, cardY, usableWidth, usableWidth )
				} );
				x += (int)slotWidth;
			}

		}

		void CalcDestroyedAndTimeLayout( Rectangle bounds, int margin, Size slotSize, Size presenceSize ) {
			int slotWidth = slotSize.Width;
			int energyRowHeight = slotSize.Height;
			int destroyedAndTimeY = (int)(bounds.Y + energyRowHeight + margin + slotWidth * .5f);

			SlotLookup.Add( Track.Destroyed, new SlotMetrics {
				PresenceRect = new Rectangle(
					(int)(bounds.Right - 2 * slotWidth),
					destroyedAndTimeY,
					presenceSize.Width,
					presenceSize.Height
				)
			} );

			Time = new Rectangle(
				(int)(bounds.Right - 1 * slotWidth),
				destroyedAndTimeY,
				presenceSize.Width,
				presenceSize.Height
			);
		}

		void CalcBigCoinLayout( Rectangle bounds, int slotWidth ) {
			var temp = new RectangleF( bounds.Right - slotWidth * 2, bounds.Y, slotWidth * 2, bounds.Height );
			float bigCoinWidth = slotWidth * 1.7f;
			BigCoin = new RectangleF( temp.Right - bigCoinWidth, temp.Y, bigCoinWidth, bigCoinWidth );
		}

		#endregion

	}

}
