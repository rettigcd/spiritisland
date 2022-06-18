namespace SpiritIsland;

public class InvaderDeck {

	#region public static

	public static readonly ImmutableList<IInvaderCard> Level1Cards = ImmutableList.Create<IInvaderCard>(
		InvaderCard.Stage1( Terrain.Jungle ),
		InvaderCard.Stage1( Terrain.Wetland ),
		InvaderCard.Stage1( Terrain.Sand ),
		InvaderCard.Stage1( Terrain.Mountain )
	);

	public static readonly ImmutableList<IInvaderCard> Level2Cards = ImmutableList.Create<IInvaderCard>(
		InvaderCard.Stage2( Terrain.Jungle ),
		InvaderCard.Stage2( Terrain.Wetland ),
		InvaderCard.Stage2( Terrain.Sand ),
		InvaderCard.Stage2( Terrain.Mountain ),
		InvaderCard.Stage2Costal()
	);

	public static readonly ImmutableList<IInvaderCard> Level3Cards = ImmutableList.Create<IInvaderCard>(
		InvaderCard.Stage3(Terrain.Jungle,Terrain.Sand),
		InvaderCard.Stage3(Terrain.Jungle,Terrain.Mountain),
		InvaderCard.Stage3(Terrain.Jungle,Terrain.Wetland),
		InvaderCard.Stage3(Terrain.Mountain,Terrain.Sand),
		InvaderCard.Stage3(Terrain.Mountain,Terrain.Wetland),
		InvaderCard.Stage3(Terrain.Sand,Terrain.Wetland)
	);

	public static InvaderDeck BuildTestDeck( params IInvaderCard[] cards ) => new InvaderDeck( cards );

	#endregion

	#region constructors

	private InvaderDeck( params IInvaderCard[] cards ) {
		_unrevealedCards = cards.ToList();
		InitNumberOfCardsToDraw();
		Slots = new List<InvaderSlot> { Ravage, Build, Explore };
}

public InvaderDeck( int seed = default, int[] levelSelection = default ) {
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
			var level = levels[selectionLevel - 1];
			all.Add( level[0] );
			level.RemoveAt( 0 );
		}
		_unrevealedCards = all.ToList();
		InitNumberOfCardsToDraw();
		Slots = new List<InvaderSlot> { Ravage, Build, Explore };
	}

	void InitNumberOfCardsToDraw() {

		// Setup draw: 1 card at a time.
		for(int i = 0; i < UnrevealedCards.Count; ++i) 
			drawCount.Add( 1 );

	}

	#endregion

	public List<IInvaderCard> UnrevealedCards => _unrevealedCards;
	readonly List<IInvaderCard> _unrevealedCards;
	public readonly List<int> drawCount = new List<int>(); // tracks how many cards to draw each turn

	public ExploreSlot Explore { get; } = new ExploreSlot();
	public BuildSlot Build { get; } = new BuildSlot();
	public RavageSlot Ravage { get; } = new RavageSlot();
	public List<InvaderSlot> Slots;

	public List<IInvaderCard> Discards {get;} = new List<IInvaderCard>();

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
		foreach(var slot in Slots) {
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
		while(count-- > 0) {
			Explore.Cards.Add( UnrevealedCards[0] );
			UnrevealedCards.RemoveAt( 0 );
		}
	}

	public void ReplaceCards( Func<InvaderCard, IInvaderCard> replacer ) {
		InvaderDeck deck = this;
		for(int i = 0; i < deck.UnrevealedCards.Count; ++i) {
			if(deck.UnrevealedCards[i] is not InvaderCard simpleInvaderCard)
				throw new InvalidOperationException( "We can only apply Adversary modification to original (simple) Invader Cards" );
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

		}
		public void Restore(InvaderDeck src ) {
			src.UnrevealedCards.SetItems(unrevealedCards);
			src.drawCount.SetItems(drawCount);
			src.Explore.Cards.SetItems(explore);
			src.Build.Cards.SetItems(build);
			src.Ravage.Cards.SetItems(ravage);
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

public abstract class InvaderSlot {
	public List<IInvaderCard> Cards { get; } = new List<IInvaderCard>();
	public void HoldNextBack() { holdBackCount++; }
	public void SkipNextNormal() { skipCount++; }
	public virtual async Task Execute( GameState gs ) {
		foreach(var card in Cards)
			if(skipCount > 0)
				skipCount--;
			else if(card.Skip)
				card.Skip = false; // !!!! not sure if Card.Skip is ever set to true.
			else
				await CardAction(card,gs);
	}

	public List<IInvaderCard> GetCardsToAdvance() {
		var result = new List<IInvaderCard>();
		for(int i=0; i < Cards.Count; ++i)
			if(holdBackCount > 0)
				holdBackCount--;
			else {
				result.Add(Cards[i]);
				Cards.RemoveAt(i--);
			}
		return result;
	}

	protected abstract Task CardAction( IInvaderCard card, GameState gameState);

	int skipCount = 0;
	int holdBackCount = 0;
}

// ??? Is this the Visitor Pattern ???
public class RavageSlot : InvaderSlot {
	protected override Task CardAction( IInvaderCard card, GameState gameState ) => card.Ravage( gameState );
}

public class BuildSlot : InvaderSlot {
	protected override Task CardAction( IInvaderCard card, GameState gameState ) => card.Build( gameState );
}

public class ExploreSlot : InvaderSlot {
	protected override Task CardAction( IInvaderCard card, GameState gameState ) => card.Explore( gameState );
}
