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
			DrawEnergyBalance( energyRect );

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
			using(var bitmap = ResourceImages.Singleton.GetTokenIcon( energy.Text ))
				graphics.DrawImage( bitmap, energyIconRect );

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

		public void DrawEnergyBalance( RectangleF bounds ) {
			string txt = spirit.Energy.ToString();
			graphics.DrawImage( coin, bounds );
			Font coinFont = new Font( ResourceImages.Singleton.Fonts.Families[0], bounds.Height * .5f );
			SizeF textSize = graphics.MeasureString( txt, coinFont );
			graphics.DrawString( txt, coinFont, Brushes.Black, bounds.X + bounds.Width / 2 - textSize.Width * .5f, bounds.Y + bounds.Height * .5f - textSize.Height * .45f );
		}

	}

}
