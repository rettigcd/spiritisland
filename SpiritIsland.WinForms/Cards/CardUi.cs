using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

class CardUi {

	public SpiritCardInfo SpiritCardInfo {
		set {
			_spiritCardInfo = value;
			_currentDeck = _spiritCardInfo.AllDecks.First( x => x.Icon == Img.Deck_Hand );
		}
	}

	public CardLayout Layout {
		get { return _layout; }
		set { _layout = value; }
	}

	public event Action<PowerCard> CardClicked;
	public event Action AppearanceChanged;

	#region Draw / Paint

	public void DrawParts(Graphics graphics) {
		if(_spiritCardInfo==null) return;

		for(int i = 0; i< _spiritCardInfo.AllDecks.Length; ++i)
			DrawDeckTab( graphics, i );

		if(_currentDeck != null) {
			// DrawCardBackdrop( graphics );
			for(int i = 0; i < _currentDeck.Cards.Count; ++i)
				DrawCard( graphics, _currentDeck.Cards[i], i );
		}
	}

	void DrawCardBackdrop( Graphics graphics ) {
		int totalCardCount = _currentDeck.Cards.Count;
		Rectangle tl = _layout.GetCardRect( 0, totalCardCount );
		Rectangle br = _layout.GetCardRect( totalCardCount - 1, totalCardCount );
		Rectangle bgRect = new Rectangle( tl.X, tl.Y, br.Right - tl.X, br.Bottom - tl.Y ).InflateBy( 30 );
		using var brush = new SolidBrush( Color.FromArgb( 192, Color.White ) );
		graphics.FillRectangle( brush, bgRect );
	}

	void DrawDeckTab( Graphics graphics, int index ) {
		DeckInfo deck = _spiritCardInfo.AllDecks[index];
		if(deck.Cards.Count == 0) return;
		Rectangle bounds = _layout.GetTabBounds( index );

		// Yellow - Selected
		if(deck == _currentDeck)
			graphics.FillRectangle( Brushes.Yellow, bounds );
		// Icon
		graphics.DrawImageFitBoth( ResourceImages.Singleton.GetImage( deck.Icon ), bounds );
		// Contains Option cards
		if(deck.HasOption)
			graphics.DrawRectangle( Pens.Red, bounds );
		// Counts
		graphics.DrawCountIfHigherThan( bounds, deck.Cards.Count, 0 );
	}

	void DrawCard( Graphics graphics, PowerCard card, int index ) {

		int totalCardCount = _currentDeck.Cards.Count;
		var cardRect = _layout.GetCardRect( index, totalCardCount );

		bool isAnOption = _options.Contains( card );

		if(isAnOption) {
			// Draw red Selection box
			using Pen highlightPen = new Pen( Color.Red, 15 );
			graphics.DrawRectangle( highlightPen, cardRect );
		}

		// draw image
		graphics.DrawImage( _images.GetImage( card ), cardRect );

		if(isAnOption) {
			// Draw Label
			if(_pickPowerCardDecision != null) {
				var images = ResourceImages.Singleton;
				Rectangle labelRect = _layout.GetCardActionLabel( index, totalCardCount );
				using Image icon = _pickPowerCardDecision.Use( card ) switch {
					CardUse.AddToHand => images.GetImage( Img.GainCard ),
					CardUse.Discard => images.GetImage( Img.Deck_Discarded ),
					CardUse.Forget => images.GetImage( Img.Icon_DestroyedPresence ),
					CardUse.Gift => null,
					CardUse.Other => null,
					CardUse.Play => images.GetImage( Img.Icon_Play ),
					CardUse.Reclaim => images.GetImage( Img.Reclaim1 ),
					CardUse.Repeat => null,
					_ => null,
				};
				if(icon != null)
					graphics.DrawImage( icon, labelRect );
				else
					graphics.DrawString( _pickPowerCardDecision.Use( card ).ToString(), SystemFonts.MessageBoxFont, Brushes.White, labelRect.X, labelRect.Y );
			}
		}

	}

	#endregion Draw / Paint 

	public Action GetClickAction( Point coords ) {
		if( _spiritCardInfo == null || _layout == null ) return null;
		// Deck Tabs
		for(int i = 0; i < _spiritCardInfo.AllDecks.Length; ++i) {
			var clickedDeck = _spiritCardInfo.AllDecks[i];
			if(clickedDeck.Cards.Any() && _layout.GetTabBounds( i ).Contains( coords ))
				return () => { 
					_currentDeck = clickedDeck == _currentDeck ? null : clickedDeck;
					AppearanceChanged?.Invoke();
				};
		}

		// Cards
		if(_currentDeck != null)
			for(int i = 0; i < _currentDeck.Cards.Count; ++i)
				if(_layout.GetCardRect( i, _currentDeck.Cards.Count ).Contains( coords )) {
					PowerCard card = _currentDeck.Cards[i];
					return _options.Contains( card ) ? (() => CardClicked?.Invoke( card )) : null;
				}

		return null;
	}

	public void HandleNewDecision( IDecision decision ) {

		_pickPowerCardDecision = decision as Select.PowerCard; // capture so we can display card-action

		_options = _pickPowerCardDecision != null
			? new HashSet<PowerCard>( _pickPowerCardDecision.CardOptions )
			: new HashSet<PowerCard>( decision.Options.OfType<PowerCard>() ); // ??? when is this used

		// Track which Deck/Tabs have options
		UpdateWhichDeckTabsContainOptions();

		// If there are cards to display but we aren't on them, switch
		if(_currentDeck==null || !_currentDeck.HasOption)
			_currentDeck = _spiritCardInfo.AllDecks.FirstOrDefault( x => x.HasOption )
				?? null; // _spiritCardInfo.AllDecks.FirstOrDefault( x => x.Icon == Img.Deck_Hand );

	}

	void UpdateWhichDeckTabsContainOptions() {
		foreach(DeckInfo deck in _spiritCardInfo.AllDecks)
			deck.HasOption = false;
		_spiritCardInfo.ExtraDeck.Cards.Clear();
		_spiritCardInfo.ExtraDeck.HasOption = false;

		foreach(PowerCard card in _options) {
			var deck = _spiritCardInfo.AllDecks.Take( _spiritCardInfo.DeckCount )
				.FirstOrDefault( d => d.Cards.Contains( card ) );
			if(deck != null)
				deck.HasOption = true;
			else {
				_spiritCardInfo.ExtraDeck.Cards.Add( card );
				_spiritCardInfo.ExtraDeck.HasOption = true;
			}
		}
	}

	Select.PowerCard _pickPowerCardDecision;	// 1
	HashSet<PowerCard> _options;                               
	DeckInfo _currentDeck;						// 2
	SpiritCardInfo _spiritCardInfo;				// 3
	CardLayout _layout;							// 4


	readonly CardImageManager _images = new CardImageManager();

}
