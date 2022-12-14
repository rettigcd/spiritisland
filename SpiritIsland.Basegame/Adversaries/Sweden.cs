namespace SpiritIsland.Basegame;

public class Sweden : IAdversary {

	public const string Name = "Sweden";

	public ScenarioLevel[] Adjustments => new ScenarioLevel[] {
		new ScenarioLevel(1 , 3,3,3, "Swayed by the Invaders", " After Invaders Explore into each land this Phase, if that land has at least as many Invaders as Dahan, replace 1 Dahan with 1 Town." ),
		new ScenarioLevel(2 , 3,4,3, "Heavy Mining", "If the Invaders do at least 6 Damage to the land during Ravage, add an extra Blight. The additional Blight does not destroy Presence or cause cascades." ),
		new ScenarioLevel(3 , 3,4,3, "Population Pressure at Home", " During Setup, on each board add 1 City to land #4. On boards where land #4 starts with Blight, put that Blight in land #5 instead." ),
		new ScenarioLevel(5 , 3,4,3, "Fine Steel for Tools and Guns", "Town deal 3 Damage. City deal 5 Damage" ),
		new ScenarioLevel(6 , 3,4,4, "Royal Backing", "During Setup, after adding all other Invaders, discard the top card of the Invader Deck. On each board, add 1 Town to the land of that terrain with the fewest Invaders." ),
		new ScenarioLevel(7 , 4,4,4, "Mining Rush", "When Ravaging adds at least 1 Blight to a land, also add 1 Town to an adjacent land without Town/City. Cascading Blight does not cause this effect." ),
		new ScenarioLevel(8 , 4,4,5, "Prospecting Outpost", "During setup, on each board add 1 Town and 1 Blight to land #8. The Blight comes from the box, not the Blight Card." ),
	};


	public int Level { get; set; }

	public int[] InvaderCardOrder => null;

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

		//	Level 1
		//	( 2 ) 9( 3 / 3 / 3 )   Heavy Mining: 
		//	If the Invaders do at least 6 Damage to the land during Ravage, add an extra Blight.
		//	The additional Blight does not destroy Presence or cause cascades.
		if(1 <= Level) {
			gameState.LandDamaged.ForGame.Add( async args => {
				if(6<=args.Damage)
					// !!! how do we take from card but not cascade?
					await gameState.Tokens[args.Space].Blight.Bind(args.ActionId).Add(1);
			} );
		}

		// Level 3
		// (5)	10 (3/4/3)	Fine Steel for Tools and Guns:
		// Town deal 3 Damage
		// City deal 5 Damage
		if(3 <= Level) {
			// !!! Something about this is causing Invaders to draw with red slashes
			gameState.Tokens.TokenDefaults[Invader.Town] = new HealthToken( Invader.Town, gameState, 2, 3 );
			gameState.Tokens.TokenDefaults[Invader.City] = new HealthToken(Invader.City, gameState, 3, 5);
		}

		// Level 4
		// (6)	11 (3/4/4)	Royal Backing: 
		// During Setup, after adding all other Invaders, discard the top card of the Invader Deck.
		// On each board, add 1 Town to the land of that terrain with the fewest Invaders.
		if(4 <= Level) {
			var card = gameState.InvaderDeck.UnrevealedCards[0];
			gameState.InvaderDeck.UnrevealedCards.RemoveAt(0);
			foreach(var board in gameState.Island.Boards) {
				// If there are 2 spaces with 'least # of invaders', just auto-picks one of them.
				var leastInvaderSpace = board.Spaces.Where(card.MatchesCard).OrderBy(s=>gameState.Tokens[s].InvaderTotal()).First();
				gameState.Tokens[leastInvaderSpace].AdjustDefault(Invader.Town,1);
			}
		}

		// Level 5
		// (7)	12 (4/4/4)	Mining Rush: 
		// When Ravaging adds at least 1 Blight to a land, also add 1 Town to an adjacent land without Town/City.
		// Cascading Blight does not cause this effect.
		if(5 <= Level) {
			gameState.Tokens.TokenAdded.ForGame.Add( args => { 
				if(args.Reason == AddReason.Ravage && args.Token == TokenType.Blight) {
					var noBuildAdjacents = args.Space.Adjacent
						.Where( adj=>!adj.HasAny(Invader.Town,Invader.City) )
						.ToArray();
					
					// !!! user select which space to add it to
					var selection = noBuildAdjacents.FirstOrDefault();

					if(selection != null)
						selection.AdjustDefault(Invader.Town,1);
				}
			} );
		}

		// Level 6
		// (8)	13 (4/4/5)	Prospecting Outpost: 
		// During setup, on each board add 1 Town and 1 Blight to land #8.
		// The Blight comes from the box, not the Blight Card.
		if(6 <= Level) {
			foreach(var board in gameState.Island.Boards) {
				var tokens = gameState.Tokens[board[8]];
				tokens.AdjustDefault( Invader.Town, 1 );
				tokens.Blight.Adjust(1);
			}
		}
	}

	public void PostInitialization( GameState gameState ) {
		gameState.InvaderDeck.ReplaceCards( card => new SwedenInvaderCard( card ) );

		//	Level 2	
		//	(3)	10 (3/4/3)	Population Pressure at Home: 
		//	During Setup, on each board add 1 City to land #4.
		//	On boards where land #4 starts with Blight, put that Blight in land #5 instead.
		if(2 <= Level) {
			foreach(var board in gameState.Island.Boards) {
				// Add City to 4
				var space4 = gameState.Tokens[board[4]];
				space4.AdjustDefault( Invader.City, 1 );

				// If 4 has blight, 
				if( space4.Blight.Any ) {
					// bump it to 5
					space4.Blight.Adjust(-1);
					gameState.Tokens[board[5]].Blight.Adjust(1);
				}
			}
		}

	}

}

class SwedenInvaderCard : InvaderCard {
	public SwedenInvaderCard( InvaderCard orig ):base(orig) { }

	public override async Task Explore( GameState gs ) {
		SpaceState[] tokenSpacesToExplore = PreExplore( gs );
		await DoExplore( gs, tokenSpacesToExplore );
		if( HasEscalation )
			Escalation( gs, tokenSpacesToExplore );
	}
	static void Escalation( GameState gs, SpaceState[] exploredTokenSpaces ) {
		// Swayed by the Invaders:
		// After Invaders Explore into each land this Phase,
		// if that land has at least as many Invaders as Dahan,
		// replace 1 Dahan with 1 Towni.
		foreach(var tokens in exploredTokenSpaces) {
			var dahan = tokens.Dahan;
			if(0 < dahan.Count && dahan.Count <= tokens.InvaderTotal()) {
				var dahanToConvert = dahan.Keys.OrderBy(x=>x.RemainingHealth).First();
				var townToAdd = tokens.GetDefault( Invader.Town ).AddDamage( dahanToConvert.Damage );

				dahan.Adjust(dahanToConvert,-1);
				tokens.Adjust(townToAdd,1);
				gs.Log( new InvaderActionEntry($"Escalation: {tokens.Space.Text} replace {dahanToConvert} with {townToAdd}"));
			}
		}
	}
}

