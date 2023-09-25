using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms;

public class InnatesLayout {

	public InnatesLayout(Spirit spirit, VisibleButtonContainer buttonContainer, Rectangle bounds){
		const int margin = 10;

		int actualHeight = Calc_Innates( spirit, bounds, margin, buttonContainer );

		// If Innates are too tall, shrink them down and re-calculate
		if(bounds.Height < actualHeight) {
			bounds.Width = bounds.Width * bounds.Height / actualHeight; // ! Cannot use *= with integers
			Calc_Innates( spirit, bounds, margin, buttonContainer );
		}
	}

	public Dictionary<InnatePower, InnateLayout> FindLayoutByInnate = new Dictionary<InnatePower, InnateLayout>();

	int Calc_Innates( Spirit spirit, Rectangle bounds, int margin, VisibleButtonContainer buttonContainer ) {

		int columnCount = spirit.InnatePowers.Length <= 4 ? 2 : 3;
		float textHeightMultiplier = spirit.InnatePowers.Length <= 4 ? 0.033f : 0.042f; // if we go to 3 columns, text needs to be larger.

		// Calc: Layout
		int maxBottom = bounds.Top;
		var innateBounds = bounds.SplitHorizontallyIntoColumns( margin, columnCount );
		for(int i = 0; i < spirit.InnatePowers.Length; ++i) {
			var power = spirit.InnatePowers[i];
			Rectangle singleBounds = innateBounds[i % columnCount];

			var innateLayout = new InnateLayout( power, singleBounds.X, singleBounds.Y, singleBounds.Width, textHeightMultiplier, buttonContainer );
			FindLayoutByInnate[power] = innateLayout;

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
