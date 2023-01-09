namespace SpiritIsland.JaggedEarth;

public class HabsburgMonarchy : IAdversary {

	public const string Name = "Habsburg Monarchy";

	public int Level { get; set; }

	public InvaderDeckBuilder InvaderDeckBuilder => new InvaderDeckBuilder( Level switch {
		>=3 => new int[] { 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3 },
		_ => null // use default
	} );


	public int[] FearCardsPerLevel => Level switch {
		1 => new int[] { 3, 4, 3 },
		2 => new int[] { 4, 5, 2 },
		3 => new int[] { 4, 5, 3 },
		4 => new int[] { 4, 5, 3 },
		5 => new int[] { 4, 6, 3 },
		6 => new int[] { 5, 6, 3 },
		_ => null
	};


	public ScenarioLevel[] Adjustments => new ScenarioLevel[] {
		// Level 0 - Escalation
		new ScenarioLevel(2 , 3,3,3, "Seek Prime Territory", "On each board with 4 or fewer Blight, add 1 Town to a land without Town/Blight. On each board with 2 or fewer Blight, do so again." ),
		// Level 1
		new ScenarioLevel(3 , 3,4,3, "Migratory Herders", "After the normal Build Step: In each land matching a Build Card, Gather 1 Town from a land not matching a Build Card. (In board/land order.)" ),
		// Level 2
		new ScenarioLevel(5 , 4,5,2, "More Rural Than Urban", "During Setup, on each board, add 1 Town to land #2 and 1 Town to the highest-numbered land without Setup symbols. During Play, when Invaders would Build 1 City in an Inland land, they instead Build 2 Town." ),
		// Level 3
		new ScenarioLevel(6 , 4,5,3, "Fast Spread", "When making the Invader Deck, Remove 1 additional Stage I Card. (New deck order: 11-2222-33333)" ),
		// Level 4
		new ScenarioLevel(8 , 4,5,3, "Herds Thrive in Verdant Lands",  "Town in lands without Blight are Durable: they have +2 Health, and 'Destroy Town' effects instead deal 2 Damage (to those Town only) per Town they could Destroy. ('Destroy all Town' works normally.)" ),
		// Level 5
		new ScenarioLevel(9 , 4,6,3, "Wave of Immigration",  "Before the initial Explore, put the Habsburg Reminder Card under the top 5 Invader Cards. When Revealed, on each board, add 1 City to a Coastal land without City and 1 Town to the 3 Inland lands with the fewest Blight." ),
		// Level 6
		new ScenarioLevel(10, 5,6,3, "Far-Flung Herds", "Ravages do +2 Damage (total) if any adjacent lands have Town. (This does not cause lands without Invaders to Ravage.)" ),
	};

	public void PreInitialization( GameState gameState ) {

		var hasburgBuilder = new HasburgBuilder();
		gameState.InvaderDeck.Build.Engine = hasburgBuilder;

		// Escalation - Seek Prime Territory
		gameState.InvaderDeck.Explore.Engine.Escalation = SeekPrimeTerritory;

		// Level 1 - Migratory Herders
		if( 1 <= Level)
			hasburgBuilder.HasMigratoryHerders = true;

		// Level 2 - More Rural than Urban
		if( 2 <= Level) {
			MoreRuralThanUrban_Setup( gameState ); // adds towns
			hasburgBuilder.ReplaceInlandCityWith2Towns = true;
		}

		// Level 3 - Invader deck changes

		// Level 4 - Herds Thrive in Verdant Lands (durable towns)
		if( 4 <= Level)
			gameState.AddToAllActiveSpaces( new HabsburgMakeTownsDurable() );

		// Level 5 - Wave of Immigration
		// When Invader Card #5 is Flipped, on each board, add 1 City to a Coastal land without City and 1 Town to the 3 Inland lands with the fewest Blight
		if(5 <= Level)
			gameState.InvaderDeck.UnrevealedCards[4].CardFlipped += WaveOfImmigration;

		// Level 6 - Far-Flung Herds, +2 Ravage damage if adjacent town
		if(6 <= Level) {
			var originalBehavior = gameState.DefaultRavageBehavior.GetDamageFromParticipatingAttackers;
			gameState.DefaultRavageBehavior.GetDamageFromParticipatingAttackers = (behavior, counts, spaceState) => {
				bool hasNeighborTown = spaceState.Adjacent.Any( s => s.Has( Invader.Town ) );
				return originalBehavior( behavior, counts, spaceState )
					+ (hasNeighborTown ? 2 : 0);
			};
		}

		// Additional loss condition - too many 8+blight
		// if 8 <= damage, ++count, if count > # players, loss
		gameState.LandDamaged.ForGame.Add( IrreparableDamage_CheckExcessiveLandDamage );
		gameState.AddWinLossCheck( IrreparableDamage_LossCheck );

	}

	int _badbadBlightCount = 0;
	void IrreparableDamage_CheckExcessiveLandDamage( LandDamagedArgs args ) {
		if(8 <= args.Damage) ++_badbadBlightCount;
	}
	void IrreparableDamage_LossCheck( GameState gameState ) {
		if(gameState.Spirits.Length < _badbadBlightCount)
			GameOverException.Lost( $"Irreparable Damage - {_badbadBlightCount} blight were added from 8+ land damage." );
	}

