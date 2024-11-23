namespace SpiritIsland.FeatherAndFlame;

public class Scotland : AdversaryBuilder, IAdversaryBuilder {

	public const string Name = "Scotland";

	public Scotland():base(Name) { }

	#region Additional Win/Loss condition

	public override AdversaryLossCondition LossCondition => new AdversaryLossCondition(
		"Trade Hub: If the number of Coastal lands with City is ever greater than (2x # of boards), the Invaders win.",
		TradeHubImp
	);

	static void TradeHubImp(GameState gameState) {
		int coastalCityLandCount = ActionScope.Current.Spaces_Unfiltered
			.Count(s => s.Has(Human.City) && s.SpaceSpec.IsCoastal);
		if( 2*gameState.Island.Boards.Length < coastalCityLandCount )
			GameOverException.Lost($"Trade Hub - {coastalCityLandCount} coastal lands have cities.");
	}

	#endregion Additional Win/Loss condition


	public override AdversaryLevel[] Levels => [
		// Escalation
		new AdversaryLevel(0, 1 , 3,3,3, "Ports Sprawl Outward", 
			"On the single board with the most Coastal Town/City, add 1 Town to the N lands with the fewest Town (N = # of players.)" )
			.WithEscalation( PortsSprawlOutward_Escalation ),

		// Level 1
		new AdversaryLevel(1, 3 , 3,4,3, "Trading Port", 
			"After Setup, in Coastal lands, Explore Cards add 1 Town instead of 1 Explorer. 'Coastal Lands' Invader cards do this for maximum 2 lands per board." ) {
			InitFunc = (gameState,_) => { gameState.InvaderDeck.Explore.Engine = new ScotlandExploreEngine(); }
		},

		// Level 2
		new AdversaryLevel(2, 4 , 4,4,3, "Seize Opportunity", "During Setup, add 1 City to land #2." ) {
			InitFunc = (gameState,_) => {
				// Add 1 City to land #2
				foreach(Board board in gameState.Island.Boards)
					board[2].ScopeSpace.Setup( Human.City, 1 );
				ActionScope.Current.Log(new SpiritIsland.Log.Debug("Seize Opportunity - adding 1 city to space 2 of each board."));
			},
		}.WithInvaderCardOrder("11-22-1-C2-33333"),

		// Level 3
		new AdversaryLevel(3,6 , 4,5,4, "Chart the Coastline", "In Coastal lands, Build Cards affect lands without Invaders, so long as there is an adjacent City." ) {
			InitFunc = (gameState,_) => gameState.InvaderDeck.Build.Engine = new ScotlandBuildEngine(),
		},

		// Level 4
		new AdversaryLevel(4, 7 , 5,5,4, "Ambition of a Minor Nation" ).WithInvaderCardOrder("11-22-3-C2-3333"),

		// Level 5
		new AdversaryLevel(5,8 , 5,6,4, "Runoff and Bilgewater", 
			"After a Ravage Action adds Blight to a Coastal Land, add 1 Blight to that board's Ocean (without cascading). Treat the Ocean as a Coastal Wetland for this rule and for Blight removal/movement." ) {
			InitFunc = (gameState,_) => {
				// After a Ravage Action adds Blight to a Coastal Land, add 1 Blight to that board's Ocean (without cascading).
				var token = new ScotlandCoastalBlightCheckToken();
				foreach(var ss in ActionScope.Current.Spaces.Where( ss => ss.SpaceSpec.IsCoastal ))
					ss.Adjust(token,1);

				// Treat the Ocean as a Coastal Wetland for this rule and for Blight removal/movement.
			}
		},

		// Level 6
		new AdversaryLevel(6, 10 , 6,6,4, "Exports Fuel Inward Growth", "After the Ravage step, add 1 Town to each Inland land that matches a Ravage card and is within 1 of town/city" ) {
			InitFunc = (gameState,_) => gameState.InvaderDeck.Ravage.Engine = new ScotlandRavageEngine(),
		}
	];

	#region Escalation - Ports Sprawl Outward

	static async Task PortsSprawlOutward_Escalation( GameState gameState ) {
		// On the single board with the most Coastal Town / City,
		var board = gameState.Island.Boards
			.OrderByDescending( b => b.Spaces.Where( s => s.IsCoastal ).ScopeTokens()
					.Sum( ss => ss.SumAny( Human.Town_City ) ) )
			.First();
		// add 1 Town to the N lands with the fewest Town( N = # of players.)
		var spacesToAddTown = board.Spaces.ScopeTokens()
			.OrderBy( ss => ss.Sum( Human.Town ) )
			.Take( gameState.Spirits.Length )
			.ToArray();
		await using(var actionScope = await ActionScope.Start(ActionCategory.Adversary))
			foreach(Space ss in spacesToAddTown)
				await ss.AddDefaultAsync( Human.Town, 1, AddReason.Build );
		ActionScope.Current.Log(new SpiritIsland.Log.Debug($"Ports Sprawl Outword: Adding 1 town to "+spacesToAddTown.SelectLabels().Join(",")));
	}

	#endregion Escalation - Ports Sprawl Outward

}
