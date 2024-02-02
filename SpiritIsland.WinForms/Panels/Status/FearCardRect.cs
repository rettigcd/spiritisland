using System.Drawing;

namespace SpiritIsland.WinForms;

class FearCardRect( IFearCard _card, int _count ) : IPaintableRect {

	public float? WidthRatio => 5f/7f;

	public void Paint( Graphics graphics, Rectangle bounds ){
		using Image img = _card.Flipped
			? FearCardImageBuilder.Build( _card, ResourceImages.Singleton )  // !!! shouldn't this use the cache?
			: ResourceImages.Singleton.FearCardBack();
		var fitted = bounds.FitBoth(img.Size,Align.Far);
		graphics.DrawImage( img, fitted );
		graphics.DrawCountIfHigherThan( bounds, _count );
	}
}
