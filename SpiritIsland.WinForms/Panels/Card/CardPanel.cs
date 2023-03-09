using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

class CardPanel : IPanel {

	public CardPanel(SharedCtx ctx, Control parentControl) {
		_ctx = ctx;

		_spiritCardInfo = new SpiritCardInfo( _ctx._spirit );
		_currentDeck = _spiritCardInfo.AllDecks
			.First( x => x.Icon == Img.Deck_Hand );

		_onAppearanceChanged = parentControl.Invalidate;
	}

	public Rectangle Bounds { 
		get => _bounds;
		set { 
			_bounds = value;
			_layout = null;
		}
	}

	public void OnGameLayoutChanged() { _layout = null; }

	#region Paint

	public void Paint(Graphics graphics) {

		_layout ??= new CardLayout( _bounds );

		for(int i = 0; i< _spiritCardInfo.AllDecks.Length; ++i)
			PaintDeckTab( graphics, i );

		if(_currentDeck != null) {
			// DrawCardBackdrop( graphics );
			int countToShow = Math.Min(_layout.MaxCards, _currentDeck.Cards.Count- _currentDeck.FirstCard );
			for(int i = 0; i < countToShow; ++i)
				PaintCard( graphics, _currentDeck.Cards[i+_currentDeck.FirstCard], i );

			// Prev
			if(0 < _currentDeck.FirstCard) {
				int l = _layout.PrevArrow.Left, r = _layout.PrevArrow.Right, yOffset = (r-l);
				Point point = new Point(l,_layout.PrevArrow.Top + _layout.PrevArrow.Height/2);
				Point above = new Point(r, point.Y - yOffset);
				Point below = new Point(r, point.Y + yOffset );
				Rectangle box = new Rectangle( l, point.Y - yOffset, (r-l)*4/5, yOffset * 2 ); 
				graphics.FillPolygon(Brushes.Gold, new[] {above,point,below });
				using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
				graphics.DrawString( _currentDeck.FirstCard.ToString(), SystemFonts.MessageBoxFont, Brushes.Black, box, alignCenter );
			}

			// Next
			if(_currentDeck.FirstCard+countToShow < _currentDeck.Cards.Count) {
				int l = _layout.NextArrow.Left, r = _layout.NextArrow.Right, yOffset = (r - l);
				Point point = new Point( r, _layout.NextArrow.Top + _layout.NextArrow.Height / 2 );
				Point above = new Point( l, point.Y - yOffset );
				Point below = new Point( l, point.Y + yOffset );
				graphics.FillPolygon( Brushes.Gold, new[] { above, point, below } );
				Rectangle box = new Rectangle(l+(r-l)/5,point.Y - yOffset, (r-l)*4/5, yOffset*2);
				using StringFormat alignCenter = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
				graphics.DrawString( (_currentDeck.Cards.Count-_currentDeck.FirstCard-countToShow).ToString(), SystemFonts.MessageBoxFont, Brushes.Black, box, alignCenter );
			}
		}
	}

	//void DrawCardBackdrop( Graphics graphics ) {
	//	int totalCardCount = _currentDeck.Cards.Count;
	//	Rectangle tl = _layout.GetCardRect( 0, totalCardCount );
	//	Rectangle br = _layout.GetCardRect( totalCardCount - 1, totalCardCount );
	//	Rectangle bgRect = new Rectangle( tl.X, tl.Y, br.Right - tl.X, br.Bottom - tl.Y ).InflateBy( 30 );
	//	using var brush = new SolidBrush( Color.FromArgb( 192, Color.White ) );
	//	graphics.FillRectangle( brush, bgRect );
	//}

	void PaintDeckTab( Graphics graphics, int index ) {
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

	void PaintCard( Graphics graphics, PowerCard card, int index ) {

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

	#endregion Paint 

	public Action GetClickableAction( Point coords ) {
		if( _spiritCardInfo == null || _layout == null ) return null;
		// Deck Tabs
		for(int i = 0; i < _spiritCardInfo.AllDecks.Length; ++i) {
			var clickedDeck = _spiritCardInfo.AllDecks[i];
			if(clickedDeck.Cards.Any() && _layout.GetTabBounds( i ).Contains( coords ))
				return () => { 
					_currentDeck = clickedDeck == _currentDeck ? null : clickedDeck;
					_onAppearanceChanged?.Invoke();
				};
		}

		// Cards
		if(_currentDeck != null) {
			int countToShow = Math.Min( _layout.MaxCards, _currentDeck.Cards.Count- _currentDeck.FirstCard );
			for(int i = 0; i < countToShow; ++i)
				if(_layout.GetCardRect( i, _currentDeck.Cards.Count ).Contains( coords )) {
					PowerCard card = _currentDeck.Cards[_currentDeck.FirstCard+i];
					return _options.Contains( card ) 
						? (() => _ctx.SelectOption( card ))
						: null;
				}

			if(_layout.NextArrow.Contains( coords )) {
				return () => {
					_currentDeck.FirstCard++;
					_onAppearanceChanged?.Invoke();
				};
			}

			if(0 < _currentDeck.FirstCard && _layout.PrevArrow.Contains( coords )) {
				return () => {
					_currentDeck.FirstCard--;
					_onAppearanceChanged?.Invoke();
				};
			}

		}

		return null;
	}

	public void ActivateOptions( IDecision decision ) {

		_pickPowerCardDecision = decision as Select.PowerCard; // capture so we can display card-action

		_options = _pickPowerCardDecision != null
			? new HashSet<PowerCard>( _pickPowerCardDecision.CardOptions )
			: new HashSet<PowerCard>( decision.Options.OfType<PowerCard>() ); // ??? when is this used

		// Track which Deck/Tabs have options
		UpdateWhichDeckTabsContainOptions();

		// If there are cards to display but we aren't on them, switch
		if(_currentDeck==null || !_currentDeck.HasOption)
			_currentDeck = _spiritCardInfo.AllDecks.FirstOrDefault( x => x.HasOption )
				?? null;

	}

	public int OptionCount => _options.Count;

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

	Select.PowerCard _pickPowerCardDecision;
	HashSet<PowerCard> _options;                               
	DeckInfo _currentDeck;
	Rectangle _bounds;
	CardLayout _layout;

	readonly SharedCtx _ctx;
	readonly SpiritCardInfo _spiritCardInfo;
	readonly CardImageManager _images = new CardImageManager();
	readonly Action _onAppearanceChanged;
}
