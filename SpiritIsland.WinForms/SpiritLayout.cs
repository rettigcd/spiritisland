using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {
	public class SpiritLayout {

		public SpiritLayout(Graphics graphics, Spirit spirit, Rectangle bounds, int margin ) {
			var rects = bounds.InflateBy(-margin).SplitVerticallyByWeight( margin, 200f, 360f, 420f, 60f );
			Calc_GrowthRow(spirit,rects[0],margin);
			trackLayout = new PresenceTrackLayout(rects[1],spirit,margin);
			Calc_Innates( spirit, graphics, rects[2],margin );
			Elements = new ElementLayout(rects[3]);
		}

		public Rectangle imgBounds; // Picutre of spirit
		public GrowthLayout growthLayout; // Growth
		public PresenceTrackLayout trackLayout; // presenct tracks
		public Dictionary<InnatePower, InnateLayout> innateLayouts = new Dictionary<InnatePower,InnateLayout>();
		public ElementLayout Elements;


		void Calc_GrowthRow( Spirit spirit, Rectangle bounds, int margin ) {
			// Calc: Layout (image & growth)
			imgBounds = new Rectangle( bounds.X, bounds.Y, bounds.Height * 3 / 2, bounds.Height );
			var growthBounds = new Rectangle( bounds.X + imgBounds.Width + margin, bounds.Y, bounds.Width - imgBounds.Width - margin, bounds.Height );
			growthLayout = new GrowthLayout(spirit.Growth.Options, growthBounds);
		}

		int Calc_Innates( Spirit spirit, Graphics graphics, Rectangle bounds, int margin ) {

			// Calc: Layout
			int maxHeight = 0;
			var innateBounds = bounds.SplitHorizontallyByWeight(margin,1f,1f); // split equally
			for(int i=0;i<spirit.InnatePowers.Length;++i) {
				var singleBounds = innateBounds[i];
				var power = spirit.InnatePowers[i];
				var layout = new InnateLayout( power, singleBounds.X, singleBounds.Y, singleBounds.Width, graphics );
				innateLayouts[power] = layout;
				maxHeight = Math.Max( layout.TotalInnatePowerBounds.Height, maxHeight );
			}

			return maxHeight;
		}


	}


}
