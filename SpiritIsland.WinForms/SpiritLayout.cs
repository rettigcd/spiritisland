using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms {
	public class SpiritLayout {

		public SpiritLayout(Graphics graphics, Spirit spirit, Rectangle bounds, int margin ) {
			var rects = bounds.InflateBy(-margin).SplitVerticallyByWeight( margin, 200f, 360f, 420f, 60f );
			Calc_GrowthRow(spirit,rects[0],margin);
			trackLayout = new PresenceTrackLayout(rects[1],spirit,margin);
			int height = Calc_Innates( spirit, graphics, rects[2], margin );
			// If Innates are too tall, shrink them down.
			if(height > rects[2].Height) {
				var r = rects[2];
				var scaledRect = new Rectangle(r.X,r.Y,r.Width * r.Height / height, r.Height);
				Calc_Innates( spirit, graphics, scaledRect, margin );
			}
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

			int columnCount = spirit.InnatePowers.Length <= 4 ? 2 : 3;
			float textHeightMultiplier = spirit.InnatePowers.Length <= 4 ? 0.033f : 0.042f; // if we go to 3 columns, text needs to be larger.

			// Calc: Layout
			int maxBottom = bounds.Top;
			var innateBounds = bounds.SplitHorizontallyIntoColumns( margin, columnCount );
			for(int i=0;i<spirit.InnatePowers.Length;++i) {
				var power = spirit.InnatePowers[i];
				Rectangle singleBounds = innateBounds[i % columnCount];

				var innateLayout = new InnateLayout( power, singleBounds.X, singleBounds.Y, singleBounds.Width, textHeightMultiplier, graphics );
				innateLayouts[power] = innateLayout;

				// Scooch this box down so we can put next one there
				int shift = innateLayout.Bounds.Height + 5;
				innateBounds[i % columnCount].Height -= shift;
				innateBounds[i % columnCount].Y += shift;

				// Track bottom of both columns
				maxBottom = Math.Max( maxBottom, innateLayout.Bounds.Bottom );
			}

			return maxBottom - bounds.Top;
		}


	}


}
