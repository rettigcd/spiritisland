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
			AddCard( new NullFearCard() );
	}

	public void AddCard( IFearOptions fearCard ) {
		var td = new PositionFearCard { FearOptions = fearCard, Text = "!!!FixMe" };
		Deck.Push( td );
	}

	public int[] cardsPerLevel = new int[] { 3, 3, 3 };

	public int TerrorLevel {
		get {
			int level3Count = cardsPerLevel[2];
			int level2Count = cardsPerLevel[1];
			int ct = Deck.Count;
			return (ct > level2Count + level3Count) ? 1
				: ct > level3Count ? 2
				: 3;
		}
	}

	public void AddDirect( FearArgs args ) {
		EarnedFear += args.count;
		while(PoolMax <= EarnedFear) { // should be while() - need unit test
			EarnedFear -= PoolMax;
			ActivatedCards.Push( Deck.Pop() );
			ActivatedCards.Peek().Text = "Active " + ActivatedCards.Count;
		}
		if(Deck.Count == 0)
			GameOverException.Win("Terror Level VICTORY");
		FearAdded?.Invoke( gs, args );
	}

	public async Task Apply() {
		while(ActivatedCards.Count > 0) {
			PositionFearCard fearCard = ActivatedCards.Pop();
			// show card to each user
			foreach(var spirit in gs.Spirits)
				await spirit.ShowFearCardToUser( "Activating Fear", fearCard, TerrorLevel );

			var ctx = new FearCtx( gs );
			switch(TerrorLevel) {
				case 1: await fearCard.FearOptions.Level1( ctx ); break;
				case 2: await fearCard.FearOptions.Level2( ctx ); break;
				case 3: await fearCard.FearOptions.Level3( ctx ); break;
			}
		}
	}

	// - ints -
	public int EarnedFear { get; private set; } = 0;
	public int PoolMax { get; set; }
	// - cards -
	public readonly Stack<PositionFearCard> Deck = new Stack<PositionFearCard>();
	public readonly Stack<PositionFearCard> ActivatedCards = new Stack<PositionFearCard>();
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
		}
		public void Restore(Fear src ) {
			src.EarnedFear = pool;
			src.Deck.SetItems( deck );
			src.ActivatedCards.SetItems(activatedCards);
		}
		readonly int pool;
		readonly PositionFearCard[] deck;
		readonly PositionFearCard[] activatedCards;
	}

	#endregion Memento

}

public class FearArgs {
	public int count;
	public Space space;
	public bool FromDestroyedInvaders; // defaults to false, only needs set when generated
}
