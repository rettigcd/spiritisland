using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms;

class DiscardInvaderRect( List<InvaderCard> cards ) : IPaintableRect {

	public float? WidthRatio {get;set;}

	public void Paint(Graphics graphics, Rectangle bounds)	{
		Rectangle fitted = bounds.FitBoth(68,45,Align.Center,Align.Center);

		if(0<cards.Count)
			DrawRotatedCard( cards[^1], graphics, fitted );

		graphics.DrawCountIfHigherThan( fitted, cards.Count );

	}

	static void DrawRotatedCard( InvaderCard card, Graphics graphics, Rectangle fitted ) {
		Point[] destinationPoints = [
			new Point(fitted.Left, fitted.Bottom),  // rotate TL => BL
				new Point(fitted.Left, fitted.Top),     // rotate TR => TL
				new Point(fitted.Right,fitted.Bottom)   // rotate BL => BR
		];
		using Image discardImg = ResourceImages.Singleton.GetInvaderCard( card );
		graphics.DrawImage( discardImg, destinationPoints );
	}
}
