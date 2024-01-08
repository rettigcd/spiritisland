namespace SpiritIsland;

public class InvaderDeck : IHaveMemento{

	#region constructors

	public InvaderDeck( List<InvaderCard> cards, Queue<InvaderCard>[] unused ) {
		_unrevealedCards = cards;
		_unused = unused;
		InitNumberOfCardsToDraw();
		ActiveSlots = new List<InvaderSlot> { Ravage, Build, Explore };
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

	public InvaderCard TakeNextUnused( int selectionLevel ) => _unused[selectionLevel - 1].Dequeue();

	public List<InvaderSlot> ActiveSlots {
		get {  return _activeSlots; }
		set { _activeSlots = value; }
	}

	public List<InvaderCard> Discards {get;} = new List<InvaderCard>();

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
			await unrevealedCard.OnReveal(GameState.Current);
		}
	}

	#region private fields

	readonly Queue<InvaderCard>[] _unused;
	readonly List<InvaderCard> _unrevealedCards;
	readonly List<int> _drawCount = new List<int>(); // tracks how many cards to draw each turn
	List<InvaderSlot> _activeSlots;

	#endregion

	#region Memento

	public virtual object Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	protected class MyMemento {
		public MyMemento(InvaderDeck src) {
			_unrevealedCards = src.UnrevealedCards.ToArray();
			_drawCount = src._drawCount.ToArray();
			_discards = src.Discards.ToArray();
			_slots = src.ActiveSlots.ToArray();

			_mementos.Save( src._activeSlots );
		}
		public void Restore(InvaderDeck src ) {
			src.UnrevealedCards.SetItems(_unrevealedCards);
			src._drawCount.SetItems(_drawCount);
			src.ActiveSlots.SetItems( _slots );
			src.Discards.SetItems( _discards );

			_mementos.Restore();
		}
		readonly Dictionary<IHaveMemento, object> _mementos = new Dictionary<IHaveMemento, object>();
		readonly InvaderCard[] _unrevealedCards;
		readonly InvaderSlot[] _slots;
		readonly int[] _drawCount;
		readonly InvaderCard[] _discards;

	}

	#endregion

}
