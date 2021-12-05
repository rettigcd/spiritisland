using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	public class PresenceTrackLayout {

		public PresenceTrackLayout(Rectangle bounds, Spirit spirit, int margin ) {

			var energySlots = spirit.Presence.GetEnergyTrackStatus();
			var cardSlots = spirit.Presence.GetCardPlayTrackStatus();

			// calc slot width and presence height
			float slotWidth = bounds.Width 
				/ (Math.Max( cardSlots.Count, energySlots.Count ) + 2); // +2 for energy & Destroyed;

			// Energy
			int energyRowHeight = bounds.Height / 2;
			int presenceWidth = (int)(slotWidth * 0.9f);
			Size presenceSize = new Size( presenceWidth, presenceWidth * 112 / 126 ); // presence icons are 126W x 112H

			CalcEnergyTrackMetrics( spirit, bounds, (int)slotWidth, presenceSize );

			Destroyed = new Rectangle( 
				(int)(bounds.Right - 2.5f * slotWidth + slotWidth / 2), 
				(int)(bounds.Y + energyRowHeight + margin + slotWidth * .5f), 
				(int)presenceSize.Width, 
				(int)presenceSize.Height
			);
			SlotLookup.Add(Track.Destroyed,new SlotMetrics { PresenceRect = Destroyed } );
			BigCoin = new RectangleF(  bounds.Right-slotWidth * 2, bounds.Y, slotWidth * 2, bounds.Height );
			CalculateCardPlaySlots( spirit, slotWidth, bounds.X, bounds.Y + energyRowHeight + margin, presenceSize );

			EnergyTitleLocation = bounds.Location;
		}

		public Point EnergyTitleLocation;
		public Point CardPlayTitleLocation;
		public Dictionary<Track,SlotMetrics> SlotLookup = new Dictionary<Track, SlotMetrics>();
		public Rectangle Destroyed;
		public RectangleF BigCoin; // Why is this float?

		public Rectangle ClickRectFor(Track track) => SlotLookup[track].PresenceRect;

		#region private

		void CalcEnergyTrackMetrics( Spirit spirit, Rectangle bounds, int slotWidth, Size presenceSize ) {

			int x = bounds.X;
			int y = bounds.Y;

			// Energy Slots
			float coinWidth = slotWidth * 0.8f;
			int presenceOffset = (int)presenceSize.Height*3/4;
			float coinLeftOffset = (slotWidth - coinWidth) / 2;
			foreach(var energySlot in spirit.Presence.GetEnergyTrackStatus()) {
				SlotLookup.Add( energySlot, new SlotMetrics {
					PresenceRect = new Rectangle( x + (slotWidth - presenceSize.Width) / 2, y, presenceSize.Width, presenceSize.Height ),
					TrackRect = new RectangleF( x + coinLeftOffset, y + presenceOffset, coinWidth, coinWidth )
				} );
				x += slotWidth;
			}

		}

		void CalculateCardPlaySlots( Spirit spirit, float slotWidth, int x, int y, Size presenceSize ) {
			// Add Card-Play title
			CardPlayTitleLocation = new Point(x,y);

			// Add Card-Play slots
			float usableWidth = slotWidth * 0.8f;
			int presenceYOffset = (int)presenceSize.Height * 3 / 4;
			int cardY = y + presenceYOffset;

			foreach(var status in spirit.Presence.GetCardPlayTrackStatus()) {
				SlotLookup.Add( status, new SlotMetrics {
					PresenceRect = new Rectangle( x + (int)((slotWidth - presenceSize.Width) / 2), y, presenceSize.Width, presenceSize.Height ),
					TrackRect = new RectangleF( x + slotWidth * .1f, cardY, usableWidth, usableWidth )
				} );
				x += (int)slotWidth;
			}

		}

		#endregion

	}

}
