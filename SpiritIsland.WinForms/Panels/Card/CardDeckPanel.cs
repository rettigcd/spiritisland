using SpiritIsland.NatureIncarnate;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

class CardDeckPanel : IPanel {

	public Rectangle Bounds {
		get => _bounds;
		set { _bounds = value; }
	}

	public bool HasFocus { set; private get; }

	#region constructor

	public CardDeckPanel( SharedCtx ctx, Control _, int deckIndex, Color bgColor ) {
		_ctx = ctx;
		_deckIndex = deckIndex;
		_currentDeck = _ctx._spirit.Decks[deckIndex];
		// _onAppearanceChanged = parentControl.Invalidate;
		_bgColor = bgColor;
	}

	#endregion constructor

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

	public void OnGameLayoutChanged() {
		_cardRects = null;
	}

	#region Paint

	public void Paint( Graphics graphics ) {
		if(_bounds.Width == 0 || _bounds.Height == 0 || _currentDeck.Cards.Count == 0) return;

		// Cards
		_cardRects = [.._currentDeck.Cards.Select(CardRect)];

		PoolRect cardRow = new RowRect(FillFrom.Left, 
			_cardRects
		){
			Background = Color.FromArgb( 200, _bgColor ),
		}
			.FloatSelf_WithRatio();

		// Elements
		if(_pickPowerCardDecision!=null && _pickPowerCardDecision.Use(_pickPowerCardDecision.CardOptions[0]) == CardUse.Play ){
			CountDictionary<Element> elements = _ctx._spirit.Elements.Elements.Clone();
			foreach(var selected in _selected)
				elements.AddRange(selected.Elements);
			cardRow.Float(SpiritPanel.ActivatedElementsRect(elements),  0,-10,100,10);
//			cardRow.Float(new TextRect(ElementStrings.BuildElementString( elements )){ Background="White", Font=.7f}, 0,-10,100,10);
		}

		cardRow.Paint(graphics,_bounds.FitBoth(cardRow.WidthRatio!.Value,Align.Near));

	}

	CardRectClass CardRect( PowerCard card ) {

		// Card
		CardRectClass cardRect = new CardRectClass{ WidthRatio=5f/7f, Margin=.03f };
		cardRect.Float(new ImgRect( card ),2,2,96,96);

		// Add: Impending Energy
		if(_ctx._spirit is DancesUpEarthquakes due && due.Impending.Contains(card)) {
			int remaining = Math.Max(0,card.Cost - due.ImpendingEnergy[card.Title]);
			cardRect.Float( 
				new TextRect($"T-{remaining}" ){ Font=.9f, Background="#FFFFFF80", Padding=(.2f,.3f) }, 
				5,50,90,45
			);
		}

		// Add: Selectable option
		if( IsOption( card ) ){
			bool isSelected = IsSelected(card);

			// Border
			var cardImg = (ImgRect)cardRect.Children[0].Paintable;
			if( isSelected ) cardImg.Border = "Green;.015";
			else if( CanSelect(card) ) cardImg.Border = "Red;.015";

			// Icon
			CardUse use = _pickPowerCardDecision != null ? _pickPowerCardDecision.Use(card) : CardUse.Accept;
			var accept = new PoolRect{WidthRatio=3}
				.Float( new RectRect{ Fill = isSelected ? "Green" : "Gray", Stroke = "Black;.01" }.RoundCorners(.25f) )
				.Float( new TextRect( use.ToString() ){ Font=.7f, Brush=isSelected ? Brushes.White : Brushes.DarkGray }, 30,0,70,100 );
			accept.Float( new ImgRect( GetImgForCardUse( use ) ), 0,0,30,100 );

			cardRect.Float(accept, x:68,w:35, y:-2,h:32, horizontal:Align.Default, vertical:Align.Near );
			cardRect.Accept = accept;
		}

		return cardRect;
	}

	class CardRectClass : PoolRect {
		public PoolRect Accept {get; set;}
	}

	static Img GetImgForCardUse( CardUse use ){
		return use switch {
			CardUse.AddToHand => Img.GainCard,
			CardUse.Discard   => Img.Discard1,
			CardUse.Forget    => Img.NoX,
			CardUse.Play      => Img.Icon_Play,
			CardUse.Impend    => Img.ImpendingCard,
			CardUse.Reclaim   => Img.Reclaim1,
//			CardUse.Gift      => Img.None,
//			CardUse.Other     => Img.None,
//			CardUse.Repeat    => Img.None,
			_ => Img.None,
		};
	}


	#endregion Paint 

	public IClickable GetClickableAction( Point coords ) {
		if(_cardRects == null || !HasFocus) return null;

		for(int i = 0; i < _currentDeck.Cards.Count; ++i){
			CardRectClass cardRect = _cardRects[i];
			PowerCard card = _currentDeck.Cards[i];

			// Accept 1
			if( cardRect.Accept is not null && cardRect.Accept.Bounds.Contains(coords) ){
				return _selected.Contains(card)
					? new GenericClickable( () => {
						_selected.Remove(card);
						_ctx.SelectOption(card);
					})
					: (IClickable)null;
			}

			// Toggle
			if( cardRect.Bounds.Contains( coords ) && CanToggle(card) )
				return new GenericClickable( () => ToggleCard( card ) );

		}

		return null;
	}

	void SubmitSelected(){
		var card = _selected[0];
		_selected.RemoveAt(0);
		_ctx.SelectOption(card);
	}

	void ToggleCard(PowerCard card){
		if(_selected.Remove( card )) {
			_ctx.Invalidate();
		} else if(_selected.Count < _maxToSelect) {
			_selected.Add(card);
			_ctx.Invalidate();
		}

	}

	virtual public void ActivateOptions( IDecision decision ) {

		if(decision is A.PowerCard card){
			_pickPowerCardDecision = card;
			_options = new HashSet<PowerCard>( _pickPowerCardDecision.CardOptions );
			_maxToSelect = _pickPowerCardDecision.NumberToSelect;
			_maxEnergy = _ctx._spirit.Energy;
		} else {
			_pickPowerCardDecision = null;
			_options = [ ..decision.Options.OfType<PowerCard>() ]; // ??? when is this used
			_maxToSelect = 1;
			_maxEnergy = int.MaxValue;
		}

		if(0<_selected.Count && _options.Contains(_selected[0])){
			SubmitSelected();
			return;
		}

		_selected.Clear();

		OptionCount = _currentDeck.Cards.Intersect( _options ).Count();

		// Track which Deck/Tabs have options
		// HasOption = 0 < OptionCount;
	}

	public int OptionCount { get; private set; }

	// ------------------------------
	// Selected Toggle Predicates
	// ------------------------------

	bool CanToggle(PowerCard card) => CanSelect(card) || IsSelected(card);

	bool CanSelect(PowerCard card) => _selected.Count < _maxToSelect
		&& IsOption( card )
		&& _selected.Sum(c=>c.Cost) + card.Cost <= _maxEnergy;

	bool IsOption( PowerCard card ) => _options != null && _options.Contains( card );

	bool IsSelected(PowerCard card) => _selected.Contains(card);

	int _maxToSelect = 1;
	int _maxEnergy = 0;


	A.PowerCard _pickPowerCardDecision;
	/// <summary> All Power-Card Options, not just the ones contained in this deck. </summary>
	HashSet<PowerCard> _options;
	Rectangle _bounds;

	CardRectClass[] _cardRects;

	// Spirit Settings
	readonly SpiritDeck _currentDeck;
	readonly SharedCtx _ctx;
	readonly Color _bgColor;
	readonly List<PowerCard> _selected = [];
	readonly int _deckIndex;
}

