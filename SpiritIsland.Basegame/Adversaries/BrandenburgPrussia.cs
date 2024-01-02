namespace SpiritIsland.Basegame;

public class BrandenburgPrussia : AdversaryBase, IAdversary {

	public const string Name = "Brandenburg-Prussia";

	public override AdversaryLevel[] Levels => new AdversaryLevel[] {
		// Escalation
		new AdversaryLevel(1 , 3,3,3, "Land Rush", "On each board with Town/City, add 1 Town to a land withouth Towns." ).WithEscalation( LandRush ),
		// Level 1
		new AdversaryLevel(2 , 3,3,3, "Fast Start", "During Setup, on each board add 1 town to land #3" ){ 
			InitFunc = (gs,_) => {
				foreach(var board in gs.Island.Boards)
					board[3].Tokens.AdjustDefault( Human.Town, 1 );
			}
		},
		new AdversaryLevel(4 , 3,3,3, "Surge of Colonists",   "111-3-2222-3333" ).WithInvaderDeck(1,1,1, 3, 2,2,2,2, 3,3,3,3),
		new AdversaryLevel(6 , 3,4,3, "Efficient",            "11-3-2222-3333"  ).WithInvaderDeck(1,1,   3, 2,2,2,2, 3,3,3,3),
		new AdversaryLevel(7 , 4,4,3, "Agressive Timetable",  "11-3-222-3333"   ).WithInvaderDeck(1,1,   3, 2,2,2,   3,3,3,3),
		new AdversaryLevel(9 , 4,4,3, "Ruthlessly Efficent",  "1-3-222-3333"    ).WithInvaderDeck(1,     3, 2,2,2,   3,3,3,3),
		new AdversaryLevel(10, 4,4,4, "Terrifying Efficient", "3-222-3333"      ).WithInvaderDeck(       3, 2,2,2,   3,3,3,3),
	};

	#region LandRush - Escalation

	static async Task LandRush( GameState gs ) {
		// Land Rush: On each board with Town / City, add 1 Town to a land without Town

		var counts = gs.Spaces.IsInPlay()
			.ToDictionary( s => s.Space, s => s );

		var boardsWithTownOrCity = gs.Island.Boards
			.Where( b => b.Spaces.Any( s => counts[s].SumAny( Human.Town_City ) > 0 ) )
			.ToHashSet();

		var terrainMapper = TerrainMapper.Current;

		var buildSpaces = boardsWithTownOrCity
			.Select( FirstNonBuildingSpace )
			.Where( x => x != null )
			.ToArray();

		await using ActionScope actionScope = await ActionScope.Start(ActionCategory.Default);
		foreach(SpaceState bs in buildSpaces)
			await bs.AddDefault( Human.Town, 1, AddReason.Build );

		gs.LogDebug( "Land Rush: Adding 1 town to " + buildSpaces.SelectLabels().Order().Join( "," ) );

	}

	static SpaceState FirstNonBuildingSpace( Board board )
		=> board.Spaces.Tokens()
			.Where( ss => ss.SumAny( Human.Town_City ) == 0 )
			.OrderBy( ss => ss.Space.Text )
			.FirstOrDefault(); // (!! simplification) when multiple, select closest to coast.;

	#endregion

}
