using System.Drawing;

namespace SpiritIsland.WinForms;

class BlightCardRect(IBlightCard _blightCard) : IPaintableRect {

	public float? WidthRatio => 5f/7f;
	
	public void Paint( Graphics graphics, Rectangle bounds ){
		using Image healthy = _blightCard.CardFlipped
			? ResourceImages.Singleton.GetBlightCard( _blightCard )
			: ResourceImages.Singleton.GetHealthBlightCard();
		var fitted = bounds.FitHeight( healthy.Size );
		graphics.DrawImageFitHeight( healthy, fitted );
	}

}
