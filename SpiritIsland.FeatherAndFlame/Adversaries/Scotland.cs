namespace SpiritIsland.FeatherAndFlame;

public class Scotland : AdversaryBase, IAdversary {

	public const string Name = "Scotland";

	public override AdversaryLevel[] Levels => new AdversaryLevel[] {
		// Escalation
		new AdversaryLevel(1 , 3,3,3, "Ports Sprawl Outward", 
			"On the single board with the most Coastal Town/City, add 1 Town to the N lands with the fewest Town (N = # of players.)" )
			.WithEscalation( PortsSprawlOutward_Escalation )
			.WithWinLossCondition( TradeHub ),

		// Level 1
		new AdversaryLevel(3 , 3,4,3, "Trading Port", 
			"After Setup, in Coastal lands, Explore Cards add 1 Town instead of 1 Explorer. 'Coastal Lands' Invader cards do this for maximum 2 lands per board." ) {
			InitFunc = (gameState,_) => { gameState.InvaderDeck.Explore.Engine = new ScotlandExploreEngine(); }
		},

		// Level 2
		new AdversaryLevel(4 , 4,4,3, "Seize Opportunity", "11-22-1-C2-33333" ) {
			InitFunc = (gameState,_) => {
				// Add 1 City to land #2
				foreach(Board board in gameState.Island.Boards)
					board[2].Tokens.AdjustDefault( Human.City, 1 );
				gameState.Log(new SpiritIsland.Log.Debug("Seize Opportunity - adding 1 city to space 2 of each board."));
			},
			InvaderDeckBuilder = new ScotlandInvaderDeckBuilder(1, 1, 2, 2, 1, ScotlandInvaderDeckBuilder.Coastal, 2, 3, 3, 3, 3, 3),
		},

		// Level 3
		new AdversaryLevel(6 , 4,5,4, "Chart the Coastline", "In Coastal lands, Build Cards affect lands without Invaders, so long as there is an adjacent City." ) {
			InitFunc = (gameState,_) => gameState.InvaderDeck.Build.Engine = new ScotlandBuildEngine { ShouldChartTheCoastline = true },
		},

		// Level 4
		new AdversaryLevel(7 , 5,5,4, "Ambition of a Minor Nation", "11-22-3-C2-3333" ) {
			InvaderDeckBuilder = new ScotlandInvaderDeckBuilder(1, 1, 2, 2, 3, ScotlandInvaderDeckBuilder.Coastal, 2, 3, 3, 3, 3),
		},

		// Level 5
		new AdversaryLevel(8 , 5,6,4, "Runoff and Bilgewater", 
			"After a Ravage Action adds Blight to a Coastal Land, add 1 Blight to that board's Ocean (without cascading). Treat the Ocean as a Coastal Wetland for this rule and for Blight removal/movement." ) {
			InitFunc = (gameState,_) => {
				// After a Ravage Action adds Blight to a Coastal Land, add 1 Blight to that board's Ocean (without cascading).
				var token = new ScotlandCoastalBlightCheckToken();
				foreach(var ss in gameState.Spaces.Where( ss => ss.Space.IsCoastal ))
					ss.Adjust(token,1);

				// Treat the Ocean as a Coastal Wetland for this rule and for Blight removal/movement.
			}
		},

		// Level 6
		new AdversaryLevel(10 , 6,6,4, "Exports Fuel Inward Growth", "After the Ravage step, add 1 Town to each Inland land that matches a Ravage card and is within 1 of town/city" ) {
			InitFunc = (gameState,_) => gameState.InvaderDeck.Ravage.Engine = new ScotlandRavageEngine(),
		},
	};

	#region Escalation - Ports Sprawl Outward

	static async Task PortsSprawlOutward_Escalation( GameState gameState ) {
		// On the single board with the most Coastal Town / City,
		var board = gameState.Island.Boards
			.OrderByDescending( b => b.Spaces.Where( s => s.IsCoastal ).Tokens()
					.Sum( ss => ss.SumAny( Human.Town_City ) ) )
			.First();
		// add 1 Town to the N lands with the fewest Town( N = # of players.)
		var spacesToAddTown = board.Spaces.Tokens()
			.OrderBy( ss => ss.Sum( Human.Town ) )
			.Take( gameState.Spirits.Length )
			.ToArray();
		await using(var actionScope = await ActionScope.Start(ActionCategory.Adversary))
			foreach(SpaceState ss in spacesToAddTown)
				await ss.AddDefault( Human.Town, 1, AddReason.Build );
		gameState.Log(new SpiritIsland.Log.Debug($"Ports Sprawl Outword: Adding 1 town to "+spacesToAddTown.SelectLabels().Join(",")));
	}

	#endregion Escalation - Ports Sprawl Outward

	#region Additional Win/Loss condition

	// If the number of Coastal lands with City is ever greater than( 2 x # of boards), the Invaders win.
	void TradeHub( GameState gameState ) {
		int coastalCityLandCount = gameState.Spaces_Unfiltered
			.Count( s => s.Has( Human.City ) && s.Space.IsCoastal );
		if( gameState.Island.Boards.Length < coastalCityLandCount )
			GameOverException.Lost($"Trade Hub - {coastalCityLandCount} coastal lands have cities.");
	}

	#endregion Additional Win/Loss condition
}
