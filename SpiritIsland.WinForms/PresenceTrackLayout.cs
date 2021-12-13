using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {

	public class PresenceTrackLayout {

		public Rectangle OutterBounds { get; }

		public PresenceTrackLayout(Rectangle bounds, Spirit spirit, int margin ) {
			this.OutterBounds = bounds;

			var energySlots = spirit.Presence.GetEnergyTrack();
			var cardSlots = spirit.Presence.GetCardPlayTrack();

			// calc slot width and presence height
			float slotWidth = bounds.Width 
				/ (Math.Max( cardSlots.Count, energySlots.Count ) + 2); // +2 for energy & Destroyed;

			// Energy
			int energyRowHeight = bounds.Height / 2;
			int presenceWidth = (int)(slotWidth * 0.8f);
			Size presenceSize = new Size( presenceWidth, presenceWidth * 112 / 126 ); // presence icons are 126W x 112H

			CalcEnergyTrackMetrics( spirit, bounds, (int)slotWidth, presenceSize );

			Destroyed = new Rectangle( 
				(int)(bounds.Right - 2 * slotWidth ), 
				(int)(bounds.Y + energyRowHeight + margin + slotWidth * .5f), 
				presenceSize.Width, 
				presenceSize.Height
			);

			Time = new Rectangle( 
				(int)(bounds.Right - 1 * slotWidth ), 
				Destroyed.Y,
				presenceSize.Width, 
				presenceSize.Height
			);

			SlotLookup.Add(Track.Destroyed,new SlotMetrics { PresenceRect = Destroyed } );
			var temp = new RectangleF(  bounds.Right-slotWidth * 2, bounds.Y, slotWidth * 2, bounds.Height );
			float bigCoinWidth = slotWidth * 1.7f;
			BigCoin = new RectangleF( temp.Right - bigCoinWidth, temp.Y, bigCoinWidth, bigCoinWidth );

			CalculateCardPlaySlots( spirit, slotWidth, bounds.X, bounds.Y + energyRowHeight + margin, presenceSize );

			EnergyTitleLocation = bounds.Location;
		}

		public Point EnergyTitleLocation;
		public Point CardPlayTitleLocation;
		public Dictionary<Track,SlotMetrics> SlotLookup = new Dictionary<Track, SlotMetrics>();
		public Rectangle Destroyed;
		public Rectangle Time;
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
			foreach(var energySlot in spirit.Presence.GetEnergyTrack()) {
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
			int presenceYOffset = (int)(presenceSize.Height * .6f);
			int cardY = y + presenceYOffset;

			foreach(var status in spirit.Presence.GetCardPlayTrack()) {
				SlotLookup.Add( status, new SlotMetrics {
					PresenceRect = new Rectangle( x + (int)((slotWidth - presenceSize.Width) / 2), y, presenceSize.Width, presenceSize.Height ),
					TrackRect    = new RectangleF( x + slotWidth * .1f, cardY, usableWidth, usableWidth )
				} );
				x += (int)slotWidth;
			}

		}

		#endregion

	}

}
