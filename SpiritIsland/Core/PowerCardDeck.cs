namespace SpiritIsland;

public class PowerCardDeck {

	readonly Random randomizer;

	public PowerCardDeck(IList<PowerCard> cards, int seed) {
 		this.randomizer = new Random(seed);
			
		var temp = cards.ToArray();
		randomizer.Shuffle(temp);

		this.cards = new Stack<PowerCard>(temp);
		discards = new List<PowerCard>();
	}

	public PowerCard FlipNext() {
		if(cards.Count == 0)
			ReshuffleDiscardDeck();
		var next = cards.Pop();
		return next;
	}

	public List<PowerCard> Flip( int count ) {
		var flipped = new List<PowerCard>();
		for(int i = 0; i < count; ++i) flipped.Add( FlipNext() );
		return flipped;
	}

	/// <summary> Puts cards in the Deck's discard pile </summary>
	public void Discard(IEnumerable<PowerCard> discards) => this.discards.AddRange(discards);

	void ReshuffleDiscardDeck() {
		randomizer.Shuffle(discards);
		foreach(var card in discards) cards.Push(card);
		discards.Clear();
	}

	readonly Stack<PowerCard> cards = new Stack<PowerCard>();
	readonly List<PowerCard> discards;

	public virtual IMemento<PowerCardDeck> SaveToMemento() => new Memento(this);
	public virtual void RestoreFrom( IMemento<PowerCardDeck> memento ) => ((Memento)memento).Restore(this);

	protected class Memento : IMemento<PowerCardDeck> {
		public Memento(PowerCardDeck src) {
			cards = src.cards.ToArray();
			discards = src.discards.ToArray();
		}
		public void Restore( PowerCardDeck src ) {
			src.cards.SetItems( cards );
			src.discards.SetItems(discards);
		}
		readonly PowerCard[] cards;
		readonly PowerCard[] discards;
	}

}
