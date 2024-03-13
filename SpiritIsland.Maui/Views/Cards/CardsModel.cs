using System.Collections.ObjectModel;

namespace SpiritIsland.Maui;

public class CardsModel : ObservableModel1 {

	#region Observalbe Properties

	public bool IsVisible { get => GetStruct<bool>(); set => SetProp(value); }

	public ObservableCollection<CardSlotModel> Misc { get; } = [];
	public ObservableCollection<CardSlotModel> Played { get; } = [];
	public ObservableCollection<CardSlotModel> Hand { get; } = [];
	public ObservableCollection<CardSlotModel> Draw { get; } = [];
	public ObservableCollection<CardSlotModel> Discards { get; } = [];

	public CardUse Use {				get => GetStruct<CardUse>();		private set => SetProp(value);	}

	// Play
	public int RemainingEnergy {		get => GetInt();					private set => SetProp(value);	}
	public ElementDictModel Elements {  get => GetProp<ElementDictModel>(); private set => SetProp(value);	}

	public bool IsMisc {				get => GetStruct<bool>(); private set => SetProp(value); }
	public bool IsDiscarding {			get => GetStruct<bool>();			private set => SetProp(value);	}
	public bool IsPlaying {				get => GetStruct<bool>();			private set => SetProp(value);	}
	public bool IsAddingToHand {		get => GetStruct<bool>();			private set => SetProp(value);	}
	public bool IsDrawing {				get => GetStruct<bool>();			private set => SetProp(value);	}

	public bool ButtonIsEnabled {		get => GetStruct<bool>();			private set => SetProp(value);	}
	public string? ButtonText {			get => GetNullableProp<string>();	private set => SetProp(value);	}

	public string? Title {				get => GetNullableProp<string>();	private set => SetProp(value);	}
	public string? Instructions {		get => GetNullableProp<string>();	private set => SetProp(value);	}

	#endregion Observalbe Properties

	public CardsModel( Spirit spirit, IUserPortal userPortal ) {
		_spirit = spirit;
		userPortal.NewWaitingDecision += (obj) => InitSlots(obj as A.PowerCard);

		Elements = new ElementDictModel([]);
//		IsVisible = false;

		Show();
	}

	public void InitSlots(A.PowerCard? powerCardSelection) {

		if(powerCardSelection == null) {
			Show();
			return;
		}

		// setup model
		Use = powerCardSelection.Use(powerCardSelection.CardOptions[0]); // !! Shifting Memories might return a Discard here for a Forget
		int numberToSelect = powerCardSelection.NumberToSelect;
		switch (Use) {

			case CardUse.Play:
				Elements = new ElementDictModel( _spirit.Elements.Elements );
				RemainingEnergy = _spirit.Energy;
				Play(numberToSelect);
				IsPlaying = true;
				IsAddingToHand = false;
				IsDiscarding = false;
				IsMisc = false;
				break;

			case CardUse.AddToHand:
				AddToHand(numberToSelect, "Add");
				IsPlaying = false;
				IsAddingToHand = true;
				IsDiscarding = false;
				IsMisc = false; 
				break;

			case CardUse.Reclaim:
				AddToHand(numberToSelect, "Reclaim");
				IsPlaying = false;
				IsAddingToHand = true;
				IsDiscarding = false;
				IsMisc = false;
				break;

			case CardUse.Discard:
				Discard(numberToSelect);
				IsPlaying = false;
				IsAddingToHand = false;
				IsDiscarding = true;
				IsMisc = false;
				break; 

			case CardUse.Repeat:
				SetupMisc("Repeat", numberToSelect);
				IsPlaying = false;
				IsAddingToHand = false;
				IsDiscarding = false;
				IsMisc = true;
				break;

			default:
				Show();
				IsPlaying = false;
				IsAddingToHand = false;
				break;
		}

		if (powerCardSelection != null) {
			// Detect "Done" in options
			_optionsIncludeDone = powerCardSelection.Options.Any(TextOption.Done.Equals);

			_maxCardToSelect = Math.Min(                                // use minimum so Text.Done doesn't roll over to next action.
				powerCardSelection.NumberToSelect,                      // based on: circumstance
				powerCardSelection.Options.OfType<PowerCard>().Count()  // based on: cards in source
			);

		}

	}

	public IOption[] FindNew() {
		var moved = MovedPowerCards.ToArray();
		List<IOption> options = moved
			.Cast<IOption>()
			.ToList();

		bool includeDone = _optionsIncludeDone
			&& options.Count < _maxCardToSelect;

		if( IsPlaying ) {
			// only include done if there are move cards we can afford
			includeDone &= _spirit.Hand.Except(moved).Any(card => card.Cost <= RemainingEnergy);
		}

		if( includeDone )
			options.Add(TextOption.Done);

		return [..options];

	}

	public event Action<IOption[]>? CardsSelected;

	public void AcceptCards() {
		ResetDetails();
		CardsSelected?.Invoke( FindNew() );
	}

	public void ResetDetails() {
		Title = "";
		Instructions = "";
	}

	#region private methods

	// Action - Play
	void Play(int playCount) {
		InitPlay(playCount);
		InitHand(0,true);
		InitDiscards();
		InitDraw();
		InitSpecial();
		SetAction("Play");
	}

	// Action - AddToHand
	void AddToHand(int count,string actionType) {
		InitPlay();
		InitHand(count);
		InitDiscards(0,actionType=="Reclaim");
		InitDraw(actionType=="Add");
		InitSpecial();
		SetAction(actionType);
	}

