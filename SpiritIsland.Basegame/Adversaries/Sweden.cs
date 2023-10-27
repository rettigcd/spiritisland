using System.ComponentModel.Design;

namespace SpiritIsland.Basegame;

public class Sweden : IAdversary {

	public const string Name = "Sweden";

	public ScenarioLevel[] Adjustments => new ScenarioLevel[] {
		new ScenarioLevel(1 , 3,3,3, "Swayed by the Invaders", "After Invaders Explore into each land this Phase, if that land has at least as many Invaders as Dahan, replace 1 Dahan with 1 Town." ),
		new ScenarioLevel(2 , 3,4,3, "Heavy Mining", "If the Invaders do at least 6 Damage to the land during Ravage, add an extra Blight. The additional Blight does not destroy Presence or cause cascades." ),
		new ScenarioLevel(3 , 3,4,3, "Population Pressure at Home", "During Setup, on each board add 1 City to land #4. On boards where land #4 starts with Blight, put that Blight in land #5 instead." ),
		new ScenarioLevel(5 , 3,4,3, "Fine Steel for Tools and Guns", "Town deal 3 Damage. City deal 5 Damage" ),
		new ScenarioLevel(6 , 3,4,4, "Royal Backing", "During Setup, after adding all other Invaders, discard the top card of the Invader Deck. On each board, add 1 Town to the land of that terrain with the fewest Invaders." ),
		new ScenarioLevel(7 , 4,4,4, "Mining Rush", "When Ravaging adds at least 1 Blight to a land, also add 1 Town to an adjacent land without Town/City. Cascading Blight does not cause this effect." ),
		new ScenarioLevel(8 , 4,4,5, "Prospecting Outpost", "During setup, on each board add 1 Town and 1 Blight to land #8. The Blight comes from the box, not the Blight Card." ),
	};


	public int Level { get; set; }

	public InvaderDeckBuilder InvaderDeckBuilder => InvaderDeckBuilder.Default;


	public int[] FearCardsPerLevel => Level switch {
		1 => new int[] { 3, 3, 3 },
		2 => new int[] { 3, 4, 3 },
		3 => new int[] { 3, 4, 3 },
		4 => new int[] { 3, 4, 4 },
		5 => new int[] { 4, 4, 4 },
		6 => new int[] { 4, 4, 5 },
		_ => null,
	};

	public void PreInitialization( GameState gameState ) {

		// Escalation - Swayed by the Invaders
		gameState.InvaderDeck.Explore.Engine = new SwedenExplorer();

		//	Level 1 - Heavy Mining: >=6 +1 blight
		//	The additional Blight does not destroy Presence or cause cascades.
		HeavyMining heavyMining = new HeavyMining();
		if(1 <= Level)
			gameState.AddIslandMod( heavyMining );

		// Level 3 - Fine Steel for Tools and Guns: (Town deal 3 Damage, City deal 5 Damage)
		if(3 <= Level) {
			gameState.Tokens.TokenDefaults[Human.Town] = ((HumanToken)gameState.Tokens.TokenDefaults[Human.Town]).SetAttack(3);
			gameState.Tokens.TokenDefaults[Human.City] = ((HumanToken)gameState.Tokens.TokenDefaults[Human.City]).SetAttack( 5 );
			// no logging, Ravage has plenty of it.
		}

		// Level 4 - Royal Backing - Add 1 town to lands matching 1st invader card
		if(4 <= Level) {
			var card = gameState.InvaderDeck.UnrevealedCards[0];
			gameState.InvaderDeck.UnrevealedCards.RemoveAt(0);
			var addTownSpaces = gameState.Island.Boards
				.Select(board => board.Spaces.Tokens()
					.Where( card.MatchesCard )
					.OrderBy( s => s.InvaderTotal() )
					// If there are 2 spaces with 'least # of invaders', just auto-picks one of them.
					.First()
				).ToArray();

			foreach(var leastInvaderSpace in addTownSpaces)
				leastInvaderSpace.AdjustDefault( Human.Town, 1 );

			gameState.LogDebug("Royal Backing - added 1 town to "+addTownSpaces.SelectLabels().Order().Join(","));
		}

		// Level 5 - Mining Rush: blight => +1 town on adjacent land 
		if(5 <= Level)
			heavyMining.MiningRush = true;

		// Level 6 - Prospecting Outpost: Setup +1Town +1 blight on Land 8
		if(6 <= Level) {
			var spaces = gameState.Island.Boards
				.Select( board => board[8].Tokens )
				.ToArray();
			
			foreach(SpaceState space in spaces ) {
				space.AdjustDefault( Human.Town, 1 );
				space.Blight.Adjust(1);
			}
			gameState.LogDebug("Prospecting Outpost: Adding Town/Blight to "+spaces.SelectLabels().Order().Join(","));

		}
	}

	public void PostInitialization( GameState gameState ) {

		//	Level 2	- Population Pressure at Home: 
		if(2 <= Level)
			PopulationPressureAtHome( gameState ); // Depends on tokens being already initialized

	}

	static void PopulationPressureAtHome( GameState gameState ) {
		//	Level 2	- Population Pressure at Home: 
		//	During Setup, on each board add 1 City to land #4.
		//	On boards where land #4 starts with Blight, put that Blight in land #5 instead.
		
		var additionalCitySpaces = gameState.Island.Boards
			.Select( board => board[4].Tokens)
			.ToArray();
		foreach(var space4 in additionalCitySpaces) {
			// Add City to 4
			space4.AdjustDefault( Human.City, 1 );

			// If 4 has blight, 
			if(space4.Blight.Any) {
				// bump it to 5
				space4.Blight.Adjust( -1 );
				space4.Space.Boards.First()[5].Tokens.Blight.Adjust( 1 );
			}
		}

		gameState.LogDebug($"Population Pressure At Home: adding 1 city to "+additionalCitySpaces.SelectLabels().Order().Join(","));
	}
}

//	Level 1 - Heavy Mining: >=6 +1 blight
//	The additional Blight does not destroy Presence or cause cascades.
class HeavyMining : BaseModEntity, IHandleTokenAddedAsync {

	public bool MiningRush { get; set; }

	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {

		//	Level 1 - Heavy Mining: >=6 +1 blight
		//	The additional Blight does not destroy Presence or cause cascades.
		if(args.Added == LandDamage.Token && 6 <= args.To[args.Added]) {

			var config = BlightToken.ForThisAction;
			config.DestroyPresence = false;
			config.ShouldCascade = false;

			await args.To.Blight.AddAsync( 1 );
			GameState.Current.LogDebug( "Heavy Mining: additional blight on " + args.To.Space.Text );
		}

		// Level 5 - Mining Rush: blight => +1 town on adjacent land 
		if(MiningRush)
			// When ravage adds at least 1 blight to a land
			if(args.Reason == AddReason.Ravage && args.Added == Token.Blight) {
				var noBuildAdjacents = args.To.Adjacent
					.Where( adj => !adj.HasAny( Human.Town_City ) )
					.ToArray();

				var spirit = args.To.Space.Boards[0].FindSpirit();

				var selection = await spirit.Gateway.Decision( Select.ASpace.ToPlaceToken( "Mining Rush: Place Town", noBuildAdjacents, Present.Always, args.To.GetDefault( Human.Town ) ) );
				if(selection != null) {
					selection.Tokens.AdjustDefault( Human.Town, 1 );
					GameState.Current.LogDebug( $"Mining Rush: Blight on {args.To.Space.Text} caused +1 Town on {selection.Text}." );
				}
			}

	}
}
