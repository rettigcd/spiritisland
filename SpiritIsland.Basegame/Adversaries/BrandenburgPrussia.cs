namespace SpiritIsland.Basegame;

public class BrandenburgPrussia : AdversaryBase, IAdversary {

	public const string Name = "Brandenburg-Prussia";

	public override AdversaryLevel[] Levels => new AdversaryLevel[] {
		// Escalation
		new AdversaryLevel(level:0, 1 , 3,3,3, "Land Rush", "On each board with Town/City, add 1 Town to a land withouth Towns." ).WithEscalation( LandRush ),
		// Level 1
		new AdversaryLevel(level:1, 2 , 3,3,3, "Fast Start", "During Setup, on each board add 1 town to land #3" ){ 
			InitFunc = (gs,_) => {
				foreach(var board in gs.Island.Boards)
					board[3].Tokens.AdjustDefault( Human.Town, 1 );
			}
		},
		new AdversaryLevel(level:2, 4 , 3,3,3, "Surge of Colonists"  ).WithInvaderCardOrder("111-3-2222-3333"),
		new AdversaryLevel(level:3, 6 , 3,4,3, "Efficient"           ).WithInvaderCardOrder("11-3-2222-3333"),
		new AdversaryLevel(level:4, 7 , 4,4,3, "Agressive Timetable" ).WithInvaderCardOrder("11-3-222-3333"),
		new AdversaryLevel(level:5, 9 , 4,4,3, "Ruthlessly Efficent" ).WithInvaderCardOrder("1-3-222-3333"),
		new AdversaryLevel(level:6, 10, 4,4,4, "Terrifying Efficient").WithInvaderCardOrder("3-222-3333"),
	};

	#region LandRush - Escalation

	static async Task LandRush( GameState gs ) {
		// Land Rush: On each board with Town / City, add 1 Town to a land without Town
		await Cmd.AddTown( 1, " (Escalation - Land Rush)" )
			.On().OneLandPerBoard().Which( Has.No( Human.Town ) )
			.ForEachBoard().Which( BoardHas.TownOrCity )
			.ActAsync( gs );
	}

	#endregion

}
