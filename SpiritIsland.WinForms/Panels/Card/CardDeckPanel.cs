using SpiritIsland.NatureIncarnate;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

class CardDeckPanel : IPanel {

	public CardDeckPanel( SharedCtx ctx, Control _, int deckIndex, Color bgColor ) {
		_ctx = ctx;
		_deckIndex = deckIndex;
		_currentDeck = _ctx._spirit.Decks[deckIndex];
		// _onAppearanceChanged = parentControl.Invalidate;
		_bgColor = bgColor;
	}

	readonly Color _bgColor;

	public int ZIndex => _currentDeck.Cards.Count == 0 ? 0 
		: HasFocus ? 2
		: 1;

	public void AssignBounds( RegionLayoutClass regionLayout ) {
		Bounds = regionLayout.DeckRects[ _deckIndex ];
	}

	public RegionLayoutClass GetLayout( Rectangle bounds ) {
		return 0 < _currentDeck.Cards.Count 
			? RegionLayoutClass.ForCardFocused( bounds, _ctx._spirit.Decks.Length + 1, _deckIndex )
			: RegionLayoutClass.ForIslandFocused( bounds, _ctx._spirit.Decks.Length + 1);
	}

	public Rectangle Bounds {
		get => _bounds;
		set {
			_bounds = value;
			_layout = null;
		}
	}
	public bool HasFocus { set; private get; }

	public void OnGameLayoutChanged() { _layout = null; }

	#region Paint

	public void Paint( Graphics graphics ) {
		if(_bounds.Width == 0 || _bounds.Height == 0 || _currentDeck.Cards.Count == 0) return;

		if(!HasFocus)
			using(SolidBrush bgBrush = new SolidBrush( Color.FromArgb( 200, _bgColor ) ))
				graphics.FillRectangle( bgBrush, _bounds );

		_layout = new CardLayout( _bounds, _currentDeck.Cards.Count );
		for(int i = 0; i < _currentDeck.Cards.Count; ++i)
			PaintCard( graphics, _currentDeck.Cards[i], i );
	}

	void PaintCard( Graphics graphics, PowerCard card, int index ) {

		int totalCardCount = _currentDeck.Cards.Count;
		var cardRect = _layout.GetCardRect( index );

		bool isAnOption = _options != null && _options.Contains( card );

		if(isAnOption) {
			// Draw red Selection box
			using Pen highlightPen = new Pen( Color.Red, 15 );
			graphics.DrawRectangle( highlightPen, cardRect );
		}

		// draw image
		graphics.DrawImage( _images.GetImage( card ), cardRect );

		// Draw Impending Energy
		if(_ctx._spirit is DancesUpEarthquakes due && due.Impending.Contains(card)) {
			int remaining = Math.Max(0,card.Cost - due.ImpendingEnergy[card.Name]);
			var bot = cardRect.SplitVerticallyAt( .5f )[1];
			Rectangle numRect = bot.InflateBy( -bot.Width / 4, -bot.Height / 3 );
			using var bgBrush = new SolidBrush(Color.FromArgb(128,Color.White));
			graphics.FillRectangle( bgBrush, numRect );
			using Font font = ResourceImages.Singleton.UseGameFont( numRect.Height );
			graphics.DrawStringCenter( $"T-{remaining}", font, Brushes.Black, numRect );
		}

		if(isAnOption) {
			// Draw Label
			if(_pickPowerCardDecision != null) {
				var images = ResourceImages.Singleton;
				Rectangle labelRect = _layout.GetCardActionLabel( index );
				using Image icon = _pickPowerCardDecision.Use( card ) switch {
					CardUse.AddToHand => images.GetImage( Img.GainCard ),
					CardUse.Discard => images.GetImage( Img.Deck_Discarded ),
					CardUse.Forget => images.GetNoSymbol(),
					CardUse.Play => images.GetImage( Img.Icon_Play ),
					CardUse.Impend => images.GetImage( Img.Icon_ImpendingCard ),
					CardUse.Reclaim => images.GetImage( Img.Reclaim1 ),
					CardUse.Gift => null,
					CardUse.Other => null,
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
		if(_layout == null || !HasFocus) return null;

		// Cards
		for(int i = 0; i < _currentDeck.Cards.Count; ++i)
			if(_layout.GetCardRect( i ).Contains( coords )) {
				PowerCard card = _currentDeck.Cards[i];
				return _options.Contains( card )
					? (() => _ctx.SelectOption( card ))
					: null;
			}

		return null;
	}

	virtual public void ActivateOptions( IDecision decision ) {

		_pickPowerCardDecision = decision as Select.APowerCard; // capture so we can display card-action

		_options = _pickPowerCardDecision != null
			? new HashSet<PowerCard>( _pickPowerCardDecision.CardOptions )
			: new HashSet<PowerCard>( decision.Options.OfType<PowerCard>() ); // ??? when is this used

		OptionCount = _currentDeck.Cards.Intersect( _options ).Count();

		// Track which Deck/Tabs have options
		// HasOption = 0 < OptionCount;
	}

	public int OptionCount { get; private set; }

	Select.APowerCard _pickPowerCardDecision;
	/// <summary> All Power-Card Options, not just the ones contained in this deck. </summary>
	HashSet<PowerCard> _options;
	Rectangle _bounds;
	CardLayout _layout;


	// Spirit Settings
	readonly SpiritDeck _currentDeck;
	readonly SharedCtx _ctx;
	readonly CardImageManager _images = new CardImageManager();
	// readonly Action _onAppearanceChanged;
	readonly int _deckIndex;
}

