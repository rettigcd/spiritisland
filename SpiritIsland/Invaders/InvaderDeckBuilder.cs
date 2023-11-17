namespace SpiritIsland;

public class InvaderDeckBuilder {

	public static ImmutableList<InvaderCard> Level1Cards => ImmutableList.Create<InvaderCard>(
		InvaderCard.Stage1( Terrain.Jungle ),
		InvaderCard.Stage1( Terrain.Wetland ),
		InvaderCard.Stage1( Terrain.Sands ),
		InvaderCard.Stage1( Terrain.Mountain )
	);

	public static ImmutableList<InvaderCard> Level2Cards => ImmutableList.Create<InvaderCard>(
		InvaderCard.Stage2( Terrain.Jungle ),
		InvaderCard.Stage2( Terrain.Wetland ),
		InvaderCard.Stage2( Terrain.Sands ),
		InvaderCard.Stage2( Terrain.Mountain ),
		InvaderCard.Stage2Costal()
	);

	public static ImmutableList<InvaderCard> Level3Cards => ImmutableList.Create<InvaderCard>(
		InvaderCard.Stage3( Terrain.Jungle, Terrain.Sands ),
		InvaderCard.Stage3( Terrain.Jungle, Terrain.Mountain ),
		InvaderCard.Stage3( Terrain.Jungle, Terrain.Wetland ),
		InvaderCard.Stage3( Terrain.Mountain, Terrain.Sands ),
		InvaderCard.Stage3( Terrain.Mountain, Terrain.Wetland ),
		InvaderCard.Stage3( Terrain.Sands, Terrain.Wetland )
	);

	public readonly static InvaderDeckBuilder Default = new InvaderDeckBuilder();

	readonly int[] _levelSelection;

	public InvaderDeckBuilder( int[] levelSelection = default ) {
		_levelSelection = levelSelection 
			?? new int[] { 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3 };
	}
	public InvaderDeck Build( int seed = default ) {
		var unrevealedCards = new List<InvaderCard>();
		Queue<InvaderCard>[] unused = PrepareShuffledStageCards( seed );
		foreach(var selectionLevel in _levelSelection)
			SelectCard( unrevealedCards, unused, selectionLevel );

		return new InvaderDeck( unrevealedCards, unused );
	}

	protected virtual void SelectCard( List<InvaderCard> dst, Queue<InvaderCard>[] src, int level ) {
		dst.Add( src[level - 1].Dequeue() );
	}


	protected virtual IEnumerable<InvaderCard> SelectLevel1Cards() => Level1Cards;
	protected virtual IEnumerable<InvaderCard> SelectLevel2Cards() => Level2Cards;
	protected virtual IEnumerable<InvaderCard> SelectLevel3Cards() => Level3Cards;

	Queue<InvaderCard>[] PrepareShuffledStageCards( int seed ) {
		var l1 = SelectLevel1Cards().ToList();
		var l2 = SelectLevel2Cards().ToList();
		var l3 = SelectLevel3Cards().ToList();
		if(seed != default) {
			var random = new Random( seed );
			random.Shuffle( l1 );
			random.Shuffle( l2 );
			random.Shuffle( l3 );
		}

		return new Queue<InvaderCard>[] {
			new Queue<InvaderCard>( l1 ),
			new Queue<InvaderCard>( l2 ),
			new Queue<InvaderCard>( l3 )
		};
	}
}
