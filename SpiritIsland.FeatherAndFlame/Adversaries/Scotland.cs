namespace SpiritIsland.FeatherAndFlame;

public class Scotland : IAdversary {

	public const string Name = "Scotland";

	public int Level { get; set; }

	public ScenarioLevel[] Adjustments => new ScenarioLevel[] {
		// Escalation
		new ScenarioLevel(1 , 3,3,3, "Ports Sprawl Outward", "On the single board with the most Coastal Town/City, add 1 Town to the N lands with the fewest Town (N = # of players.)" ),
		// Level 1
		new ScenarioLevel(3 , 3,4,3, "Trading Port", "After Setup, in Coastal lands, Explore Cards add 1 Town instead of 1 Explorer. 'Coastal Lands' Invader cards do this for maximum 2 lands per board." ),
		// Level 2
		new ScenarioLevel(4 , 4,4,3, "Seize Opportunity", "11-22-1-C2-33333" ),
		// Level 3
		new ScenarioLevel(6 , 4,5,4, "Chart the Coastline", "In Coastal lands, Build Cards affect lands without Invaders, so long as there is an adjacent City." ),
		// Level 4
		new ScenarioLevel(7 , 5,5,4, "Ambition of a Minor Nation", "11-22-3-C2-3333" ),
		// Level 5
		new ScenarioLevel(8 , 5,6,4, "Runoff and Bilgewater", "After a Ravage Action adds Blight to a Coastal Land, add 1 Blight to that board's Ocean (without cascading). Treat the Ocean as a Coastal Wetland for this rule and for Blight removal/movement." ),
		// Level 6
		new ScenarioLevel(10 , 6,6,4, "Exports Fuel Inward Growth", "After the Ravage step, add 1 Town to each Inland land that matches a Ravage card and is within 1 of town/city" ),
	};

	public InvaderDeckBuilder InvaderDeckBuilder => new ScotlandInvaderDeckBuilder(Level);

	public int[] FearCardsPerLevel => Level switch {
		1 => new int[] { 3, 4, 3 },
		2 => new int[] { 4, 4, 3 },
		3 => new int[] { 4, 5, 4 },
		4 => new int[] { 5, 5, 4 },
		5 => new int[] { 5, 6, 4 },
		6 => new int[] { 6, 6, 4 },
		_ => null,
	};

	public void PreInitialization( GameState gameState ) {

		// Escalation - Ports Sprawl Outward:
		gameState.InvaderDeck.Explore.Engine.Escalation = PortsSprawlOutward_Escalation;

		// Level 1 - Trading Port: After Setup, in Coastal lands, Explore Cards add 1 Town instead of 1 Explorer. "Coastal Lands" Invader cards do this for maximum 2 lands per board.
		if(1 <= Level)
			gameState.InvaderDeck.Explore.Engine = new ScotlandExploreEngine();

		// Level 2 - Seize Opportunity: During Setup, add 1 City to land #2. Place "Coastal Lands" as the 3rd Stage II card, and move the two Stage II Cards above it up by one. (New Deck Order: 11-22-1-C2-33333, where C is the Stage II Coastal Lands Card.)
		if(2 <= Level)
			SeizeOpportunity( gameState );

		// Level 3 -  Chart the Coastline: In Coastal lands, Build Cards affect lands without Invaders, so long as there is an adjacent City.
		if(3 <= Level)
			gameState.InvaderDeck.Build.Engine = new ScotlandBuildEngine { ShouldChartTheCoastline = true };

		// Level 4 - Nothing to do, Invader Deck order only

		// Level 5 - Runoff and Bilgewater:
		if(5 <= Level) {
			// After a Ravage Action adds Blight to a Coastal Land, add 1 Blight to that board's Ocean (without cascading).
			var token = new ScotlandCoastalBlightCheckToken();
			foreach(var ss in gameState.AllActiveSpaces.Where( ss => ss.Space.IsCoastal ))
				ss.Adjust(token,1);

			// Treat the Ocean as a Coastal Wetland for this rule and for Blight removal/movement.
			// !!! Mustbe able to target ocean for removing it.
		}

		// LEvel 6 - Exports Fuel Inward Growth:
		if(6 <= Level)
			gameState.InvaderDeck.Ravage.Engine = new ScotlandRavageEngine();

		// Additional Loss Condition - Trade Hub: If the number of Coastal lands with City is ever greater than( 2 x # of boards), the Invaders win.
		gameState.AddWinLossCheck( TradeHub );

	}

	public void PostInitialization( GameState gamestate ) { }

	static async Task PortsSprawlOutward_Escalation( GameState gameState ) {
		// On the single board with the most Coastal Town / City,
		var board = gameState.Island.Boards
			.OrderByDescending( b => b.Spaces.Where( s => s.IsCoastal ).Upgrade()
					.Sum( ss => ss.SumAny( Human.Town_City ) ) )
			.First();
		// add 1 Town to the N lands with the fewest Town( N = # of players.)
		var spacesToAddTown = board.Spaces.Upgrade()
			.Where( gameState.Island.Terrain.IsInPlay )
			.OrderBy( ss => ss.Sum( Human.Town ) )
			.Take( gameState.Spirits.Length )
			.ToArray();
		await using(var actionScope = new ActionScope( ActionCategory.Adversary ))
			foreach(SpaceState ss in spacesToAddTown)
				await ss.AddDefault( Human.Town, 1, AddReason.Build );
		gameState.Log(new SpiritIsland.Log.Debug($"Ports Sprawl Outword: Adding 1 town to "+spacesToAddTown.SelectLabels().Join(",")));
	}

	static void SeizeOpportunity( GameState gameState ) {
		// Add 1 City to land #2
		foreach(Board board in gameState.Island.Boards)
			gameState.Tokens[board[2]].AdjustDefault( Human.City, 1 );
		gameState.Log(new SpiritIsland.Log.Debug("Seize Opportunity - adding 1 city to space 2 of each board."));
	}

	void TradeHub( GameState gameState ) {
		int coastalCityLandCount = gameState.AllSpaces.Count( s => s.Has( Human.City ) );
		if( gameState.Island.Boards.Length < coastalCityLandCount )
			GameOverException.Lost($"Trade Hub - {coastalCityLandCount} coastal lands have cities.");
	}

}
