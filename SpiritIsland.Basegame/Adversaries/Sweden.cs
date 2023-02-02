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
		if(1 <= Level) {
			gameState.LandDamaged.ForGame.Add( async args => {
				if(6 <= args.Damage) {
					// !!! how do we take from card but not cascade?
					if(args.Space.Blight.Blocked)

					// !!! This shouldn't destroy presence?  Does it?
					await args.Space.Blight.Bind(args.Space.ActionScope).Add(1);
					args.GameState.LogDebug("Heavy Mining: additional blight on "+args.Space.Space.Text);
				}
			} );
		}

		// Level 3 - Fine Steel for Tools and Guns: (Town deal 3 Damage, City deal 5 Damage)
		if(3 <= Level) {
			gameState.Tokens.Attack[Human.Town] = 3;
			gameState.Tokens.Attack[Human.City] = 5;
			// no logging, Ravage has plenty of it.
		}

		// Level 4 - Royal Backing - Add 1 town to lands matching 1st invader card
		if(4 <= Level) {
			var card = gameState.InvaderDeck.UnrevealedCards[0];
			gameState.InvaderDeck.UnrevealedCards.RemoveAt(0);
			var addTownSpaces = gameState.Island.Boards
				.Select(board => board.Spaces
					.Select( gameState.Tokens.GetTokensFor )
					.Where( card.MatchesCard )
					.OrderBy( s => s.InvaderTotal() )
					// If there are 2 spaces with 'least # of invaders', just auto-picks one of them.
					.First()
				).ToArray();

			foreach(var leastInvaderSpace in addTownSpaces)
				leastInvaderSpace.AdjustDefault( Human.Town, 1 );

			gameState.LogDebug("Royal Backing - added 1 town to "+addTownSpaces.Select(x=>x.Space.Text).Order().Join(","));
		}

		// Level 5 - Mining Rush: blight => +1 town on adjacent land 
		if(5 <= Level) {
			var mod = new TokenAddedHandler("Sweden", async args => {
				// When ravage adds at least 1 blight to a land
				if(args.Reason == AddReason.Ravage && args.Token == Token.Blight) {
					var noBuildAdjacents = args.AddedTo.Adjacent
						.Where( adj => !adj.HasAny( Human.Town_City ) )
						.ToArray();

					var spirit = BoardCtx.FindSpirit( args.GameState, args.AddedTo.Space.Board );

					var selection = await spirit.Gateway.Decision(Select.Space.ToPlaceToken("Mining Rush: Place Town",noBuildAdjacents,Present.Always, args.AddedTo.GetDefault( Human.Town ) ) );
					if(selection != null) {
						args.GameState.Tokens[selection].AdjustDefault( Human.Town, 1 );
						args.GameState.LogDebug($"Mining Rush: Blight on {args.AddedTo.Space.Text} caused +1 Town on {selection.Text}.");
					}
				}
			}, true );
			gameState.AddToAllActiveSpaces( mod );
		}

		// Level 6 - Prospecting Outpost: Setup +1Town +1 blight on Land 8
		if(6 <= Level) {
			var spaces = gameState.Island.Boards
				.Select( board => gameState.Tokens[board[8]] )
				.ToArray();
			
			foreach(SpaceState space in spaces ) {
				space.AdjustDefault( Human.Town, 1 );
				space.Blight.Adjust(1);
			}
			gameState.LogDebug("Prospecting Outpost: Adding Town/Blight to "+spaces.Select(s=>s.Space.Text).Order().Join(","));

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
			.Select( board => gameState.Tokens[board[4]])
			.ToArray();
		foreach(var space4 in additionalCitySpaces) {
			// Add City to 4
			space4.AdjustDefault( Human.City, 1 );

			// If 4 has blight, 
			if(space4.Blight.Any) {
				// bump it to 5
				space4.Blight.Adjust( -1 );
				gameState.Tokens[space4.Space.Board[5]].Blight.Adjust( 1 );
			}
		}

		gameState.LogDebug($"Population Pressure At Home: adding 1 city to "+additionalCitySpaces.Select(s=>s.Space.Text).Order().Join(","));
	}
}

