using System.Drawing;

namespace SpiritIsland.WinForms;

class InvaderCardMetrics {

	public InvaderCardMetrics( InvaderSlot slot, int x, int y, int width, int height, float textHeight ) {
		this.slot = slot;

		// Individual card rects
		int count = slot.Cards.Count;
		Rect = new Rectangle[count];
		if(0 < count) {
			int buildWidth = (int)(width / count);
			int buildHeight = (int)(height / count);
			for(int i = 0; i < Rect.Length; ++i)
				Rect[i] = new Rectangle( x + i * buildWidth, y + i * buildHeight, buildWidth, buildHeight );
		}

		// Text location
		textBounds = new RectangleF( x, y + height + textHeight * .1f, width, textHeight * 1.5f );
	}
	public readonly InvaderSlot slot;
	public readonly Rectangle[] Rect;
	public readonly RectangleF textBounds;

	public void Draw( Graphics graphics, Font labelFont, Font invaderStageFont ) {
		// Draw all of the cards in that slot
		// !! we could make them overlap and bigger
		for(int i = 0; i < Rect.Length; ++i) {
			var card = slot.Cards[i];
			if(card.Flipped)
				graphics.DrawInvaderCardFront( Rect[i], card );
			else
				DrawInvaderBack( graphics, invaderStageFont, i, card );

		}
		graphics.DrawStringCenter( slot.Label, labelFont, Brushes.Black, textBounds );
	}

	void DrawInvaderBack( Graphics graphics, Font invaderStageFont, int i, InvaderCard card ) {
		var cardRect = Rect[i];
		using(SolidBrush brush = new SolidBrush( Color.LightSteelBlue ))
			graphics.FillRoundedRectangle( brush, cardRect, (int)(cardRect.Width*.15f) );
		var smallerRect = cardRect.InflateBy( -(int)(cardRect.Width * .15f) );
		graphics.DrawInvaderCardBack( smallerRect, card );
		smallerRect = cardRect.InflateBy( -25 );
		graphics.DrawStringCenter( card.InvaderStage.ToString(), invaderStageFont, Brushes.DarkRed, smallerRect );
	}
}
