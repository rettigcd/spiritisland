using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms;

class InvaderCardRect(InvaderCard _card) : IPaintableRect {

	static public RowRect GetInvaderCardsRect( GameState gameState ) {

		var deck = gameState.InvaderDeck;
		var paintables = new List<IPaintableRect>();

		// Slots
		for(int i = deck.ActiveSlots.Count-1; 0 <= i; --i)
			paintables.Add( GetInvaderSlotRect( deck.ActiveSlots[i] ) );

		// #-of-unrevealed-cards
		const int ExplorIndex = 0;
		paintables[ ExplorIndex ] = AddUnrevealedCount( paintables[ ExplorIndex ], deck.UnrevealedCards.Count );

		// Discard
		paintables.Add( new DiscardInvaderRect( deck.Discards ){ WidthRatio=.8f } ); // Cards are only drawn .8 high so rotated, needs to be .8 wide

		return new RowRect( FillFrom.Right, [..paintables] );
	}

	static IPaintableRect AddUnrevealedCount( IPaintableRect explorerRect, int unrevealedCount ) {
		if(unrevealedCount==0) return explorerRect; // don't need a count
		++unrevealedCount; // +1 because one of the cards is in the Explore pile?
		explorerRect = new PoolRect { WidthRatio = explorerRect.WidthRatio }
			.Float( explorerRect )
			.Float( new SubScriptRect( "x" + unrevealedCount ), 0, 0, 1, .8f );
		return explorerRect;
	}

	static PoolRect GetInvaderSlotRect( InvaderSlot slot ) {

		// Label
		var paintable = new PoolRect(){ WidthRatio = .55f }
			.Float( new TextRect(slot.Label), 0,.85f,1,.15f);

		int count = slot.Cards.Count;
		if(count == 0)
			{} // do nothing
		else if(count == 1)
			paintable.Float( new InvaderCardRect( slot.Cards[0] ), 0, 0, 1, .8f );
		else {
			float xStep = 1/(1f+count); // each step is a half card with;
			float cardWidth = 2*xStep; // we are dividing half/cards
			float cardHeight = cardWidth *1.5f; // 45mm x 68mm is aprox 1.5
			float yStep = cardHeight *.5f;
			for(int i=0;i<count;++i)
				paintable.Float( new InvaderCardRect( slot.Cards[i] ), i*xStep, i*yStep, cardWidth, cardHeight );
		}
		return paintable;
	}

	public float? WidthRatio => .66666f;

	public void Paint( Graphics graphics, Rectangle bounds ){
		using Image img = ResourceImages.Singleton.GetInvaderCard( _card );
		var fitted = bounds.FitBoth(img.Size);
		graphics.DrawImage( img, fitted );
	}
}