namespace SpiritIsland.Basegame;

// The Kingdom of Brandenburg-Prussia
// 1	(2)		9 (3/3/3)	Fast Start:  During Setup, on each board add 1 town to land #3
// 2	(4)		9 (3/3/3)	Surge of Colonists:		(111-3-2222-3333)
// 3	(6)		10 (3/4/3)	Efficient:				(11-3-2222-3333)
// 4	(7)		11 (4/4/3)	Agressive Timetable:	(11-3-222-3333)
// 5	(9)		11 (4/4/3)	Ruthlessly Efficent:	(1-3-222-3333)
// 6	(10)	12 (4/4/4)	Terrifying Efficient:	(3-222-3333)

public class BrandenburgPrussia : IAdversary {

	public int Level { get; set; }

	public int[] InvaderCardOrder => Level switch {
		2 => new int[] {1,1,1,3,2,2,2,2,3,3,3,3, },
		3 => new int[] { 1, 1, 3, 2, 2, 2, 2, 3, 3, 3, 3, },
		4 => new int[] { 1, 1, 3, 2, 2, 2, 3, 3, 3, 3 },
		5 => new int[] { 1, 3, 2, 2, 2, 3, 3, 3, 3 },
		6 => new int[] { 3, 2, 2, 2, 3, 3, 3, 3 },
		_ => null // use default
	};

	public int[] FearCardsPerLevel => Level switch {
		3 => new int[] {3,4,3 },
		4 => new int[] {4,4,3 },
		5 => new int[] { 4, 4, 3 },
		6 => new int[] { 4, 4, 4 },
		_ => null
	};

	//readonly ScenarioLevel[] Adjustments = new ScenarioLevel[] {
	//	new ScenarioLevel(2 , 3,3,3, "Fast Start",           "During Setup, on each board add 1 town to land #3" ),
	//	new ScenarioLevel(4 , 3,3,3, "Surge of Colonists",   "111-3-2222-3333" ),
	//	new ScenarioLevel(6 , 3,4,3, "Efficient",            "11-3-2222-3333" ),
	//	new ScenarioLevel(7 , 4,4,3, "Agressive Timetable",  "11-3-222-3333" ),
	//	new ScenarioLevel(9 , 4,4,3, "Ruthlessly Efficent",  "1-3-222-3333" ),
	//	new ScenarioLevel(10, 4,4,4, "Terrifying Efficient", "(3-222-3333" ),
	//};

	public void Adjust( GameState gameState ) {
		if( Level < 1) return;
		foreach(var board in gameState.Island.Boards)
			gameState.Tokens[ board[3] ].AdjustDefault( Invader.Town, 1 );
	}

	public void AdjustInvaderDeck( InvaderDeck deck ) {
		for(int i = 0; i < deck.UnrevealedCards.Count; ++i) {
			if(deck.UnrevealedCards[i] is not InvaderCard simpleInvaderCard)
				throw new InvalidOperationException( "We can only apply Brandenburg Prussia modification to original (simple) Invader Cards" );
			deck.UnrevealedCards[i] = new BrandenburgPrussiaInvaderCard( simpleInvaderCard );
		}
	}
}

// Adds Escalation
class BrandenburgPrussiaInvaderCard : InvaderCard {
	public BrandenburgPrussiaInvaderCard( InvaderCard card ):base(card.Filter,card.InvaderStage ) { }
	public override async Task Explore( GameState gs ) {
		await base.Explore( gs );
		if( HasEscalation )
			await Escalation( gs );
	}
	Task Escalation( GameState gs ) {
		// Land Rush: On each board with Townicon / City, add 1 Town to a land without Town

		var counts = gs.Island.AllSpaces.ToDictionary( s=>s, s=> gs.Tokens[s].SumAny(Invader.Town,Invader.City));

		var boards = gs.Island.Boards
			.Where( b=>b.Spaces.Any(s=>counts[s]>0) )
			.ToHashSet();

		var buildSpaces = counts
			.Where(pair=>boards.Contains(pair.Key.Board) && pair.Value==0)
			.Select(pair => pair.Key)
			.GroupBy(space => space.Board)
			.Select(grp => grp.OrderBy(space=>space.Text).First()) // (!! simplification) when multiple, select closest to coast.
			.ToArray();

		return England.SimplifiedBuild( gs, buildSpaces );
	}
}