	void Discard(int count) {
		InitPlay(0,true);
		InitHand(0,true);
		InitDiscards(count);
		InitDraw();
		InitSpecial();
		SetAction("Discard");
	}

	void SetupMisc(string action, int count=1) {
		InitPlay(0, true);
		InitHand();
		InitDiscards();
		InitDraw();
		InitSpecial(count);
		SetAction(action);
	}

	void SetAction(string action) {
		_action = action;
		ButtonText = $"{action} These";
		UpdateButtonText(0);
	}
	string _action = "";

	// Aciton - None / Show
	void Show() {
		InitPlay();
		InitHand();
		InitDiscards();
		InitDraw();
		ResetDetails();
	}

	#region Init - Deck

	void InitPlay(int emptySlots = 0, bool isDraggable=false) {
		BuildSlots(Played, _spirit.InPlay, emptySlots, isDraggable);
	}

	void InitHand(int emptySlots = 0, bool isDraggable=false) {
		BuildSlots(Hand,_spirit.Hand, emptySlots, isDraggable);
	}

	void InitDiscards(int emptySlots = 0,bool isDraggable=false) {
		BuildSlots(Discards,_spirit.DiscardPile, emptySlots, isDraggable);
	}

	void InitDraw(bool isDraggable=false) {
		IsDrawing = isDraggable;
		BuildSlots(Draw, _spirit.DraftDeck, 0, isDraggable);
	}

	void InitSpecial(int emptySlots = 0) {
		BuildSlots(Misc, [], emptySlots, false);
	}

	#endregion Init - Deck

	void BuildSlots(ObservableCollection<CardSlotModel> collection, List<PowerCard> src, int desiredEmptySlots, bool isDraggable=false) {

		// Find Empty Slots
		var slotsToVacate = collection.Where(slot=>slot.Card is null || !src.Contains(slot.Card.Card)).ToList();

		// Add new Power Cards to the collection
		foreach (var card in src) {
			CardSlotModel? match = collection.FirstOrDefault(slot=>slot.Card is not null && slot.Card.Card == card);
			// update Pre-existing
			if(match is not null) {
				match.Card!.IsDraggable = isDraggable;
				continue;
			}

			var cardModel = new CardModel(card) { IsDraggable = isDraggable };
			// Find place to insert it
			if (1 < slotsToVacate.Count) {
				slotsToVacate[0].Card = cardModel;
				slotsToVacate.RemoveAt(0);
			} else {
				// create a new slot
				var cardSlotModel = new CardSlotModel(cardModel);
				cardSlotModel.PropertyChanged += CardSlot_PropertyChanged;
				collection.Add(cardSlotModel);
			}
		}

		// Reduce the vacate slots to match empty Slots
		while( desiredEmptySlots < slotsToVacate.Count) {
			collection.Remove(slotsToVacate[0]);
			slotsToVacate.RemoveAt(0);
		}
		// remaining vacate slots should be vacated
		foreach(var vacateSlot in slotsToVacate) vacateSlot.Card = null;

		// Add
		int addEmptySlots = desiredEmptySlots - slotsToVacate.Count;
		while(0 < addEmptySlots-- ) {
			var cardSlotModel = new CardSlotModel();
			cardSlotModel.PropertyChanged += CardSlot_PropertyChanged;
			collection.Add(cardSlotModel);
		}

		// track result
		if (desiredEmptySlots != 0) {
			_movedCardDestination = src;
			_movedCardSlots = collection;
		}
	}

	void CardSlot_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
		CardSlotModel cardSlotModel = (CardSlotModel)sender!;
		if( e.PropertyName == nameof(cardSlotModel.Selected) )
			ShowInstructions(cardSlotModel);
		if( e.PropertyName == nameof(cardSlotModel.Card) )
			UpdateMovedUi();
	}

	void UpdateMovedUi() {
		var moved = MovedPowerCards.ToArray();
		bool buttonEnabled = 0 < moved.Length;

		if (IsPlaying) {
			var elements = _spirit.Elements.Elements.Clone();
			foreach(var card in moved)
				elements.AddRange(card.Elements);
			Elements = new ElementDictModel(elements);

			RemainingEnergy = _spirit.Energy - moved.Sum(x => x.Cost);
			buttonEnabled &= 0 <= RemainingEnergy;
		}

		ButtonIsEnabled = buttonEnabled;
		UpdateButtonText( moved.Length );
	}

	void UpdateButtonText( int count) {
		ButtonText = $"{_action} ({count})";
	}

	/// <summary>
	/// Cards Moved into the Destination Deck
	/// </summary>
	IEnumerable<PowerCard> MovedPowerCards => _movedCardSlots
		.Select(c => (c.Card?.Card))
		.Where(x => x is not null)
		.Cast<PowerCard>()
		.Except(_movedCardDestination);

	void ShowInstructions(CardSlotModel cardSlotModel) {
		if (cardSlotModel.Selected && cardSlotModel.Card is not null) {
			// unselect old
			if (_focusCard is not null)
				_focusCard.Selected = false;
			// capture new
			_focusCard = cardSlotModel;
			// show instructions
			Title = _focusCard.Card.Title;
			Instructions = _focusCard.Card.Instructions;
		}
	}

	#endregion private methods

	#region private fields

	CardSlotModel? _focusCard;

	// Collecting the card that will be moved
	ObservableCollection<CardSlotModel> _movedCardSlots = []; // card slots that receive cards
	List<PowerCard> _movedCardDestination = []; // assumes only 1 destination, never 2.

	// For building Select-Option List
	bool _optionsIncludeDone;
	int _maxCardToSelect;

	readonly Spirit _spirit;

	#endregion private fields
}
