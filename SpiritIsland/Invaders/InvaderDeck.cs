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
	public void Advance() {

		var destination = Discards;
		foreach(var slot in ActiveSlots) {
			var cardsToMove = slot.GetCardsToAdvance();
			destination.AddRange( cardsToMove );
			destination = slot.Cards;
		}

		InitExploreSlot();
	}

	public void InitExploreSlot() {
		if(UnrevealedCards.Count == 0) return; // !!! Should this throw a GameOver(Loss) exception?
		int count = _drawCount[0]; _drawCount.RemoveAt( 0 );
		while(0 < count--) {
			var unrevealedCard = UnrevealedCards[0];
			UnrevealedCards.RemoveAt( 0 );
			Explore.Cards.Add( unrevealedCard );
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
			unrevealedCards = src.UnrevealedCards.ToArray();
			drawCount = src._drawCount.ToArray();

			explore = src.Explore.Cards.ToArray();
			build = src.Build.Cards.ToArray();
			ravage = src.Ravage.Cards.ToArray();
			discards = src.Discards.ToArray();
			flipped = explore.Union(build).Union(ravage).Union(discards).ToDictionary(c=>c,c=>c.Flipped);
		}
		public void Restore(InvaderDeck src ) {
			src.UnrevealedCards.SetItems(unrevealedCards);
			src._drawCount.SetItems(drawCount);
			src.Explore.Cards.SetItems(explore);
			src.Build.Cards.SetItems(build);
			src.Ravage.Cards.SetItems(ravage);
			src.Discards.SetItems(discards);
			foreach(var pair in flipped)
				pair.Key.Flipped = pair.Value;

		}
		readonly InvaderCard[] unrevealedCards;
		readonly int[] drawCount;
		readonly InvaderCard[] explore;
		readonly InvaderCard[] build;
		readonly InvaderCard[] ravage;
		readonly InvaderCard[] discards;
		readonly Dictionary<InvaderCard, bool> flipped;

	}

	#endregion

}
