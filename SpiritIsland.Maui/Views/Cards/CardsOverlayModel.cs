//using Foundation;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SpiritIsland.Maui;

public class CardsOverlayModel : ObservableModel1 {

	#region Observalbe Properties

	public ObservableCollection<CardSlotModel> Resolved { get; } = [];
	public ObservableCollection<CardSlotModel> Played { get; } = [];
	public ObservableCollection<CardSlotModel> Hand { get; } = [];
	public ObservableCollection<CardSlotModel> Draw { get; } = []; // only visible when drawing cards
	public ObservableCollection<CardSlotModel> Discards { get; } = [];
	public ObservableCollection<CardSlotModel> Forgotten { get; } = [];

	public CardUse Use {				get => GetStruct<CardUse>();		private set => SetProp(value);	}

	// Play
	public int RemainingEnergy {		get => GetInt();					private set => SetProp(value);	}
	public ElementModel[]   Elements {  get => GetProp<ElementModel[]>(); private set => SetProp(value);	}
	public string InnateSummary { get => GetProp<string>(); private set => SetProp(value); }

	public bool IsRepeating {			get => GetStruct<bool>();			private set => SetProp(value);	}
	public bool IsDiscarding {			get => GetStruct<bool>();			private set => SetProp(value);	}
	public bool IsPlaying {				get => GetStruct<bool>();			private set => SetProp(value);	}
	public bool IsAddingToHand {		get => GetStruct<bool>();			private set => SetProp(value);	}
	public bool IsDrawing {				get => GetStruct<bool>();			private set => SetProp(value);	}
	public bool IsForgetting {			get => GetStruct<bool>();			private set => SetProp(value); }

	public bool ButtonIsEnabled {		get => GetStruct<bool>();			private set => SetProp(value);	}
	public string? ButtonText {			get => GetNullableProp<string>();	private set => SetProp(value);	}

	public string? Title {				get => GetNullableProp<string>();	private set => SetProp(value);	}
	public string? Instructions {		get => GetNullableProp<string>();	private set => SetProp(value);	}

	#endregion Observalbe Properties

	public event Action<IOption[]>? CardsSelected;

	public event Action? RequestClose;

	public ICommand AcceptCardsCommand { get; }
	public ICommand CloseCommand { get; }

	#region constructor

	public CardsOverlayModel( Spirit spirit, IDecisionPortal userPortal ) {
		_spirit = spirit;
		_innates = [..spirit.InnatePowers.Select(ip=>new InnateStatus(ip,_spirit))];
		userPortal.NewWaitingDecision += (obj) => InitSlotsForNewDecision(obj as A.PowerCard);
		Elements = [];
		InnateSummary = "aa";
		// InitStuff(); Show();

		CloseCommand = new Command( TryToClose );
		AcceptCardsCommand = new Command( AcceptCards );
	}

	readonly InnateStatus[] _innates;

	#endregion constructor

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

	void AcceptCards() {
		CardsSelected?.Invoke(FindNew());
		TryToClose();
	}

	void TryToClose() {
		ResetDetails();
		RequestClose?.Invoke();
	}

	#region private methods

	PowerCard[] _options = [];

	int _numberToSelect = 0;
	readonly CountDictionary<Destination> _destinations = [];

	enum Destination { None, Resolved, Play, Hand, Draw, Discard, Forgotten }

