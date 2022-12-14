namespace SpiritIsland.Basegame;

// !!! Additional Loss Condition
// Proud & Mighty Capital: If 7 or more Town/City are ever in a single land, the Invaders win.

public class England : IAdversary {

	public const string Name = "England";

	public ScenarioLevel[] Adjustments => new ScenarioLevel[] {
		new ScenarioLevel(1 , 3,3,3, "Building Boom", " On each board with Town/City, Build in the land with the most Town/City." ),

		//1	(3)	10 (3/4/3)	Indentured Servants Earn Land: 
		//	Invader Build Cards affect matching lands without Invaders if they are adjacent to at least 2 Towns/Citys.
		new ScenarioLevel(3 , 3,4,3, "Indentured Servants Earn Land",           "Invader Build Cards affect matching lands without Invaders if they are adjacent to at least 2 Towns/Citys." ),

		//2	(4)	11 (4/4/3)	Criminals and Malcontents: 
		//	During Setup, on each board add 1 City to land #1 and 1 Town to land #2.
		new ScenarioLevel(4 , 4,4,3, "Criminals and Malcontents",   "During Setup, on each board add 1 City to land #1 and 1 Town to land #2." ),

		//3	(6)	13 (4/5/4)	High Immigration(I): 
		//	Put the "High Immigration" tile on the Invader board, to the left of "Ravage".
		//	The Invaders take this Build action each Invader phase before Ravaging.
		//	Cards slide left from Ravage to it, and from it to the discard pile.
		//	Remove the tile when a Stage II card slides onto it, putting that card in the discard.
		new ScenarioLevel(6 , 4,5,4, "High Immigration(I)", "Perform a build prior to Ravage" ),

		//4	(7)	14 (4/5/5)	High Immigration(full): 
		//	The extra Build tile remains out the entire game.
		new ScenarioLevel(7 , 4,5,5, "High Immigration(full)",  "The extra Build tile remains out the entire game." ),

		//5	(9)	14 (4/5/5)	Local Autonomy: 
		//	Towns/Citys have +1 Health.
		new ScenarioLevel(9 , 4,5,5, "Local Autonomy",  "Towns/Citys have +1 Health" ),

		//6	(11)	13 (4/5/4)	Independent Resolve: 
		//	During Setup, add an additional Fear to the Fear Pool per player in the game.
		//	During any Invader Phase where you resolve no Fear Cards, perform the Build from High Immigration twice.
		//	(This has no effect if no card is on the extra Build slot.)
		new ScenarioLevel(11, 4,5,4, "Independent Resolve", "FearPool+=1, NoFear=>Additional build" ),
	};

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

	public void PreInitialization( GameState gameState ) {
		if( 2 <= Level ) {
			// During Setup, on each board add 1 City to land #1 and 1 Town to land #2.
			foreach(var board in gameState.Island.Boards) {
				gameState.Tokens[board[1]].AdjustDefault( Invader.City, 1 );
				gameState.Tokens[board[2]].AdjustDefault( Invader.Town, 1 );
			}
		}
		if( 5 <= Level ) {
			gameState.Tokens.TokenDefaults[Invader.City] = new HealthToken(Invader.City, gameState, 4);
			gameState.Tokens.TokenDefaults[Invader.Town] = new HealthToken( Invader.Town, gameState, 3 );
		}
		if( Level == 6) {
			gameState.Fear.PoolMax += gameState.Spirits.Length;
		}
	}

	public void PostInitialization( GameState gs ) {
		gs.InvaderDeck.ReplaceCards( card => new EnglandInvaderCard( card, Level > 0 ) );
		if(3 <= Level) {
			var highBuildSlot = new HighImmegrationSlot( Level );
			gs.InvaderDeck.Slots.Insert( 0, highBuildSlot );
			if(Level == 3)
				HighImmegrationSlot.RemoveForLevel2Invaders( gs, highBuildSlot );
		}
	}

	// We are NOT wrapping the source card.
	// Instead, since we know it is a standard InvaderCard,
	// We can derive from InvaderCard and override the behavior we want to change.
	// If the input were not an InvaderCard, we would have to wrap the card passed in so as to not drop any functionality.
	public class EnglandInvaderCard : InvaderCard {
		readonly bool expandedBuild;

		#region constructor
		public EnglandInvaderCard(InvaderCard card,bool expandedBuild):base(card) {
			this.expandedBuild = expandedBuild;
		}
		#endregion

		protected override bool ShouldBuildOnSpace( SpaceState tokens ) {
			static int cityTownCounts(SpaceState space) => space.SumAny( Invader.Town, Invader.City );
			static bool isAdjacentTo2OrMoreCitiesOrTowns(SpaceState tokens) => !tokens.Has(TokenType.Isolate) 
				&& 2 <= tokens.Adjacent.Sum( adj => cityTownCounts( adj ) );

			return base.ShouldBuildOnSpace( tokens )
				|| expandedBuild && isAdjacentTo2OrMoreCitiesOrTowns(tokens);
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
			Space[] buildSpaces = gs.AllActiveSpaces
				.Select( ss => new { ss.Space, Count = ss.SumAny( Invader.Town, Invader.City ) } )
				.Where( x => x.Count > 0 )
				.GroupBy( x => x.Space.Board )
				.Select( grp => grp.OrderByDescending( x => x.Count ).ThenBy( x => x.Space.Text ).First().Space )
				.ToArray();

			await England.EscalationBuild( gs, buildSpaces );
		}

	}

	public class HighImmegrationSlot : BuildSlot {
		public HighImmegrationSlot( int level ):base("High Immigration"){
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
				return Task.CompletedTask;
			};
		}

	}

	public static async Task EscalationBuild( GameState gs, Space[] buildSpaces ) {

		foreach(var space in buildSpaces) {
			var tokens = gs.Tokens[space];
			tokens.Adjust( TokenType.DoBuild, 1 );
			var buildEngine = gs.GetBuildEngine( tokens );
			string result = await buildEngine.DoBuilds();
			gs.Log( new InvaderActionEntry( $"Escalation: {space.Text}: " + result ) );
		}

	}

}
