using System.Drawing;

namespace SpiritIsland.WinForms;

class CardLayout {

	public CardLayout(Rectangle bounds) {
		_bounds = bounds;
		// Vertical Stack: TopSpacer, Margin, Card-Height, Margin, TabHeight, Margin
		_topSpacer = (int)(bounds.Height * .1f);
		_cardBottomMargin = (int)(bounds.Height * .25f) + 2*MARGIN;
		_cardHeight = bounds.Height - _topSpacer - _cardBottomMargin - MARGIN;
		_cardWidth = _cardHeight * 350 / 500;

		_tabDimension = (int)(bounds.Height * .2f);
		_tabLeft = bounds.X + (bounds.Width - 3 * TAB_SPACER - 4*_tabDimension)/2; // create space for 4 decks/tabs
	}

	public Rectangle GetTabBounds( int i ) {
		return new Rectangle(
			_tabLeft + i * (_tabDimension+TAB_SPACER),
			_bounds.Bottom - _tabDimension - MARGIN,
			_tabDimension,
			_tabDimension
		);
	}

	public Rectangle GetCardActionLabel( int index, int totalCardsInDeck ) {
		int left = CalcLeft( totalCardsInDeck );
		// int left = _bounds.X + MARGIN;
		return new Rectangle(
			left + index * (_cardWidth + CARD_SPACER) + _cardWidth - _topSpacer,
			_bounds.Y + _topSpacer / 2,
			_topSpacer,
			_topSpacer
		);
	}

	int CalcLeft( int totalCardsInDeck ) => _bounds.X + (_bounds.Width - (_cardWidth + CARD_SPACER) * totalCardsInDeck + MARGIN) / 2;

	public Rectangle GetCardRect( int index, int totalCardsInDeck ) {
		int left = CalcLeft( totalCardsInDeck );
		// int left = _bounds.X + MARGIN;
		return new Rectangle(
			left + index * (_cardWidth + CARD_SPACER), 
			_bounds.Y + _topSpacer + MARGIN,
			_cardWidth, 
			_cardHeight
		);
	}

	readonly int _topSpacer;
	readonly int _tabDimension;
	readonly int _cardHeight;
	readonly int _cardWidth;
	readonly Rectangle _bounds;
	readonly int _tabLeft;
	readonly int _cardBottomMargin;

	const int MARGIN = 10;
	const int TAB_SPACER = 30;
	const int CARD_SPACER = 30;

}