	/// <summary>
	/// Updates the cards in each slot and their draggability.
	/// Called each time a NewDecision comes in.
	/// </summary>
	/// <param name="powerCardSelection"></param>
	void InitSlotsForNewDecision(A.PowerCard? powerCardSelection) {

		_destinations.Clear();

		if( powerCardSelection == null) {
			_options = [];
			_numberToSelect = 0;
			InitAllSections();
			ResetDetails();
			UpdateElements();
			SetAction("---");
			return;
		}

		// save options, destinations, and num-to-select
		_options = powerCardSelection.CardOptions;
		foreach(var card in _options )
			AddDestinationCount(powerCardSelection.Use(card));
		_numberToSelect = powerCardSelection.NumberToSelect;

		// setup model
		Use = powerCardSelection.Use(_options[0]); // !! Shifting Memories might return a Discard here for a Forget
		InitAllSections();
		switch( Use ){

			case CardUse.Play:
				Elements = ElementModel.FromDict( _spirit.Elements.Elements );
				RemainingEnergy = _spirit.Energy;
				SetAction("Play", playing: true); break;

			case CardUse.AddToHand: SetAction("Add", addingToHand: true, isDrawing: true); break;
			case CardUse.Reclaim:   SetAction("Reclaim", addingToHand:true, isDrawing: false); break;
			case CardUse.Discard:   SetAction("Discard", discarding: true); break;
			case CardUse.Forget:	SetAction("Forget", discarding: false); break;
			case CardUse.Repeat:    SetAction("Repeat", repeating: true); break;
			default: ResetDetails(); break;
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

	void AddDestinationCount(CardUse cardUse) {
		var destination = cardUse switch {
			CardUse.Play => Destination.Play,
			CardUse.Repeat => Destination.Play,
			CardUse.AddToHand => Destination.Hand,
			CardUse.Reclaim => Destination.Hand,
			CardUse.Discard => Destination.Discard,
			CardUse.Forget => Destination.Forgotten,
			//				CardUse.Gift,
			//				CardUse.Impend,
			//				CardUse.Accept   // Generic card use. (was "Other")
			_ => Destination.None,
		};
		_destinations[destination]++;
	}

	void InitAllSections() {
		PowerCard[] resolvedCards = _spirit.UsedActions.OfType<PowerCard>().ToArray();
		var unresolved = _spirit.InPlay.Except(resolvedCards);

		BuildSlots(Resolved, resolvedCards, Destination.Resolved);
		BuildSlots(Played, unresolved, Destination.Play);
		BuildSlots(Hand, _spirit.Hand, Destination.Hand); 
		BuildSlots(Discards,_spirit.DiscardPile, Destination.Discard);
		BuildSlots(Draw, _spirit.DraftDeck, Destination.Draw );
		BuildSlots(Forgotten, [], Destination.Forgotten);
	}


	void SetAction(string action, 
		bool playing = false,
		bool addingToHand = false,
		bool discarding = false,
		bool repeating = false,
		bool isDrawing = false
	) {
		_action = action;
		UpdateButtonTextCount(0);

		IsPlaying = playing;
		IsAddingToHand = addingToHand;
		IsDiscarding = discarding;
		IsRepeating = repeating;
		IsDrawing = isDrawing;
		IsForgetting = _destinations[Destination.Forgotten] != 0;

		ButtonIsEnabled = false; // at least initially it is.
	}
	string _action = "";

	void ResetDetails() {
		Title = "";
		Instructions = "";
	}

	void BuildSlots(
		ObservableCollection<CardSlotModel> observableCollection, 
		IEnumerable<PowerCard> preExisting, 
		Destination destination
	) {
		int desiredEmptySlots = int.Min( _numberToSelect, _destinations[destination] );

		// Identify Current Empty Slots
		List<CardSlotModel> slotsToVacate = observableCollection
			.Where(
				slot=>slot.Card is null						// no card
				|| !preExisting.Contains(slot.Card.Card)	// card should not be in this list
			) 
			.ToList();

		// === Add new cards ===

		// Add new Power Cards to the collection
		foreach( PowerCard card in preExisting) {
			bool isDraggable = _options.Contains(card);

			// Determine if card is already observable
			CardSlotModel? match = observableCollection.FirstOrDefault(slot=>slot.Card is not null && slot.Card.Card == card);

			// If so, update its Draggable
			if(match is not null) {
				match.Card!.IsDraggable = isDraggable;
				continue;
			}

			// Else: Insert it
			var cardModel = new CardModel(card) { IsDraggable = isDraggable };
			// Find place to insert it
			if( 0 < slotsToVacate.Count ) {
				slotsToVacate[0].Card = cardModel;
				slotsToVacate.RemoveAt(0);
			} else {
				// create a new slot
				var cardSlotModel = new CardSlotModel(cardModel);
				cardSlotModel.PropertyChanged += CardSlot_PropertyChanged;
				observableCollection.Add(cardSlotModel);
			}
		}

		// === Updated Vacated Spots ===

		// Reduce the vacated slots to match empty Slots
		while( desiredEmptySlots < slotsToVacate.Count) {
			observableCollection.Remove(slotsToVacate[0]);
			slotsToVacate.RemoveAt(0);
		}

		// remaining vacated slots should be vacated
		foreach(var vacateSlot in slotsToVacate) vacateSlot.Card = null;

		// Add
		int addEmptySlots = desiredEmptySlots - slotsToVacate.Count;
		while(0 < addEmptySlots-- ) {
			var cardSlotModel = new CardSlotModel();
			cardSlotModel.PropertyChanged += CardSlot_PropertyChanged;
			observableCollection.Add(cardSlotModel);
		}

		// === track result ===
		if( desiredEmptySlots != 0 ) {
			_preExistingDestinationCards = preExisting.ToArray();
			_movedCardSlots = observableCollection;
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
		PowerCard[] moved = MovedPowerCards.ToArray();
		bool buttonEnabled = 0 < moved.Length;

		if (IsPlaying) {
			// Update Elements
			UpdateElements();

			// Update Energy
			RemainingEnergy = _spirit.Energy - moved.Sum(x => x.Cost);

			// Update Button Enabled
			buttonEnabled &= 0 <= RemainingEnergy;
		}

		ButtonIsEnabled = buttonEnabled;
		UpdateButtonTextCount( moved.Length );
	}

	void UpdateElements() {
		CountDictionary<Element> elements = [];

		// build card elements
		foreach (var card in MovedPowerCards)
			elements.AddRange(card.Elements);

		// Calculate Innate summaries by passing CARD ELEMENTS only.
		InnateSummary = _innates.Select(i => {
			try {
				return i.GetStatusString(elements);
			} catch {
				return "BB";
			}
		}).Join(" ");

		// Add in spirit elements to show Total elements.
		elements.AddRange(_spirit.Elements.Elements);
		Elements = ElementModel.FromDict(elements);

	}

	void UpdateButtonTextCount( int count ) {
		ButtonText = $"{_action} ({count})";
	}

	/// <summary>
	/// Cards Moved into the Destination Deck
	/// </summary>
	IEnumerable<PowerCard> MovedPowerCards => _movedCardSlots
		.Select(c => (c.Card?.Card))
		.Where(x => x is not null)
		.Cast<PowerCard>()
		.Except(_preExistingDestinationCards);

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
	PowerCard[] _preExistingDestinationCards = []; // assumes only 1 destination, never 2.

	// For building Select-Option List
	bool _optionsIncludeDone;
	int _maxCardToSelect;

	readonly Spirit _spirit;

	#endregion private fields
}
