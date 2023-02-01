namespace SpiritIsland.Basegame;

public class BrandenburgPrussia : IAdversary {

	public const string Name = "Brandenburg-Prussia";

	public int Level { get; set; }

	public InvaderDeckBuilder InvaderDeckBuilder => new InvaderDeckBuilder( Level switch {
		2 => new int[] { 1, 1, 1, 3, 2, 2, 2, 2, 3, 3, 3, 3, },
		3 => new int[] { 1, 1, 3, 2, 2, 2, 2, 3, 3, 3, 3, },
		4 => new int[] { 1, 1, 3, 2, 2, 2, 3, 3, 3, 3 },
		5 => new int[] { 1, 3, 2, 2, 2, 3, 3, 3, 3 },
		6 => new int[] { 3, 2, 2, 2, 3, 3, 3, 3 },
		_ => null // use default
	} );


	public int[] FearCardsPerLevel => Level switch {
		1 => new int[] { 3, 3, 3 },
		2 => new int[] { 3, 3, 3 },
		3 => new int[] { 3, 4, 3 },
		4 => new int[] { 4, 4, 3 },
		5 => new int[] { 4, 4, 3 },
		6 => new int[] { 4, 4, 4 },
		_ => null
	};

	public ScenarioLevel[] Adjustments => new ScenarioLevel[] {
		new ScenarioLevel(1 , 3,3,3, "Land Rush", "On each board with Town/City, add 1 Town to a land withouth Towns." ),
		new ScenarioLevel(2 , 3,3,3, "Fast Start",           "During Setup, on each board add 1 town to land #3" ),
		new ScenarioLevel(4 , 3,3,3, "Surge of Colonists",   "111-3-2222-3333" ),
		new ScenarioLevel(6 , 3,4,3, "Efficient",            "11-3-2222-3333" ),
		new ScenarioLevel(7 , 4,4,3, "Agressive Timetable",  "11-3-222-3333" ),
		new ScenarioLevel(9 , 4,4,3, "Ruthlessly Efficent",  "1-3-222-3333" ),
		new ScenarioLevel(10, 4,4,4, "Terrifying Efficient", "3-222-3333" ),
	};

	public void PreInitialization( GameState gameState ) {
		gameState.InvaderDeck.Explore.Engine.Escalation = LandRush;

		if(1 <= Level)
			FastStart( gameState );
	}

	static void FastStart( GameState gameState ) {
		foreach(var board in gameState.Island.Boards)
			gameState.Tokens[board[3]].AdjustDefault( Human.Town, 1 );
		gameState.LogDebug("Adding 1 town to land #3");
	}

	public void PostInitialization( GameState gameState ) {}

	static async Task LandRush( GameState gs ) {
		// Land Rush: On each board with Town / City, add 1 Town to a land without Town

		var counts = gs.AllActiveSpaces
			.ToDictionary( s => s.Space, s => s );

		// s.SumAny(Invader.Town,Invader.City)

		var boards = gs.Island.Boards
			.Where( b => b.Spaces.Any( s => counts[s].SumAny( Human.Town_City ) > 0 ) )
			.ToHashSet();

		var terrainMapper = gs.Island.Terrain;

		var buildSpaces = counts.Values
			.Where( ss => boards.Contains( ss.Space.Board ) && ss.SumAny( Human.Town_City ) == 0 && terrainMapper.IsInPlay( ss ) )
			.GroupBy( space => space.Space.Board )
			.Select( grp => grp.OrderBy( ss => ss.Space.Text ).First() ) // (!! simplification) when multiple, select closest to coast.
			.ToArray();

		await using UnitOfWork actionScope = gs.StartAction( ActionCategory.Default );
		foreach(SpaceState bs in buildSpaces)
			await bs.Bind(actionScope).AddDefault(Human.Town, 1, AddReason.Build);

		gs.LogDebug("Land Rush: Adding 1 town to "+buildSpaces.Select(x=>x.Space.Text).Order().Join(","));

	}


}
