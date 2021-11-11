﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms {

	class EnergyTrackPainter {

		readonly Graphics graphics;
		readonly Spirit spirit;
		readonly Bitmap presence;
		readonly Bitmap coin;
		readonly SizeF presenceSize;

		readonly Font simpleFont;
		readonly Pen highlightPen;
		readonly Track[] trackOptions;
		readonly Dictionary<IOption, RectangleF> hotSpots;

		public EnergyTrackPainter( Graphics graphics, Spirit spirit, Bitmap presence, SizeF presenceSize, Font simpleFont, Pen highlightPen, Track[] trackOptions, Dictionary<IOption, RectangleF> hotSpots ) {
			this.graphics = graphics;
			this.spirit = spirit;
			this.presence = presence;
			this.presenceSize = presenceSize;
			this.simpleFont = simpleFont;
			this.highlightPen = highlightPen;
			this.trackOptions = trackOptions;
			this.hotSpots = hotSpots;
			coin = ResourceImages.Singleton.GetToken( "coin" );
		}

		public void DrawEnergyRow(float slotWidth, int x, int y, int width, int height ) {
			int startingX = x; // capture so we calc differene at end.

			// Title
			graphics.DrawString( "Energy", simpleFont, SystemBrushes.ControlDarkDark, x, y );

			int revealedEnergySpaces = spirit.Presence.Energy.RevealedCount;

			float idx = 0;
			float maxY = y; // inc 

			foreach(var energy in spirit.Presence.Energy.slots) {
				float energyHeight = DrawEnergySlot( x, y, (int)slotWidth, highlightPen, energy,
					revealedEnergySpaces == idx && trackOptions.Contains( energy ),
					revealedEnergySpaces <= idx
				);

				++idx;
				x += (int)slotWidth;
				maxY = Math.Max( maxY, y + energyHeight );
			}

			const float scaleCoin = 0.7f;
			var energyRect = new RectangleF( 
				startingX + width - slotWidth * (1 + scaleCoin), 
				y, 
				slotWidth * (1 + scaleCoin), 
				slotWidth * (1 + scaleCoin)
			);
			graphics.DrawImage( coin, energyRect );
			DrawTextOnCoin( ref energyRect, spirit.Energy.ToString() );

		}


		public float DrawEnergySlot(
			int x, int y,
			int slotWidth,

			Pen highlightPen,

			Track energy,
			bool isNextToPlace,
			bool coverWithPresence
		) {
			float coinWidth = slotWidth * 0.8f;
			int presenceOffset = (int)presenceSize.Height / 2;
			float coinLeftOffset = (slotWidth - coinWidth) / 2;

			var energyIconRect = new RectangleF( x + coinLeftOffset, y +presenceOffset, coinWidth, coinWidth);
			var presenceRect   = new RectangleF( x + (slotWidth - presenceSize.Width) / 2, y, presenceSize.Width, presenceSize.Height );

			// Draw - energy icons
			DrawCoin( energyIconRect, energy );

			// Highlight Option
			if(isNextToPlace) {
				graphics.DrawEllipse( highlightPen, presenceRect );
				hotSpots.Add( energy, presenceRect );
			}

			// presence
			if(coverWithPresence)
				graphics.DrawImage( presence, presenceRect );

			return energyIconRect.Bottom - y;
		}

		public void DrawCardPlayTrack( float slotWidth, int x, int y ) {

			// draw title
			graphics.DrawString( "Cards", simpleFont, SystemBrushes.ControlDarkDark, x, y );

			float usableWidth = slotWidth * 0.8f;


			int revealedCardSpaces = spirit.Presence.CardPlays.RevealedCount;
			int idx = 0;

			int presenceYOffset = (int)presenceSize.Height / 2;

			int maxY = y;
			int cardY = y + presenceYOffset;
			foreach(var track in spirit.Presence.CardPlays.slots) {

				var rect = new RectangleF( x + slotWidth * .1f, cardY, usableWidth, usableWidth );
				var presenceRect = new RectangleF( x + (slotWidth - presenceSize.Width) / 2, y, presenceSize.Width, presenceSize.Height );

				DrawCard( track, rect );

				// Highlight Option
				if(revealedCardSpaces == idx && trackOptions.Contains( track )) {
					graphics.DrawEllipse( highlightPen, presenceRect );
					hotSpots.Add( track, presenceRect );
				}
				// presence
				if(revealedCardSpaces <= idx)
					graphics.DrawImage( presence, presenceRect );

				maxY = Math.Max( maxY, (int)rect.Bottom );
				x += (int)slotWidth;
				++idx;
			}

		}

		public void DrawCoin( RectangleF bounds, Track track ) {

			if( track.Energy.HasValue) {
				graphics.DrawImage( coin, bounds );
				DrawTextOnCoin(ref bounds, track.Energy.Value.ToString() );
			}

			// Element
			var innerBounds = bounds.InflateBy( -bounds.Height / 5 );
			if(track.Elements.Any())
				DrawElement( ref innerBounds, track.Elements );

			// Action
			if(track.Action != null)
				DrawAction( bounds, track );

		}

		void DrawCard( Track track, RectangleF bounds ) {
			var innerBounds = bounds.InflateBy( -bounds.Height / 5 );

			// Card PLays
			if( track.CardPlay.HasValue) {
				using var bitmap = ResourceImages.Singleton.GetIcon( track.CardPlay + " cardplay" );
				graphics.DrawImageFitHeight( bitmap, bounds );
				innerBounds.X += innerBounds.Width * 0.5f;
				innerBounds.Y += innerBounds.Height * 0.5f;
			}

			// Elements
			if(track.Elements.Any())
				DrawElement( ref innerBounds, track.Elements );

			// Action
			if(track.Action != null)
				DrawAction( innerBounds, track );

		}

		void DrawTextOnCoin( ref RectangleF bounds, string txt ) {
			// Draw Text
			bounds.Y += bounds.Height * .05f; // it looks too high
			Font coinFont = new Font( ResourceImages.Singleton.Fonts.Families[0], bounds.Height * .6f, GraphicsUnit.Pixel );
			StringFormat center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			graphics.DrawString( txt, coinFont, Brushes.Black, bounds, center );

			bounds.X += bounds.Width*.3f;
			bounds.Y += bounds.Height*.3f;
		}

		void DrawElement( ref RectangleF innerBounds, params Element[] elements ) {
			foreach(var element in elements) {
				using Image image = ResourceImages.Singleton.GetToken( element );
				graphics.DrawImageFitHeight( image, innerBounds );
				innerBounds.X += innerBounds.Width * 0.3f;
				innerBounds.Y += innerBounds.Height * 0.3f;
			}
		}

		void DrawAction( RectangleF bounds, Track track ) {
			// draw icon
			string iconName = track.Action.Name switch {
				"Reclaim(1)" => "reclaim 1",
				"Push1DahanFromLands" => "Push1dahan",
				"DrawMinorOnceAndPlayExtraCardThisTurn" => "Cardplayplusone",
				"DiscardElementsForCardPlay" => "DiscardElementsForCardPlay",
				_ => track.Action.Name
			};
			var bitmap = ResourceImages.Singleton.GetIcon( iconName );
			graphics.DrawImageFitHeight( bitmap, bounds );
		}

	}

}
