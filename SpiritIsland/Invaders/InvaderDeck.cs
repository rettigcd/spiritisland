namespace SpiritIsland;

public class InvaderDeck : IHaveMemento {

	#region constructors

	/// <summary>
	/// 
	/// </summary>
	/// <param name="cards">Prepared cards for the game.</param>
	/// <param name="leftOver">Remaining cards that were not part of the game.</param>
	public InvaderDeck( List<InvaderCard> cards, Queue<InvaderCard>[] leftOver ) {
		_unrevealedCards = cards;
		_leftOverCards = leftOver;
		InitNumberOfCardsToDraw();
		ActiveSlots = [ Ravage, Build, Explore ];
	}

	void InitNumberOfCardsToDraw() {
		// Setup draw: 1 card at a time.
		for(int i = 0; i < UnrevealedCards.Count; ++i) 
			_drawCount.Add( 1 );
	}

	#endregion

	public List<InvaderCard> UnrevealedCards => _unrevealedCards;
	// ??? Should Unused, Unrevealed, and Discard be slots also?  (Just not in the ActiveSlots) list
	public ExploreSlot Explore { get; } = new ExploreSlot();
	public BuildSlot Build { get; } = new BuildSlot();
	public RavageSlot Ravage { get; } = new RavageSlot();

	public InvaderCard TakeNextUnused( int selectionLevel ) => _leftOverCards[selectionLevel - 1].Dequeue();

	public List<InvaderSlot> ActiveSlots {
		get {  return _activeSlots; }
		set { _activeSlots = value; }
	}

	public List<InvaderCard> Discards {get;} = [];

	public int InvaderStage => (Explore.Cards.FirstOrDefault() ?? UnrevealedCards.First()).InvaderStage;

	/// <summary>
	/// Triggers Ravage / 
	/// </summary>
	public async Task AdvanceAsync() {

		var destination = Discards;
		foreach(var slot in ActiveSlots) {
			var cardsToMove = slot.GetCardsToAdvance();
			destination.AddRange( cardsToMove );
			destination = slot.Cards;
		}

		await InitExploreSlotAsync();
	}

	public async Task InitExploreSlotAsync() {
		if(UnrevealedCards.Count == 0) return; // !!! Should this throw a GameOver(Loss) exception?
		int count = _drawCount[0]; _drawCount.RemoveAt( 0 );
		while(0 < count--) {
			var unrevealedCard = UnrevealedCards[0];
			UnrevealedCards.RemoveAt( 0 );
			Explore.Cards.Add( unrevealedCard );
			await unrevealedCard.OnReveal();
		}
	}

	#region private fields

	List<InvaderSlot> _activeSlots;
	readonly List<InvaderCard> _unrevealedCards;
	readonly List<int> _drawCount = []; // tracks how many cards to draw each turn
	readonly Queue<InvaderCard>[] _leftOverCards; // cards not drawn and made part of the deck.  (Russia only)

	#endregion

	#region Memento

	public virtual object Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	protected class MyMemento {
		public MyMemento(InvaderDeck src) {
			// Explore/Build/Ravage
			_slots = [..src.ActiveSlots];
			_mementos.SaveMany( src._activeSlots );

			// Future / Past cards
			_unrevealedCards = [..src.UnrevealedCards];
			_discards = [..src.Discards];
			_drawCount = [..src._drawCount];
		}
		public void Restore(InvaderDeck src ) {
			src.UnrevealedCards.SetItems(_unrevealedCards);
			src._drawCount.SetItems(_drawCount);
			src.ActiveSlots.SetItems( _slots );
			src.Discards.SetItems( _discards );

			_mementos.Restore();
		}
		readonly Dictionary<IHaveMemento, object> _mementos = [];
		readonly InvaderSlot[] _slots;
		readonly InvaderCard[] _unrevealedCards;
		readonly InvaderCard[] _discards;
		readonly int[] _drawCount;

	}

	#endregion

}
