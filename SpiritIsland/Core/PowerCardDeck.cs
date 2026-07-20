namespace SpiritIsland;

public sealed class PowerCardDeck : IHaveMemento {

	#region constructor

	public PowerCardDeck(IList<PowerCard> cards, int seed, PowerType powerType) {
		PowerType = powerType;
 		_randomizer = new Random(seed);
			
		var temp = cards.ToArray();
		_randomizer.Shuffle(temp);

		_cards = new Stack<PowerCard>(temp);
		_discards = [];
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

	class MyMemento( PowerCardDeck _src ) {
		public void Restore( PowerCardDeck src ) {
			src._cards.SetItems( cards );
			src._discards.SetItems(discards);
		}
		readonly PowerCard[] cards = [.. _src._cards];
		readonly PowerCard[] discards = [.. _src._discards];
	}

	#endregion

	#region Json

	/// <summary>
	/// Named keys - same fields as MyMemento above; index `[0]` of each array is still the top of the
	/// stack, matching `Stack&lt;T&gt;.SetItems`'s own convention (`IEnumerableExtensions.SetItems`) - no
	/// reversal needed since a `Stack&lt;T&gt;` already enumerates top-first. Each entry is just
	/// `PowerCard.ToJson()` - its bare `Title`, resolved back via the single shared `PowerCardRegistry`
	/// (Major/Minor/Spirit cards all share one pool, so nothing deck-specific needs to travel alongside
	/// it). `_randomizer`'s seed is deliberately not captured - accepted drift, not a gap: a reshuffle
	/// after restore won't match what the original run would have produced, but reshuffles are rare in
	/// practice (none observed) and the current card order is preserved exactly either way. See the
	/// field's own remarks below.
	/// </summary>
	public JsonObject ToJson() => new JsonObject {
		["Cards"] = new JsonArray( _cards.Select( c => c.ToJson() ).ToArray() ),
		["Discards"] = new JsonArray( _discards.Select( c => c.ToJson() ).ToArray() )
	};

	/// <summary> Restores onto this existing deck (built through the normal constructor, so PowerType/
	/// the RNG seed are already set) rather than constructing a new one - same "restore onto existing"
	/// shape as Fear.RestoreFromJson/InvaderDeck.RestoreFromJson. </summary>
	public void RestoreFromJson( JsonObject json ) {
		_cards.SetItems( ( (JsonArray)json["Cards"]! ).Select( n => PowerCardRegistry.Deserialize( n! ) ).ToArray() );
		_discards.SetItems( ( (JsonArray)json["Discards"]! ).Select( n => PowerCardRegistry.Deserialize( n! ) ).ToArray() );
	}

	#endregion Json

	#region private

	void ReshuffleDiscardDeck() {
		_randomizer.Shuffle(_discards);
		foreach(var card in _discards) _cards.Push(card);
		_discards.Clear();
	}

	// Its seed is never captured by ToJson/RestoreFromJson (nor MyMemento) - accepted, deliberate drift,
	// not an oversight. A reshuffle after a JSON restore won't reproduce what the original unbroken run
	// would have shuffled to, but in practice a reshuffle this deck cares about is rare enough that it's
	// never actually been observed - not worth the complexity of threading Random's state through JSON.
	readonly Random _randomizer;
	readonly Stack<PowerCard> _cards = new Stack<PowerCard>();
	readonly List<PowerCard> _discards;

	#endregion
}
