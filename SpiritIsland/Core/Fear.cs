namespace SpiritIsland;

/// <remarks>
/// Not an engine because it contains games state.
/// So it is ok to hold a GameState instance.
/// </remarks>
public class Fear {

	public Fear(GameState gs ) {
		this.gs = gs;
		this.PoolMax = gs.Spirits.Length * 4;
		gs.TimePasses_WholeGame += FearAdded.EndOfRound;
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
		EarnedFear += args.Count;
		while(PoolMax <= EarnedFear && Deck.Any() ) {
			EarnedFear -= PoolMax;

			Deck.Peek().Activate( gs );
		}
		// ! Do NOT check for victory here and throw GameOverException(...)
		// This is called inside PowerCard using Invoke() which converts exception to a TargetInvocationException which we don't want.
		// Let the post-Action check catch the victory.

		FearAdded?.Invoke( gs, args );
	}

	public async Task Apply() {
		while(ActivatedCards.Count > 0) {
			IFearCard fearCard = ActivatedCards.Pop();

			// show card to each user
			fearCard.ActivatedTerrorLevel = TerrorLevel;

			await using var actionScope = gs.StartAction( ActionCategory.Fear );
			foreach(var spirit in gs.Spirits)
				await spirit.BindSelf().FlipFearCard(fearCard,true);


			var ctx = new GameCtx( gs );
			switch(TerrorLevel) {
				case 1: await fearCard.Level1( ctx ); break;
				case 2: await fearCard.Level2( ctx ); break;
				case 3: await fearCard.Level3( ctx ); break;
			}

			++ResolvedCards; // record discard cards (for England-6)
		}
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
	readonly GameState gs;

	#region Memento

	public virtual IMemento<Fear> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<Fear> memento ) => ((Memento)memento).Restore(this);

	protected class Memento : IMemento<Fear> {
		public Memento(Fear src) {
			pool = src.EarnedFear;
			deck = src.Deck.ToArray();
			activatedCards = src.ActivatedCards.ToArray();
			flipped = deck.Union( activatedCards ).ToDictionary(c=>c,c=>c.Flipped);
		}
		public void Restore(Fear src ) {
			src.EarnedFear = pool;
			src.Deck.SetItems( deck );
			src.ActivatedCards.SetItems(activatedCards);
			foreach(var pair in flipped)
				pair.Key.Flipped = pair.Value;
		}
		readonly int pool;
		readonly IFearCard[] deck;
		readonly IFearCard[] activatedCards;
		readonly Dictionary<IFearCard, bool> flipped;
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
