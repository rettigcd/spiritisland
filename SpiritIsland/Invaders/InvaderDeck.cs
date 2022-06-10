namespace SpiritIsland;

public class InvaderDeck {

	#region public static

	public static readonly ImmutableList<IInvaderCard> Level1Cards = ImmutableList.Create<IInvaderCard>(
		new InvaderCard( Terrain.Jungle ),
		new InvaderCard( Terrain.Wetland ),
		new InvaderCard( Terrain.Sand ),
		new InvaderCard( Terrain.Mountain )
	);

	public static readonly ImmutableList<IInvaderCard> Level2Cards = ImmutableList.Create(
		InvaderCard.Stage2( Terrain.Jungle ),
		InvaderCard.Stage2( Terrain.Wetland ),
		InvaderCard.Stage2( Terrain.Sand ),
		InvaderCard.Stage2( Terrain.Mountain ),
		InvaderCard.Stage2Costal()
	);

	public static readonly ImmutableList<IInvaderCard> Level3Cards = ImmutableList.Create(
		InvaderCard.Stage3(Terrain.Jungle,Terrain.Sand),
		InvaderCard.Stage3(Terrain.Jungle,Terrain.Mountain),
		InvaderCard.Stage3(Terrain.Jungle,Terrain.Wetland),
		InvaderCard.Stage3(Terrain.Mountain,Terrain.Sand),
		InvaderCard.Stage3(Terrain.Mountain,Terrain.Wetland),
		InvaderCard.Stage3(Terrain.Sand,Terrain.Wetland)
	);

	public static InvaderDeck BuildTestDeck( params IInvaderCard[] cards ) => new InvaderDeck( cards );


	public static InvaderDeck Unshuffled() => new InvaderDeck();

	#endregion

	#region constructors

	#region constructors

	private InvaderDeck( params IInvaderCard[] cards ) {
		this.unrevealedCards = cards.ToList();
		Init();
	}

	public InvaderDeck( int[] levelSelection = default, int seed = default ) {
		levelSelection ??= new int[] { 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3 };

		var levels = new List<IInvaderCard>[] {
			Level1Cards.ToList(),
			Level2Cards.ToList(),
			Level3Cards.ToList()
		};

		if(seed != default) {
			var random = new Random( seed );
			random.Shuffle( levels[0] );
			random.Shuffle( levels[1] );
			random.Shuffle( levels[2] );
		}

		// Merge
		var all = new List<IInvaderCard>();
		foreach(var selectionLevel in levelSelection) {
			var level = levels[selectionLevel-1];
			all.Add( level[0] );
			level.RemoveAt(0);
		}
		unrevealedCards = all.ToList();

		Init();
	}

	void Init() {
		// Setup draw: 1 card at a time.
		for(int i = 0; i < unrevealedCards.Count; ++i) 
			drawCount.Add( 1 );

		InitExplorers(); // initialize the first explorer card up
	}

	#endregion

	#endregion

	public readonly List<IInvaderCard> unrevealedCards;
	public readonly List<int> drawCount = new List<int>(); // tracks how many cards to draw each turn

	public List<IInvaderCard> Explore {get;} = new List<IInvaderCard>();

	public List<IInvaderCard> Build { get; } = new List<IInvaderCard>();

	public List<IInvaderCard> Ravage { get; } = new List<IInvaderCard>();

	public int CountInDiscard => Discards.Count;
	public List<IInvaderCard> Discards {get;} = new List<IInvaderCard>();


	public bool KeepBuildCards = false; // !!! is there a way to make this go away?


	/// <summary>
	/// Triggers Ravage / 
	/// </summary>
	public void Advance() {

		// Move Ravage to Discard
		Discards.AddRange( Ravage );
		Ravage.Clear();

		// Move Build to Ravage
		if(KeepBuildCards)
			KeepBuildCards = false;
		else {
			Ravage.AddRange( Build );
			Build.Clear();
		}

		// move Explore to Build
		CheckIfTimeRunsOut();
		Build.AddRange( Explore );
		Explore.Clear();

		InitExplorers();
	}

	void CheckIfTimeRunsOut() {
		if( Explore.Count==0 && unrevealedCards.Count==0 )
			GameOverException.Lost("Time runs out");
	}

	void InitExplorers() {
		if(unrevealedCards.Count > 0) {
			int count = drawCount[0]; drawCount.RemoveAt( 0 );
			while(count-- > 0) {
				Explore.Add( unrevealedCards[0] );
				unrevealedCards.RemoveAt( 0 );
			}
		}
	}

	public void DelayLastExploreCard() {
		if(drawCount.Count==0) drawCount.Add(0);

		var idx = Explore.Count - 1;
		var card = Explore[idx];
		Explore.RemoveAt( idx );
		unrevealedCards.Insert( 0, card );
		drawCount[0]++;
	}

	#region Memento

	public virtual IMemento<InvaderDeck> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<InvaderDeck> memento ) => ((Memento)memento).Restore(this);

	protected class Memento : IMemento<InvaderDeck> {
		public Memento(InvaderDeck src) {
			unrevealedCards = src.unrevealedCards.ToArray();
			drawCount = src.drawCount.ToArray();

			explore = src.Explore.ToArray();
			build = src.Build.ToArray();
			ravage = src.Ravage.ToArray();
			discards = src.Discards.ToArray();

		}
		public void Restore(InvaderDeck src ) {
			src.unrevealedCards.SetItems(unrevealedCards);
			src.drawCount.SetItems(drawCount);
			src.Explore.SetItems(explore);
			src.Build.SetItems(build);
			src.Ravage.SetItems(ravage);
			src.Discards.SetItems(discards);
		}
		readonly IInvaderCard[] unrevealedCards;
		readonly int[] drawCount;
		readonly IInvaderCard[] explore;
		readonly IInvaderCard[] build;
		readonly IInvaderCard[] ravage;
		readonly IInvaderCard[] discards;

	}

	#endregion

}