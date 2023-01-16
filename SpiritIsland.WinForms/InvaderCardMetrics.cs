using System.Drawing;

namespace SpiritIsland.WinForms;

class InvaderCardMetrics {
	public InvaderCardMetrics( InvaderSlot slot, float x, float y, float width, float height, float textHeight ) {
		this.slot = slot;

		// Individual card rects
		int count = slot.Cards.Count;
		Rect = new RectangleF[count];
		float buildWidth = width / count, buildHeight = height / count;
		for(int i = 0; i < Rect.Length; ++i)
			Rect[i] = new RectangleF( x + i * buildWidth, y + i * buildHeight, buildWidth, buildHeight );

		// Text location
		textBounds = new RectangleF( x, y + height + textHeight * .1f, width, textHeight * 1.5f );
	}
	public readonly InvaderSlot slot;
	public readonly RectangleF[] Rect;
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
			graphics.FillRoundedRectangle( brush, cardRect.ToInts(), (int)(cardRect.Width*.15f) );
		var smallerRect = cardRect.InflateBy( -cardRect.Width * .15f );
		graphics.DrawInvaderCardBack( smallerRect, card );
		smallerRect = cardRect.InflateBy( -25f );
		graphics.DrawStringCenter( card.InvaderStage.ToString(), invaderStageFont, Brushes.DarkRed, smallerRect );
	}
}
