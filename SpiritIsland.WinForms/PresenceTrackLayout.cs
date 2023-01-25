using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	public class PresenceTrackLayout {

		public PresenceTrackLayout(Rectangle bounds, Spirit spirit, int margin, VisibleButtonContainer buttonContainer ) {
			this.Bounds = bounds;

			var energySlots = spirit.Presence.Energy.Slots;
			var cardSlots = spirit.Presence.CardPlays.Slots;

			// Slot-Size
			int slotsNeeded = Math.Max( cardSlots.Count, energySlots.Count ) + 2;// +2 for energy & Destroyed;
			Size slotSize = new Size( bounds.Width / slotsNeeded, bounds.Height / 2 );


			var energyBoundsRect = new Rectangle( bounds.X, bounds.Y, slotSize.Width * energySlots.Count, slotSize.Height );
			var cardBoundsRect = new Rectangle( energyBoundsRect.X, energyBoundsRect.Bottom, slotSize.Width * cardSlots.Count, slotSize.Height );

			// Presence-Size
			int presenceWidth = (int)(slotSize.Width * 0.8f);
			Size presenceSize = new Size( presenceWidth, presenceWidth * 112 / 126 ); // presence icons are 126W x 112H

			CalcEnergyTrackLayout( spirit, energyBoundsRect, slotSize, presenceSize, buttonContainer );
			CalculateCardPlaySlots( spirit, cardBoundsRect, slotSize, presenceSize, buttonContainer );
			CalcDestroyedAndTimeLayout( bounds, margin, slotSize, presenceSize );

			CalcBigCoinLayout( bounds, slotSize.Width );

			EnergyTitleLocation = bounds.Location;
		}

		public Rectangle Bounds { get; }

		public Point EnergyTitleLocation;
		public Point CardPlayTitleLocation;

		public Dictionary<Track,PresenceSlotLayout> SlotLookup = new Dictionary<Track, PresenceSlotLayout>();

		public Rectangle Time;
		public RectangleF BigCoin; // Why is this float?

		public Rectangle ClickRectFor(Track track) => SlotLookup[track].PresenceRect;

		#region private

		void CalcEnergyTrackLayout( Spirit spirit, Rectangle bounds, Size slotSize, Size presenceSize, VisibleButtonContainer buttonContainer ) {

			int currentX = bounds.X;

			// Energy Slots
			float coinWidth = slotSize.Width * 0.8f;
			float coinMargin = (slotSize.Width - coinWidth) / 2;

			var presenceTrack = spirit.Presence.Energy;
			foreach(Track energySlot in presenceTrack.Slots) {
				var button = (PresenceSlotButton)buttonContainer[energySlot];

				button.DebugBounds  = new Rectangle( currentX, bounds.Y, slotSize.Width, slotSize.Height );
				button.PresenceRect = new Rectangle( currentX + (slotSize.Width - presenceSize.Width) / 2, bounds.Y, presenceSize.Width, presenceSize.Height );
				button.TrackRect    = new RectangleF( currentX + coinMargin, bounds.Bottom - coinWidth - coinMargin, coinWidth, coinWidth );

				SlotLookup.Add( energySlot, button );
				currentX += slotSize.Width;
			}

		}

		void CalculateCardPlaySlots( Spirit spirit, Rectangle bounds, Size slotSize, Size presenceSize, VisibleButtonContainer buttonContainer ) {
			int slotWidth = slotSize.Width;
			int x=bounds.X;
			int y=bounds.Y;
			// Add Card-Play title
			CardPlayTitleLocation = new Point(x,y);

			// Add Card-Play slots
			float margin = slotWidth *.05f; 
			float usableWidth = slotSize.Width - 2 * margin;
			int cardY = (int)(bounds.Bottom - margin - usableWidth);

			foreach(Track cardSlot in spirit.Presence.CardPlays.Slots) {
				var button = (PresenceSlotButton)buttonContainer[cardSlot];

				button.DebugBounds = new Rectangle( x, y, slotSize.Width, slotSize.Height );
				button.PresenceRect = new Rectangle( x + (int)((slotWidth - presenceSize.Width) / 2), y, presenceSize.Width, presenceSize.Height );
				button.TrackRect = new RectangleF( x + slotWidth * .1f, cardY, usableWidth, usableWidth );
				
				SlotLookup.Add( cardSlot, button );
				x += (int)slotWidth;
			}

		}

		void CalcDestroyedAndTimeLayout( Rectangle bounds, int margin, Size slotSize, Size presenceSize ) {
			int slotWidth = slotSize.Width;
			int energyRowHeight = slotSize.Height;
			int destroyedAndTimeY = (int)(bounds.Y + energyRowHeight + margin + slotWidth * .5f);

			SlotLookup.Add( Track.Destroyed, new PresenceSlotLayout { // !!! not 100% sure this is correct
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
