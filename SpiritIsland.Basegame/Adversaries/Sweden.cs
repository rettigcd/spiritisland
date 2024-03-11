namespace SpiritIsland.Basegame;

public class Sweden : AdversaryBase, IAdversary {

	public const string Name = "Sweden";

	public override AdversaryLevel[] Levels => _levels;

	readonly AdversaryLevel[] _levels = [
		// Escalation
		new AdversaryLevel(0,1 , 3,3,3, "Swayed by the Invaders", "After Invaders Explore into each land this Phase, if that land has at least as many Invaders as Dahan, replace 1 Dahan with 1 Town." ){
			InitFunc = (gs,_) => gs.InvaderDeck.Explore.Engine = new SwedenExplorer()
		},

		// Level 1 
		new AdversaryLevel(1, 2 , 3,4,3, "Heavy Mining", 
			"If the Invaders do at least 6 Damage to the land during Ravage, add an extra Blight. The additional Blight does not destroy Presence or cause cascades."
		) {
			InitFunc = (gs,adv) => {
				if(5 <= adv.Level) return; // Level 5 changes HeavyMining config
				gs.AddIslandMod( new SwedenHeavyMining() );
			}
		},

		// Level 2
		new AdversaryLevel(2, 3 , 3,4,3, "Population Pressure at Home", 
			"During Setup, on each board add 1 City to land #4. On boards where land #4 starts with Blight, put that Blight in land #5 instead." 
		) {
			AdjustFunc = (gameState,_) => {
				var additionalCitySpaces = gameState.Island.Boards
					.Select( board => board[4].ScopeTokens)
					.ToArray();
				foreach(var space4 in additionalCitySpaces) {
					// Add City to 4
					space4.Setup( Human.City, 1 );

					// If 4 has blight, 
					if(space4.Blight.Any) {
						// bump it to 5
						space4.Blight.Adjust( -1 );
						space4.Space.Boards.First()[5].ScopeTokens.Blight.Adjust( 1 );
					}
				}
			}
		},

		// Level 3
		new AdversaryLevel(3, 5 , 3,4,3, "Fine Steel for Tools and Guns", "Town deal 3 Damage. City deal 5 Damage"
		) {
			InitFunc = (gameState,_) => {
				gameState.Tokens.TokenDefaults[Human.Town] = ((HumanToken)gameState.Tokens.TokenDefaults[Human.Town]).SetAttack(3);
				gameState.Tokens.TokenDefaults[Human.City] = ((HumanToken)gameState.Tokens.TokenDefaults[Human.City]).SetAttack( 5 );
			}
		},

		// Level 4
		new AdversaryLevel(4, 6 , 3,4,4, "Royal Backing", 
			"During Setup, after adding all other Invaders, discard the top card of the Invader Deck. On each board, add 1 Town to the land of that terrain with the fewest Invaders."
		) {
			InitFunc = (gameState,_) => {
				var card = gameState.InvaderDeck.UnrevealedCards[0];
				gameState.InvaderDeck.UnrevealedCards.RemoveAt(0);
				var addTownSpaces = gameState.Island.Boards
					.Select(board => board.Spaces.ScopeTokens()
						.Where( card.MatchesCard )
						.OrderBy( s => s.InvaderTotal() )
						// If there are 2 spaces with 'least # of invaders', just auto-picks one of them.
						.First()
					).ToArray();

				foreach(var leastInvaderSpace in addTownSpaces)
					leastInvaderSpace.Setup( Human.Town, 1 );
			}
		},

		// Level 5
		new AdversaryLevel(5, 7 , 4,4,4, "Mining Rush", "When Ravaging adds at least 1 Blight to a land, also add 1 Town to an adjacent land without Town/City. Cascading Blight does not cause this effect."
		) {
			InitFunc = ( gameState, _ ) => {
				gameState.AddIslandMod( new SwedenHeavyMining(){ MiningRush = true } );
			}
		},

		// Level 6
		new AdversaryLevel(6, 8 , 4,4,5, "Prospecting Outpost", 
			"During setup, on each board add 1 Town and 1 Blight to land #8. The Blight comes from the box, not the Blight Card."
		) {
			InitFunc = ( gameState, _) => {
				var spaces = gameState.Island.Boards
					.Select( board => board[8].ScopeTokens )
					.ToArray();

				foreach(SpaceState space in spaces ) {
					space.Setup( Human.Town, 1 );
					space.Blight.Adjust(1);
				}
			}
		}
	];

}
