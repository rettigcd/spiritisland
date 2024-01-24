using System.Drawing;
using SpiritIsland.FeatherAndFlame;

namespace SpiritIsland.WinForms;

class InvaderCardRect(InvaderCard _card) : IPaintableBlockRect {

	public float WidthRatio => .6666f;

	public Rectangle Paint( Graphics graphics, Rectangle bounds ){
		using Image img = ResourceImages.Singleton.GetInvaderCard( _card );
		var fitted = bounds.FitBoth(img.Size);
		graphics.DrawImage( img, fitted );
		return fitted;
	}
}