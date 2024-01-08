namespace SpiritIsland;

/// <remarks>
/// Not an engine because it contains games state.
/// So it is ok to hold a GameState instance.
/// </remarks>
public class Fear : IRunWhenTimePasses, IHaveMemento {

	public Fear(GameState gs ) {
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

	public int[] cardsPerLevel = new int[] { 3, 3, 3 };

	public int TerrorLevel {
		get {
			int level3Count = cardsPerLevel[2];
			int level2Count = cardsPerLevel[1];
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
				int cardsFrom3 = Math.Min( cardsPerLevel[index], remaining );
				cardCounts.Add( cardsFrom3 );
				remaining -= cardsFrom3;
				--index;
			}
			cardCounts.Reverse();
			return cardCounts.ToArray();
		}
	}

	public void AddDirect( FearArgs args ) {
		if(args.Count==0) return;
		EarnedFear += args.Count;
		while(PoolMax <= EarnedFear && Deck.Any() ) {
			EarnedFear -= PoolMax;

			Deck.Peek().Activate( _gs );
		}
		// ! Do NOT check for victory here and throw GameOverException(...)
		// This is called inside PowerCard using Invoke() which converts exception to a TargetInvocationException which we don't want.
		// Let the post-Action check catch the victory.

		FearAdded?.Invoke( _gs, args );
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
			foreach(Spirit spirit in _gs.Spirits)
				await FlipFearCard(fearCard,true);

			await (TerrorLevel switch {
				1 => fearCard.Level1( _gs ),
				2 => fearCard.Level2( _gs ),
				3 => fearCard.Level3( _gs ),
				_ => throw new ArgumentOutOfRangeException(),
			});

			++ResolvedCards; // record discard cards (for England-6)
		}
	}

	public async Task FlipFearCard( IFearCard cardToFlip, bool activating = false ) {
		string label = activating ? "Activating Fear" : "Done";

		cardToFlip.Flipped = true;
		await AllSpirits.Acknowledge( label, cardToFlip.Text, cardToFlip );

		// Log
		if(cardToFlip.ActivatedTerrorLevel.HasValue)
			// Show description of Activated Level
			_gs.Log( new Log.Debug( $"{cardToFlip.ActivatedTerrorLevel.Value} => {cardToFlip.GetDescription( cardToFlip.ActivatedTerrorLevel.Value )}" ) );
		else
			// Show all Levels
			for(int i = 1; i <= 3; ++i)
				_gs.Log( new Log.Debug( $"{i} => {cardToFlip.GetDescription( i )}" ) );

	}

	public int ResolvedCards { get; private set; }

	// - ints -
	public int EarnedFear { get; private set; } = 0;
	public int PoolMax { get; set; }

	// - cards -
	public readonly Stack<IFearCard> Deck = new Stack<IFearCard>();
	public readonly Stack<IFearCard> ActivatedCards = new Stack<IFearCard>();
	// - events -
	public SyncEvent<FearArgs> FearAdded = new SyncEvent<FearArgs>();                     // Dread Apparations
	readonly GameState _gs;

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	Task IRunWhenTimePasses.TimePasses( GameState gameState ) {
		return FearAdded.EndOfRound( gameState ); // Clears - End-of-Round event handlers
	}

	#region Memento

	object IHaveMemento.Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	protected class MyMemento {
		public MyMemento(Fear src) {
			_pool = src.EarnedFear;
			_deck = src.Deck.ToArray();
			_activatedCards = src.ActivatedCards.ToArray();
			_flipped = _deck.Union( _activatedCards ).ToDictionary(c=>c,c=>c.Flipped);
		}
		public void Restore(Fear src ) {
			src.EarnedFear = _pool;
			src.Deck.SetItems( _deck );
			src.ActivatedCards.SetItems(_activatedCards);
			foreach(var pair in _flipped)
				pair.Key.Flipped = pair.Value;
		}
		readonly int _pool;
		readonly IFearCard[] _deck;
		readonly IFearCard[] _activatedCards;
		readonly Dictionary<IFearCard, bool> _flipped;
	}

	#endregion Memento

}

public class FearArgs {

	public FearArgs(int count ) { this.Count = count; }

	public readonly int Count;
	public Space space;

	// power cards says +N Fear  =>  false
	// Dream a Thousand Deaths => false   (not actually destroying)
	// Ravaging => true
	// Dread Apparations explicely says it does NOT add defence for destroyed towns / cities
	public bool FromDestroyedInvaders;

}
