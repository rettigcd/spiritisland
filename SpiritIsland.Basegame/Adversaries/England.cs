namespace SpiritIsland.Basegame;

public class England : AdversaryBase, IAdversary {

	public const string Name = "England";

	public override AdversaryLevel[] Levels => _levels;

	AdversaryLevel[] _levels = new AdversaryLevel[] {
		// Escalation
		new AdversaryLevel(1, 3,3,3, "Building Boom", " On each board with Town/City, Build in the land with the most Town/City." )
			.WithEscalation( BuildingBoom_Escalation )
			.WithWinLossCondition( ProudAndMightyCapital ),

		// Level 1
		new AdversaryLevel(3, 3,4,3, "Indentured Servants Earn Land",
			"Invader Build Cards affect matching lands without Invaders if they are adjacent to at least 2 Towns/Cities."
		){ InitFunc = (gs,_) => {
			gs.LogDebug("Indentured Servants Earn Land => Builds occur in spaces adjacent to at least 2 Towns/Cities.");
			gs.InvaderDeck.Build.Engine = new EnglandBuilder(); 
		} },

		// Level 2
		new AdversaryLevel(4, 4,4,3, "Criminals and Malcontents", 
			"During Setup, on each board add 1 City to land #1 and 1 Town to land #2."
		){ InitFunc = (gs,_) => {
			foreach(var board in gs.Island.Boards) {
				board[1].Tokens.AdjustDefault( Human.City, 1 );
				board[2].Tokens.AdjustDefault( Human.Town, 1 );
			}
			gs.LogDebug("Criminals & Malcontents: Adding additional city to #1 and town to #2");
		}},

		// Level 3
		new AdversaryLevel(6, 4,5,4, "High Immigration(I)", 
			"Perform a build prior to Ravage"
			//	Put the "High Immigration" tile on the Invader board, to the left of "Ravage".
			//	The Invaders take this Build action each Invader phase before Ravaging.
			//	Cards slide left from Ravage to it, and from it to the discard pile.
			//	Remove the tile when a Stage II card slides onto it, putting that card in the discard.
		){
			InitFunc = (gameState,adversary) => {
				var highBuildSlot = new HighImmegrationSlot( adversary.Level );
				gameState.InvaderDeck.ActiveSlots.Insert( 0, highBuildSlot );
				// only remove it for level 3, level 4 and up keeps it
				if(adversary.Level < 4)
					HighImmegrationSlot.RemoveForLevel2Invaders( gameState, highBuildSlot );
			}
		},

		// Level 4
		new AdversaryLevel(7, 4,5,5, "High Immigration(full)",  
			"The extra Build tile remains out the entire game."
		), // no action - done in Level3

		// Level 5
		new AdversaryLevel(9 , 4,5,5, "Local Autonomy",  "Towns/Citys have +1 Health" ) {
			InitFunc = (gameState,_) => {
				gameState.Tokens.TokenDefaults[Human.City] = new HumanToken( Human.City, 4 );
				gameState.Tokens.TokenDefaults[Human.Town] = new HumanToken( Human.Town, 3 );
			}
		},

		// Level 6
		new AdversaryLevel(11, 4,5,4, "Independent Resolve", 
			"FearPool+=1, NoFear=>Additional build"
		//	During Setup, add an additional Fear to the Fear Pool per player in the game.
		//	During any Invader Phase where you resolve no Fear Cards, perform the Build from High Immigration twice.
		//	(This has no effect if no card is on the extra Build slot.)
		) {
			InitFunc = (gameState,_) => gameState.Fear.PoolMax += gameState.Spirits.Length,
		},
	};

	#region Escalation - Building Boom

	static async Task BuildingBoom_Escalation( GameState gs ) {
		// Escalation Stage II
		// Building Boom: On each board with Town / City, Build in the land with the most Town / City

		// Finds the space on each board with the most town/city.
		SpaceState[] buildSpaces = gs.Island.Boards
			.Select( FindSpaceWithMostTownsOrCities )
			.Where( x=>x!=null )
			.Distinct() // for multi-space
			.ToArray();

		var buildOnce = gs.InvaderDeck.Build.Engine.Do1Build;
		foreach(var space in buildSpaces)
			await buildOnce( gs, space );

	}

	static SpaceState FindSpaceWithMostTownsOrCities( Board board ) {
		// When multiple town/city have max #,
		// picks the one closests to the coast (for simplicity)
		return board.Spaces.Tokens()
			.Select( ss => new { SpaceState = ss, Count = ss.SumAny( Human.Town_City ) } )
			.Where( x => 0 < x.Count )
			.OrderByDescending( x => x.Count )
			.ThenBy( x => x.SpaceState.Space.Text ) // closest to the coast
			.FirstOrDefault()?.SpaceState;
	}

	#endregion

	#region Win/Loss Condition - Proud and Mighty Capital

	static void ProudAndMightyCapital( GameState gs ) {
		const string Name = "Proud & Mighty Capital";
		// Additional Loss Condition
		// Proud & Mighty Capital: If 7 or more Town/City are ever in a single land, the Invaders win.
		static bool IsCapital(SpaceState s) => 7 <= s.SumAny( Human.Town_City );
		var capital = gs.Spaces_Unfiltered.FirstOrDefault( IsCapital );
		if( capital != null )
			GameOverException.Lost($"{Name} on {capital.Space.Text}");
	}

	#endregion Win/Loss Condition - Proud and Mighty Capital

	#region Level 3 - High Immegration

	public class HighImmegrationSlot : BuildSlot {
		public HighImmegrationSlot( int level ):base("High Immigration"){
			_repeatWhenNoFearResolved = level == 6;
		}

		readonly bool _repeatWhenNoFearResolved;
		int lastCountOfFearCardsResolved = 0;
		public override async Task Execute( GameState gs ) {

			// Do Normal Build
			await base.Execute( gs );

			// !!! Instead of replacing the entire BuildSlot, just hook into the BuildSlot.Complete event to do the below stuff

			// If no fear cards were Resolved
			if(_repeatWhenNoFearResolved) {
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
					deck.ActiveSlots.RemoveAt( 0 );
					highBuildSlot = null;
				}
				return Task.CompletedTask;
			};
		}

	}

	#endregion Level 3 - High Immegration

}