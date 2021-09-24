using System;
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
			coin = ResourceImages.Singleton.GetTokenIcon( "coin" );
		}

		public Size DrawEnergyRow(float slotWidth, int x, int y, float width ) {
			int startingX = x; // capture so we calc differene at end.
			int startingY = y; // capture so we calc differene at end.

			// Title
			graphics.DrawString( "Energy", simpleFont, SystemBrushes.ControlDarkDark, x, y );

			int revealedEnergySpaces = spirit.Presence.Energy.RevealedCount;

			float idx = 0;
			float maxY = y; // inc 

			foreach(var energy in spirit.Presence.Energy.slots) {
				float height = DrawEnergySlot( x, y, (int)slotWidth, highlightPen, energy,
					revealedEnergySpaces == idx && trackOptions.Contains( energy ),
					revealedEnergySpaces <= idx
				);

				++idx;
				x += (int)slotWidth;
				maxY = Math.Max( maxY, y + height );
			}

			const float scaleCoin = 0.7f;
			var energyRect = new RectangleF( 
				startingX + (int)width - slotWidth * (1 + scaleCoin), 
				y, 
				slotWidth * (1 + scaleCoin), 
				slotWidth * (1 + scaleCoin)
			);
			DrawCoin( energyRect, spirit.Energy.ToString() );

			return new Size(
				x - startingX,
				(int)maxY - startingY // 
			);
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
			DrawCoin(energyIconRect,energy.Text);

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

		public float DrawCardPlayTrack( float slotWidth, int x, int y ) {
			int startingY = y; // capture so we can calc Height

			// draw title
			graphics.DrawString( "Cards", simpleFont, SystemBrushes.ControlDarkDark, x, y );

			float maxCardHeight = 0;
			float cardWidth = slotWidth * 0.6f;
			float cardLeft = (slotWidth - cardWidth) / 2; // center

			int revealedCardSpaces = spirit.Presence.CardPlays.RevealedCount;
			int idx = 0;

			int presenceYOffset = (int)presenceSize.Height / 2;

			int maxY = y;
			int cardY = y + presenceYOffset;
			foreach(var track in spirit.Presence.CardPlays.slots) {

				// card plays amount

				if(track.Text == "any energy") {
					float coinWidth = slotWidth * 0.8f;
					var rr = new RectangleF(x+slotWidth*.1f,cardY,coinWidth,coinWidth);
					DrawCoin( rr, track.Text );
				} else
					using( var bitmap = ResourceImages.Singleton.GetTokenIcon(track.Text)) {

						float cardHeight = cardWidth * bitmap.Height / bitmap.Width;
						maxCardHeight = Math.Max(cardHeight,maxCardHeight);

						var rect = new RectangleF( x+ cardLeft, cardY, cardWidth, cardHeight );
						graphics.DrawImage( bitmap, rect );
						maxY = Math.Max( maxY, cardY + (int)cardHeight );
					};


				RectangleF presenceRect = new RectangleF( x + (slotWidth - presenceSize.Width) / 2, y, presenceSize.Width, presenceSize.Height );

				// Highlight Option
				if(revealedCardSpaces == idx && trackOptions.Contains( track )) {
					graphics.DrawEllipse( highlightPen, presenceRect );
					hotSpots.Add( track, presenceRect );
				}


				// presence
				if(revealedCardSpaces <= idx)
					graphics.DrawImage( presence, presenceRect );

				x += (int)slotWidth;
				++idx;
			}

			return maxY - startingY;

		}

		public void DrawCoin( RectangleF bounds, string txt ) {
			graphics.DrawImage( coin, bounds );

			if(txt.EndsWith(" energy"))
				txt = txt[..^7];


			if("sun|moon|fire|air|water|earth|plant|animal|any".Contains( txt )) {
				string filename = "Simple_" + txt.ToString().ToLower();
				using Image image = ResourceImages.Singleton.GetTokenIcon(filename); //  Image.FromFile( $".\\images\\tokens\\{filename}.png" );
				var elementBounds = bounds.InflateBy(-bounds.Height/4);
				graphics.DrawImageFitHeight(image,elementBounds);
			} else {
				bounds.Y += bounds.Height * .05f; // it looks too high
				using Font coinFont = new Font( ResourceImages.Singleton.Fonts.Families[0], bounds.Height * .6f, GraphicsUnit.Pixel );
				StringFormat center = new StringFormat{ Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
				graphics.DrawString( txt, coinFont, Brushes.Black, bounds, center );
			}

		}

	}

}
