namespace SpiritIsland;

public class InvaderDeck {

	#region public static

	public static ImmutableList<InvaderCard> Level1Cards => ImmutableList.Create<InvaderCard>(
		InvaderCard.Stage1( Terrain.Jungle ),
		InvaderCard.Stage1( Terrain.Wetland ),
		InvaderCard.Stage1( Terrain.Sand ),
		InvaderCard.Stage1( Terrain.Mountain )
	);

	public static ImmutableList<InvaderCard> Level2Cards => ImmutableList.Create<InvaderCard>(
		InvaderCard.Stage2( Terrain.Jungle ),
		InvaderCard.Stage2( Terrain.Wetland ),
		InvaderCard.Stage2( Terrain.Sand ),
		InvaderCard.Stage2( Terrain.Mountain ),
		InvaderCard.Stage2Costal()
	);

	public static ImmutableList<InvaderCard> Level3Cards => ImmutableList.Create<InvaderCard>(
		InvaderCard.Stage3(Terrain.Jungle,Terrain.Sand),
		InvaderCard.Stage3(Terrain.Jungle,Terrain.Mountain),
		InvaderCard.Stage3(Terrain.Jungle,Terrain.Wetland),
		InvaderCard.Stage3(Terrain.Mountain,Terrain.Sand),
		InvaderCard.Stage3(Terrain.Mountain,Terrain.Wetland),
		InvaderCard.Stage3(Terrain.Sand,Terrain.Wetland)
	);

	public static InvaderDeck BuildTestDeck( params InvaderCard[] cards ) => new InvaderDeck( cards );

	#endregion

	#region constructors

	private InvaderDeck( params InvaderCard[] cards ) {
		_unrevealedCards = cards.ToList();
		InitNumberOfCardsToDraw();
		ActiveSlots = new List<InvaderSlot> { Ravage, Build, Explore };
	}

	readonly List<InvaderCard>[] _unused;

	public InvaderCard TakeNextUnused( int selectionLevel ) {
		var stageCards = _unused[selectionLevel - 1];
		var card = stageCards[0];
		stageCards.RemoveAt( 0 );
		return card;
	}

	public InvaderDeck( int seed = default, int[] levelSelection = default ) {
		levelSelection ??= new int[] { 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3 };

		_unused = new List<InvaderCard>[] {
			Level1Cards.ToList(),
			Level2Cards.ToList(),
			Level3Cards.ToList()
		};

		if(seed != default) {
			var random = new Random( seed );
			random.Shuffle( _unused[0] );
			random.Shuffle( _unused[1] );
			random.Shuffle( _unused[2] );
		}

		// Merge
		var all = new List<InvaderCard>();
		foreach(var selectionLevel in levelSelection) {
			all.Add( TakeNextUnused( selectionLevel ) );
		}
		_unrevealedCards = all.ToList();
		InitNumberOfCardsToDraw();
		ActiveSlots = new List<InvaderSlot> { Ravage, Build, Explore };
	}

	void InitNumberOfCardsToDraw() {

		// Setup draw: 1 card at a time.
		for(int i = 0; i < UnrevealedCards.Count; ++i) 
			drawCount.Add( 1 );

	}

	#endregion

	public List<InvaderCard> UnrevealedCards => _unrevealedCards;
	readonly List<InvaderCard> _unrevealedCards;
	public readonly List<int> drawCount = new List<int>(); // tracks how many cards to draw each turn

	public ExploreSlot Explore { get; } = new ExploreSlot();
	public BuildSlot Build { get; } = new BuildSlot();
	public RavageSlot Ravage { get; } = new RavageSlot();

	// ??? Should Unused, Unrevealed, and Discard be slots also?  (Just not in the ActiveSlots) list

	public List<InvaderSlot> ActiveSlots {
		get {  return _activeSlots; }
		set { _activeSlots = value; }
	}
	List<InvaderSlot> _activeSlots;

	public List<InvaderCard> Discards {get;} = new List<InvaderCard>();

	public int InvaderStage => (Explore.Cards.FirstOrDefault() ?? UnrevealedCards.First()).InvaderStage;

	public void DelayLastExploreCard() {

		// Make sure our list of DrawCounts has at least 1 slot.
		if(drawCount.Count == 0) drawCount.Add( 0 );

		// Find card
		int currentExploreIndex = Explore.Cards.Count - 1;
		var card = Explore.Cards[currentExploreIndex];

		// Remove card from the explore pile
		Explore.Cards.RemoveAt( currentExploreIndex );

		// Return card to the Unrevealed Cards
		UnrevealedCards.Insert( 0, card );
		drawCount[0]++;


		// Alternate
		card.Skip = true;
		card.HoldBack = true;

	}


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

		CheckIfTimeRunsOut();
		InitExploreSlot();
	}

	void CheckIfTimeRunsOut() {
		if( Explore.Cards.Count==0 && UnrevealedCards.Count==0 )
			GameOverException.Lost("Time runs out");
	}

	public void InitExploreSlot() {
		if(UnrevealedCards.Count == 0) return; // does this ever happen?
		int count = drawCount[0]; drawCount.RemoveAt( 0 );
		while(0 < count--) {
			var unrevealedCard = UnrevealedCards[0];
			UnrevealedCards.RemoveAt( 0 );
			Explore.Cards.Add( unrevealedCard );
		}
	}

	public void ReplaceUnrevealedCards( Func<InvaderCard, InvaderCard> replacer ) {
		InvaderDeck deck = this;
		for(int i = 0; i < deck.UnrevealedCards.Count; ++i) {
			var existing = deck.UnrevealedCards[i];
			if(existing is not InvaderCard simpleInvaderCard)
				throw new InvalidOperationException( existing.GetType().Name + " cannot be replaced/modified." );
			deck.UnrevealedCards[i] = replacer( simpleInvaderCard );
		}
	}

	#region Memento

	public virtual IMemento<InvaderDeck> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<InvaderDeck> memento ) => ((Memento)memento).Restore(this);

	protected class Memento : IMemento<InvaderDeck> {
		public Memento(InvaderDeck src) {
			unrevealedCards = src.UnrevealedCards.ToArray();
			drawCount = src.drawCount.ToArray();

			explore = src.Explore.Cards.ToArray();
			build = src.Build.Cards.ToArray();
			ravage = src.Ravage.Cards.ToArray();
			discards = src.Discards.ToArray();
			flipped = explore.Union(build).Union(ravage).Union(discards).ToDictionary(c=>c,c=>c.Flipped);
		}
		public void Restore(InvaderDeck src ) {
			src.UnrevealedCards.SetItems(unrevealedCards);
			src.drawCount.SetItems(drawCount);
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
