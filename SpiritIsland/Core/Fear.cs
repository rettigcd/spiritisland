namespace SpiritIsland;

/// <remarks>
/// Not an engine because it contains games state.
/// So it is ok to hold a GameState instance.
/// </remarks>
public class Fear : IHaveMemento {

	public Fear(GameState gs) {
		_gs = gs;
		PoolMax = gs.Spirits.Length * 4;
		Init();
	}

	public void Init() {
		while(Deck.Count < 9)
			PushOntoDeck( new NullFearCard() );
	}

	public void PushOntoDeck( IFearCard fearCard ) {
		Deck.Push( fearCard );
	}

	public int[] CardsPerLevel = new int[] { 3, 3, 3 }; // only adjusted during Setup - doesn't need saved to memento

	public int TerrorLevel {
		get {
			int level3Count = CardsPerLevel[2];
			int level2Count = CardsPerLevel[1];
			int ct = Deck.Count;
			return (ct > level2Count + level3Count) ? 1
				: ct > level3Count ? 2
				: ct > 0 ? 3
				: 4; // Victory
		}
	}

	// This returns lowest Terror Level 1st
	// When some missing, does not return that Terror Level
	public int[] CardsPerLevelRemaining {
		get {
			var cardCounts = new List<int>();

			int remaining = Deck.Count;
			int index = 2;

			while( 0 < remaining) {
				int cardsFrom3 = Math.Min( CardsPerLevel[index], remaining );
				cardCounts.Add( cardsFrom3 );
				remaining -= cardsFrom3;
				--index;
			}
			cardCounts.Reverse();
			return cardCounts.ToArray();
		}
	}

	public void AddOnSpace( SpaceState tokens, int count, FearType fearType ) {
		if(count == 0) return;

		Add( count );

		var mods = tokens.Keys.OfType<IReactToLandFear>().ToArray();
		foreach(IReactToLandFear mod in mods)
			mod.HandleFearAdded( tokens, count, fearType );
	}

	public void Add( int count ) {
		if(count == 0) return;

		EarnedFear += count;
		while(PoolMax <= EarnedFear && Deck.Any()) {
			EarnedFear -= PoolMax;

			Deck.Peek().Activate( _gs );
		}
		// ! Do NOT check for victory here and throw GameOverException(...)
		// This is called inside PowerCard using Invoke() which converts exception to a TargetInvocationException which we don't want.
		// Let the post-Action check catch the victory.
	}

	/// <summary>
	/// Flips (resolves) all ActivatedCards
	/// </summary>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public async Task Apply() {
		while(0 < ActivatedCards.Count) {
			IFearCard fearCard = ActivatedCards.Pop();

			// show card to each user
			fearCard.ActivatedTerrorLevel = TerrorLevel;

			await using var actionScope = await ActionScope.Start(ActionCategory.Fear);
			await FlipFearCard( fearCard, true );

			await (TerrorLevel switch {
				1 => fearCard.Level1( _gs ),
				2 => fearCard.Level2( _gs ),
				3 => fearCard.Level3( _gs ),
				_ => throw new ArgumentOutOfRangeException(),
			});

			++ResolvedCardCount; // record discard cards (for England-6)
		}
	}

#pragma warning disable CA1822 // Mark members as static
	public async Task FlipFearCard( IFearCard cardToFlip, bool activating = false ) {
		string label = activating ? "Activating Fear" : "Done";

		cardToFlip.Flipped = true;
		await AllSpirits.Acknowledge( label, cardToFlip.Text, cardToFlip );

		// Log
		if(cardToFlip.ActivatedTerrorLevel.HasValue)
			// Show description of Activated Level
			ActionScope.Current.Log( new Log.Debug( $"{cardToFlip.ActivatedTerrorLevel.Value} => {cardToFlip.GetDescription( cardToFlip.ActivatedTerrorLevel.Value )}" ) );
		else
			// Show all Levels
			for(int i = 1; i <= 3; ++i)
				ActionScope.Current.Log( new Log.Debug( $"{i} => {cardToFlip.GetDescription( i )}" ) );

	}
#pragma warning restore CA1822 // Mark members as static

	public int ResolvedCardCount { get; private set; }

	// - ints -
	public int EarnedFear { get; private set; } = 0;
	public int PoolMax { get; set; }

	// - cards -
	public readonly Stack<IFearCard> Deck = new Stack<IFearCard>();
	public readonly Stack<IFearCard> ActivatedCards = new Stack<IFearCard>();
	// - events -
	readonly GameState _gs;

	#region Memento

	object IHaveMemento.Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	protected class MyMemento {
		public MyMemento(Fear src) {
			_earnedFear = src.EarnedFear;
			_poolMax = src.PoolMax;
			_resolvedCardCount = src.ResolvedCardCount;
			_deck = src.Deck.ToArray();
			_activatedCards = src.ActivatedCards.ToArray();
			_flipped = _deck.Union( _activatedCards ).ToDictionary(c=>c,c=>c.Flipped);
		}
		public void Restore(Fear src ) {
			src.EarnedFear = _earnedFear;
			src.PoolMax = _poolMax;
			src.ResolvedCardCount = _resolvedCardCount;
			src.Deck.SetItems( _deck );
			src.ActivatedCards.SetItems(_activatedCards);
			foreach(var pair in _flipped)
				pair.Key.Flipped = pair.Value;
		}
		readonly int _earnedFear;
		readonly int _poolMax;
		readonly int _resolvedCardCount;
		readonly IFearCard[] _deck;
		readonly IFearCard[] _activatedCards;
		readonly Dictionary<IFearCard, bool> _flipped;
	}

	#endregion Memento

}


public enum FearType { Direct, FromInvaderDestruction }