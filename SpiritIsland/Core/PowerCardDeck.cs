namespace SpiritIsland;

public sealed class PowerCardDeck : IHaveMemento {

	#region constructor

	public PowerCardDeck(IList<PowerCard> cards, int seed, PowerType powerType = default) {
		PowerType = powerType;
 		this._randomizer = new Random(seed);
			
		var temp = cards.ToArray();
		_randomizer.Shuffle(temp);

		this._cards = new Stack<PowerCard>(temp);
		_discards = new List<PowerCard>();
	}

	#endregion constructor

	public PowerType PowerType { get; }

	public List<PowerCard> Flip( int count ) {
		var flipped = new List<PowerCard>();
		for(int i = 0; i < count; ++i) 
			flipped.Add( FlipNext() );
		return flipped;
	}

	public PowerCard FlipNext() {
		if(_cards.Count == 0)
			ReshuffleDiscardDeck();
		var next = _cards.Pop();
		return next;
	}

	/// <summary> Returns cards to the Deck's discard pile </summary>
	public void Discard( IEnumerable<PowerCard> discards ) => _discards.AddRange(discards);

	#region Save/Restore

	public object Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	class MyMemento {
		public MyMemento(PowerCardDeck src) {
			cards = src._cards.ToArray();
			discards = src._discards.ToArray();
		}
		public void Restore( PowerCardDeck src ) {
			src._cards.SetItems( cards );
			src._discards.SetItems(discards);
		}
		readonly PowerCard[] cards;
		readonly PowerCard[] discards;
	}

	#endregion

	#region private

	void ReshuffleDiscardDeck() {
		_randomizer.Shuffle(_discards);
		foreach(var card in _discards) _cards.Push(card);
		_discards.Clear();
	}

	readonly Random _randomizer;
	readonly Stack<PowerCard> _cards = new Stack<PowerCard>();
	readonly List<PowerCard> _discards;

	#endregion
}