	static async Task WaveOfImmigration( GameState gameState ) {
		using var actionScope = gameState.StartAction(ActionCategory.Invader); // ??? is this really an action?
		// on each board
		foreach(var board in gameState.Island.Boards) {
//			var boardCtx = new BoardCtx(gameState,board,actionScope);

			var spaces = gameState.Tokens.PowerUp(board.Spaces).ToArray();
			// add 1 City to a Coastal land without City
			var coastWithoutCity =  spaces.FirstOrDefault(s=>s.Space.IsCoastal && s.Sum(Invader.City)==0);
			if( coastWithoutCity != null )
				await coastWithoutCity.Bind( actionScope ).AddDefault(Invader.City,1, AddReason.Build); // What AddReason do we use for Escalation???

			// and 1 Town to the 3 Inland lands with the fewest Blight
			var leastBlightSpaces = spaces
				.Where(x=> !x.Space.IsCoastal && !x.Space.IsOcean)
				.OrderBy( x=>x.Blight.Count )
				.Take(3)
				.ToArray();
			foreach(var newTownSpace in leastBlightSpaces)
				await newTownSpace.Bind( actionScope ).AddDefault(Invader.Town,1, AddReason.Build);
		}
	}

	static void MoreRuralThanUrban_Setup( GameState gameState ) {
		// During Setup, on each board, add 1 Town to land #2 and 1 Town to the highest-numbered land without Setup symbols.
		foreach(Board board in gameState.Island.Boards) {
			// add 1 Town to land #2
			gameState.Tokens[board[2]].AdjustDefault( Invader.Town, 1 );
			// and 1 Town to the highest-numbered land without Setup symbols.
			gameState.Tokens[board.Spaces.Last( x => ((Space1)x).StartUpCounts.IsEmpty )]
				.AdjustDefault( Invader.Town, 1 );
		}

	}

	async Task SeekPrimeTerritory( GameState gameState ) {

		using var actionScope = gameState.StartAction(ActionCategory.Default);

		// On each board
		await Cmd.OnEachBoard( new DecisionOption<BoardCtx>("Add 1 or 2 blight to land without town/blight.", IfTooHealthyAddBlight ))
			.Execute( new GameCtx( gameState, actionScope ) );

	}

	static async Task IfTooHealthyAddBlight(BoardCtx ctx) {
		var spaces = ctx.GameState.Tokens.PowerUp( ctx.Board.Spaces ).ToArray();
		int townsToAdd = spaces.Sum( x => x.Blight.Count ) switch { <= 2 => 2, <= 4 => 1, _ => 0 };

		for(int i = 0; i < townsToAdd; ++i) {
			var addSpaces = spaces.Where( x => x.SumAny( TokenType.Blight, Invader.Town ) == 0 ).ToArray();
			if(addSpaces.Length == 0) break;

			var criteria = new Select.Space( $"Escalation - Add 1 Town to board {ctx.Board.Name} ({i + 1} of {townsToAdd})", addSpaces.Select( x => x.Space ), Present.Always );
			var addSpace = await ctx.Self.Gateway.Decision( criteria );
			await ctx.GameState.Tokens[addSpace].Bind( ctx.ActionScope ).AddDefault( Invader.Town, 1, AddReason.Build );
		}
	}

	public void PostInitialization( GameState gamestate ) {	}
}


/*

Escalation Stage II Escalation.png
Seek Prime Territory: After Exploring: On each board with 4 or fewer Blight, add 1 Town to a land without Town/Blight. On each board with 2 or fewer Blight, do so again.

1	(3)	10 (3/4/3)	Migratory Herders: After the normal Build Step: In each land matching a Build Card, Gather 1 Town from a land not matching a Build Card. (In board/land order.)
2	(5)	11 (4/5/2)	More Rural Than Urban: During Setup, on each board, add 1 Town to land #2 and 1 Town to the highest-numbered land without Setup symbols. During Play, when Invaders would Build 1 City in an Inland land, they instead Build 2 Town.
3	(6)	12 (4/5/3)	Fast Spread: When making the Invader Deck, Remove 1 additional Stage I Card. (New deck order: 11-2222-33333)
4	(8)	12 (4/5/3)	Herds Thrive in Verdant Lands: Town in lands without Blight are Durable: they have +2 Health, and "Destroy Town" effects instead deal 2 Damage (to those Town only) per Town they could Destroy. ("Destroy all Town" works normally.)
5	(9)	13 (4/6/3)	Wave of Immigration: Before the initial Explore, put the Habsburg Reminder Card under the top 5 Invader Cards. When Revealed, on each board, add 1 City to a Coastal land without City and 1 Town to the 3 Inland lands with the fewest Blight.
6	(10)	14 (5/6/3)	Far-Flung Herds: Ravages do +2 Damage (total) if any adjacent lands have Town. (This does not cause lands without Invaders to Ravage.)

Additional Loss Condition
Irreparable Damage: Track how many Blight come off the Blight Card during Ravages that do 8+ Damage to the land. If that number ever exceeds players, the Invaders win.

*/