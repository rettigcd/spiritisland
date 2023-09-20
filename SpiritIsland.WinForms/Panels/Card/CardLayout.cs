using System.Drawing;

namespace SpiritIsland.WinForms;

class CardLayout {

	public CardLayout( Rectangle bounds, int cardCount ) {
		_bounds = bounds;

		_rowCount = 0;
		do {
			++_rowCount;
			_cardHeight = (bounds.Height - MARGIN) / _rowCount;	// determine card height based on # of rows
			_cardWidth = _cardHeight * 350 / 500;					// scale width proportionally
			_colCount = (bounds.Width-MARGIN)/(_cardWidth+MARGIN);	// count # of columns that fit
		} while( _colCount * _rowCount < cardCount );				// as long as cards don't fit, add a row
		
	}

	public Rectangle GetCardActionLabel( int index, int totalCardsInDeck ) {
		int left = _bounds.Left + MARGIN + (index % _colCount) * (MARGIN+_cardWidth);
		int top = _bounds.Bottom - (index/_colCount) * (MARGIN+_cardHeight);
		return new Rectangle( left, top, MARGIN*2, MARGIN * 2 );
	}

	public Rectangle GetCardRect( int index ) {
		int left = _bounds.Left + MARGIN + (index % _colCount) * (MARGIN + _cardWidth);
		int top = _bounds.Bottom - (index / _colCount + 1) * (MARGIN + _cardHeight);
		return new Rectangle( left, top, _cardWidth, _cardHeight );
	}

	readonly int _cardHeight;
	readonly int _cardWidth;
	readonly Rectangle _bounds;
	readonly int _rowCount;
	readonly int _colCount;

	const int MARGIN = 10;
	const int CARD_SPACER = 30;

}