namespace SpiritIsland.Basegame;

// !!! Additional Loss Condition
// Proud & Mighty Capital: If 7 or more Town/City are ever in a single land, the Invaders win.

public class England : IAdversary {

	public int Level { get; set; }

	public int[] InvaderCardOrder => null;

	public int[] FearCardsPerLevel => Level switch {
		1 => new int[] { 3, 4, 3 },
		2 => new int[] { 4, 4, 3 },
		3 => new int[] { 4, 5, 4 },
		4 => new int[] { 4, 5, 5 },
		5 => new int[] { 4, 5, 5 },
		6 => new int[] { 4, 5, 4 },
		_ => null,
	};

	public void Adjust( GameState gameState ) {
		if( Level >= 2) {
			// During Setup, on each board add 1 City to land #1 and 1 Town to land #2.
			foreach(var board in gameState.Island.Boards) {
				gameState.Tokens[board[1]].AdjustDefault( Invader.City, 1 );
				gameState.Tokens[board[2]].AdjustDefault( Invader.Town, 1 );
			}
		}
		if( Level >= 3) {
			var highBuildSlot = new HighImmegrationSlot( Level );
			gameState.InvaderDeck.Slots.Insert( 0, highBuildSlot );
			if( Level == 3)
				HighImmegrationSlot.RemoveForLevel2Invaders( gameState, highBuildSlot );
		}
		if( Level >= 5) {
			gameState.Tokens.TokenDefaults[Invader.City] = new HealthToken(Invader.City,4);
			gameState.Tokens.TokenDefaults[Invader.Town] = new HealthToken( Invader.Town, 3 );
		}
		if( Level == 6) {
			gameState.Fear.PoolMax += gameState.Spirits.Length;
		}
	}

	public void AdjustInvaderDeck( InvaderDeck deck ) {
		deck.ReplaceCards( card => new EnglandInvaderCard( card, Level > 0 ) );
	}

	// We are NOT wrapping the source card.
	// Instead, since we know it is a standard InvaderCard,
	// We can derive from InvaderCard and override the behavior we want to change.
	// If the input were not an InvaderCard, we would have to wrap the card passed in so as to not drop any functionality.
	public class EnglandInvaderCard : InvaderCard {
		readonly bool expandedBuild;
		public EnglandInvaderCard(InvaderCard card,bool expandedBuild):base(card) {
			this.expandedBuild = expandedBuild;
		}
		protected override bool ShouldBuildOnSpace( TokenCountDictionary tokens, GameState gameState ) {
			int cityTownCounts(Space space) => gameState.Tokens[space].SumAny( Invader.Town, Invader.City );
			bool adjacentTo2OrMoreCitiesOrTowns(Space space) => 2 <= space.Adjacent.Sum( adj => cityTownCounts( adj ) );
			return base.ShouldBuildOnSpace( tokens, gameState ) 
				|| expandedBuild && adjacentTo2OrMoreCitiesOrTowns(tokens.Space);
		}
		public override async Task Explore( GameState gs ) {
			await base.Explore( gs );
			if(HasEscalation)
				await Escalation( gs );
		}
		static async Task Escalation( GameState gs ) {
			// Escalation Stage II
			// Building Boom: On each board with Towni / City, Build in the land with the most Town / City

			// Finds the space on each board with the most town/city.
			// When multiple town/city have max #, picks the one closests to the coast (for simplicity)
			Space[] buildSpaces = gs.Island.AllSpaces
				.Select( s => new { Space = s, Count = gs.Tokens[s].SumAny( Invader.Town, Invader.City ) } )
				.Where( x => x.Count > 0 )
				.GroupBy( x => x.Space.Board )
				.Select( grp => grp.OrderByDescending( x => x.Count ).ThenBy( x => x.Space.Text ).First().Space )
				.ToArray();

			await England.SimplifiedBuild( gs, buildSpaces );
		}

	}

