namespace SpiritIsland.Basegame;

public class England : AdversaryBase, IAdversary {

	public const string Name = "England";

	public override AdversaryLevel[] Levels => _levels;

	public override AdversaryLossCondition LossCondition => ProudAndMightyCapital;

	readonly AdversaryLevel[] _levels = new AdversaryLevel[] {
		// Escalation
		new AdversaryLevel(0, 1, 3,3,3, "Building Boom", " On each board with Town/City, Build in the land with the most Town/City." )
			.WithEscalation( BuildingBoom_Escalation ),

		// Level 1
		new AdversaryLevel(1, 3, 3,4,3, "Indentured Servants Earn Land",
			"Invader Build Cards affect matching lands without Invaders if they are adjacent to at least 2 Towns/Cities."
		){ InitFunc = (gs,_) => {
			gs.InvaderDeck.Build.Engine = new EnglandBuilder(); 
		} },

		// Level 2
		new AdversaryLevel(2, 4, 4,4,3, "Criminals and Malcontents", 
			"During Setup, on each board add 1 City to land #1 and 1 Town to land #2."
		){ InitFunc = (gs,_) => {
			foreach(var board in gs.Island.Boards) {
				board[1].Tokens.Setup( Human.City, 1 );
				board[2].Tokens.Setup( Human.Town, 1 );
			}
		}},

		// Level 3
		new AdversaryLevel(3, 6, 4,5,4, "High Immigration(I)", 
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
					gameState.AddTimePassesAction( highBuildSlot ); // removes itself when it gets Stage-II invaders.
			}
		},

		// Level 4
		new AdversaryLevel(4, 7, 4,5,5, "High Immigration(full)",  
			"The extra Build tile remains out the entire game."
		), // no action - done in Level3

		// Level 5
		new AdversaryLevel(5, 9 , 4,5,5, "Local Autonomy",  "Towns/Citys have +1 Health" ) {
			InitFunc = (gameState,_) => {
				gameState.Tokens.TokenDefaults[Human.City] = new HumanToken( Human.City, 4 );
				gameState.Tokens.TokenDefaults[Human.Town] = new HumanToken( Human.Town, 3 );
			}
		},

		// Level 6
		new AdversaryLevel(6, 11, 4,5,4, "Independent Resolve", 
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
		// Building Boom: On each board with Town / City, Build in the land with the most Town / City
		await Build
			.On().OneLandPerBoard().Which( Has_TheMostTownsAndCities )
			.ForEachBoard().Which( BoardHas.TownOrCity )
			.ActAsync( gs );
	}

	static SpaceAction Build => new SpaceAction( "Build (Escalation - Building Boom)", 
		x => { var gs = GameState.Current; gs.InvaderDeck.Build.Engine.Do1Build(gs,x.Tokens); }
	);

	static CtxFilter<TargetSpaceCtx> Has_TheMostTownsAndCities => new CtxFilter<TargetSpaceCtx>(" has the most Town/City", HasTheMostTownsOrCities_Imp );

	static IEnumerable<TargetSpaceCtx> HasTheMostTownsOrCities_Imp( IEnumerable<TargetSpaceCtx> src ) {
		int maxCount = src.Max( src => src.Tokens.SumAny(Human.Town_City));
		return src.Where( s => s.Tokens.SumAny( Human.Town_City ) == maxCount );
	}

	#endregion

	#region Loss Condition - Proud and Mighty Capital

	static AdversaryLossCondition ProudAndMightyCapital => new AdversaryLossCondition(
		"Proud & Mighty Capital: If 7 or more Town/City are ever in a single land, the Invaders win.",
		ProudAndMightyCapitalImp
	);

	static void ProudAndMightyCapitalImp( GameState gs ) {
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

	public class HighImmegrationSlot : BuildSlot, IRunWhenTimePasses {
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
				int currentFearCardsResolved = gs.Fear.ResolvedCardCount;
				if(currentFearCardsResolved == lastCountOfFearCardsResolved)
					await base.Execute( gs );
				lastCountOfFearCardsResolved = currentFearCardsResolved;
			}
		}

		#region IRunWhenTimePasses

		/// <summary> Removes itself when after it gets Stage-II invaders </summary>
		/// <remarks> Don't add this to the RunWhenTimePasses list for higher Adversary level.</remarks>
		Task IRunWhenTimePasses.TimePasses( GameState gameState ) {
			if(Cards.Any( c => c.InvaderStage == 2 )) {
				var deck = gameState.InvaderDeck;
				deck.Discards.AddRange( Cards );
				deck.ActiveSlots.RemoveAt( 0 );
				_removeAfterRun = true;
			}
			return Task.CompletedTask;
		}
		bool IRunWhenTimePasses.RemoveAfterRun => _removeAfterRun;
		bool _removeAfterRun = false;
		#endregion IRunWhenTimePasses
	}

	#endregion Level 3 - High Immegration

}