	public class HighImmegrationSlot : BuildSlot {
		public HighImmegrationSlot( int level ){
			this.repeatWhenNoFearResolved = level == 6;
		}
		readonly bool repeatWhenNoFearResolved;
		int lastCountOfFearCardsResolved = 0;
		public override async Task Execute( GameState gs ) {
			// Do Normal Build
			await base.Execute( gs );

			// If no fear cards were Resolved
			if(repeatWhenNoFearResolved) {
				int currentFearCardsResolved = gs.Fear.ResolvedCards;
				if(currentFearCardsResolved == lastCountOfFearCardsResolved)
					await base.Execute( gs );
				lastCountOfFearCardsResolved = currentFearCardsResolved;
			}
		}

		static public void RemoveForLevel2Invaders( GameState gameState, HighImmegrationSlot highBuildSlot ) {
			gameState.TimePasses_WholeGame += ( GameState gs ) => {
				if(highBuildSlot != null && highBuildSlot.Cards.Any( c => c.InvaderStage == 2 )) {
					var deck = gs.InvaderDeck;
					deck.Discards.AddRange( highBuildSlot.Cards );
					deck.Slots.RemoveAt( 0 );
					highBuildSlot = null;
				}
			};
		}

	}

	public static async Task SimplifiedBuild( GameState gs, Space[] buildSpaces ) {

		// This is a HACK.  Simplify the Build Engine and use it instead.

		// There are 2 problems with this code
		// !!! This is mostly a duplicate of Build, and should use the GameState.BuildEngine if it weren't so complicated.
		// !!! Stop Builds (Paralzying Fright / Infestation of Spiders / etc) should be able to stop these builds as 'Build' is an Invader action.
		foreach(var space in buildSpaces) {
			var tokens = gs.Tokens[space];
			int cityCount = tokens.Sum( Invader.City );
			int townCount = tokens.Sum( Invader.Town );
			var newTokenClass = townCount <= cityCount ? Invader.Town : Invader.City;
			await tokens.AddDefault( newTokenClass, 1, Guid.NewGuid(), AddReason.Build );
		}
	}

}

static public class InvaderDeckExtensions {
	public static void ReplaceCards( this InvaderDeck deck, Func<InvaderCard, IInvaderCard> replacer ) {
		for(int i = 0; i < deck.UnrevealedCards.Count; ++i) {
			if(deck.UnrevealedCards[i] is not InvaderCard simpleInvaderCard)
				throw new InvalidOperationException( "We can only apply Adversary modification to original (simple) Invader Cards" );
			deck.UnrevealedCards[i] = replacer( simpleInvaderCard );
		}
	}

}

/*

1	(3)	10 (3/4/3)	Indentured Servants Earn Land: 
	Invader Build Cards affect matching lands without Invaders if they are adjacent to at least 2 Towns/Citys.

2	(4)	11 (4/4/3)	Criminals and Malcontents: 
	During Setup, on each board add 1 City to land #1 and 1 Town to land #2.

3	(6)	13 (4/5/4)	High Immigration (I): 
	Put the "High Immigration" tile on the Invader board, to the left of "Ravage".
	The Invaders take this Build action each Invader phase before Ravaging.
	Cards slide left from Ravage to it, and from it to the discard pile. 
	Remove the tile when a Stage II card slides onto it, putting that card in the discard.
	// Create Invader Card Action Piles.

4	(7)	14 (4/5/5)	High Immigration (full): 
	The extra Build tile remains out the entire game.

5	(9)	14 (4/5/5)	Local Autonomy: 
	Towns/Citys have +1 Health.

6	(11)	13 (4/5/4)	Independent Resolve: 
	During Setup, add an additional Fear to the Fear Pool per player in the game. 

	During any Invader Phase where you resolve no Fear Cards, perform the Build from High Immigration twice.
	(This has no effect if no card is on the extra Build slot.)

*/