namespace SpiritIsland.JaggedEarth;

public class HabsburgMonarchy : AdversaryBase, IAdversary {

	public const string Name = "Habsburg Monarchy";

	public override AdversaryLevel[] Levels => _levels;

	public override AdversaryLossCondition LossCondition => new IrreperableDamage();

	readonly AdversaryLevel[] _levels = new AdversaryLevel[] {
		// Level 0 - Escalation
		new AdversaryLevel(0, 2 , 3,3,3, "Seek Prime Territory", "On each board with 4 or fewer Blight, add 1 Town to a land without Town/Blight. On each board with 2 or fewer Blight, do so again." )
			.WithEscalation( SeekPrimeTerritory_Escalation ),

		// Level 1
		new AdversaryLevel(1, 3, 3,4,3, "Migratory Herders", 
			"After the normal Build Step: In each land matching a Build Card, Gather 1 Town from a land not matching a Build Card. (In board/land order.)" ) {
			InitFunc = (gameState,_) => gameState.InvaderDeck.Build.Engine = new HabsurgBuilder(),
		},

		// Level 2
		new AdversaryLevel(2, 5, 4,5,2, "More Rural Than Urban", 
			"During Setup, on each board, add 1 Town to land #2 and 1 Town to the highest-numbered land without Setup symbols. "+
			"During Play, when Invaders would Build 1 City in an Inland land, they instead Build 2 Town." 
		) {
			InitFunc = (gameState,_) => {
				// on each board,
				var spaces = gameState.Island.Boards
					.SelectMany( board => new Space[] {
					// on land #2 
					board[2],
					// and the highest-numbered land without Setup symbols,
					board.Spaces.Last(x =>((Space1) x).StartUpCounts.IsEmpty)
					} )
					.Tokens()
					.ToArray();

				// add 1 Town
				foreach(SpaceState space in spaces)
					space.Setup( Human.Town, 1 );

				((HabsurgBuilder)gameState.InvaderDeck.Build.Engine).ReplaceInlandCityWith2Towns = true; 
			},
		},

		// Level 3
		new AdversaryLevel(3, 6, 4,5,3, "Fast Spread", 
			"When making the Invader Deck, Remove 1 additional Stage I Card." )
			.WithInvaderCardOrder("11-2222-33333"),

		// Level 4
		new AdversaryLevel(4, 8, 4,5,3, "Herds Thrive in Verdant Lands",  
			"Town in lands without Blight are Durable: they have +2 Health, and 'Destroy Town' effects instead deal 2 Damage (to those Town only) per Town they could Destroy. ('Destroy all Town' works normally.)" ){ 
			InitFunc = (gameState,_) => gameState.AddIslandMod( new HabsburgMakeTownsDurable() )
		},

		// Level 5
		new AdversaryLevel(5, 9, 4,6,3, "Wave of Immigration", 
			"Before the initial Explore, put the Habsburg Reminder Card under the top 5 Invader Cards. When Revealed, on each board, add 1 City to a Coastal land without City and 1 Town to the 3 Inland lands with the fewest Blight." ){ 
			InitFunc = (gameState,_) => gameState.InvaderDeck.UnrevealedCards[4].CardRevealed += HabsburgReminderCard_Revealed
		},

		// Level 6
		new AdversaryLevel(6, 10, 5,6,3, "Far-Flung Herds", 
			"Ravages do +2 Damage (total) if any adjacent lands have Town. (This does not cause lands without Invaders to Ravage.)" ){ 
			InitFunc = (gameState,_) => {
				// !! Can simplify RavageBehavior if we slap a Town-Tracking token on every space Which updates a 'NeighboringTownsToken' that does damage.
				// !! requires that we tag invaders as attackers and dahan as defenders.

				var originalBehavior = RavageBehavior.DefaultBehavior.GetDamageFromParticipatingAttackers;

				RavageBehavior.DefaultBehavior.GetDamageFromParticipatingAttackers = (rex) => {

					bool hasNeighborTown = rex.Tokens.Adjacent.Any( s => s.Has( Human.Town ) );
					// Not logging additional damage here because Ravage is already very verbose.
					return originalBehavior( rex )
						+ (hasNeighborTown ? 2 : 0);
				};
			}
		},
	};

	#region Escalation

	static async Task SeekPrimeTerritory_Escalation( GameState gameState ) {

		await using var actionScope = await ActionScope.Start( ActionCategory.Adversary );

		// On each board
		await Cmd.ForEachBoard( new BaseCmd<BoardCtx>( "Add 1 or 2 blight to land without town/blight.", IfTooHealthyAddBlight ) )
			.ActAsync( gameState );

	}

	static async Task IfTooHealthyAddBlight( BoardCtx ctx ) {
		var spaces = ctx.Board.Spaces.Tokens().ToArray();
		int townsToAdd = spaces.Sum( x => x.Blight.Count ) switch { <= 2 => 2, <= 4 => 1, _ => 0 };

		for(int i = 0; i < townsToAdd; ++i) {
			var addSpaces = spaces.Where( x => x.SumAny( Token.Blight, Human.Town ) == 0 ).ToArray();
			if(addSpaces.Length == 0) break;

			var criteria = new A.Space( $"Escalation - Add 1 Town to board {ctx.Board.Name} ({i + 1} of {townsToAdd})", addSpaces.Downgrade(), Present.Always );
			var addSpace = await ctx.Self.SelectAsync( criteria );
			await addSpace.Tokens.AddDefaultAsync( Human.Town, 1, AddReason.Build );
		}
	}

	#endregion Escalation


	#region Level-5

	static async Task HabsburgReminderCard_Revealed( GameState gameState ) {
		// Level 5

		var newTownSpaces = new List<SpaceState>();
		var newCitySpaces = new List<SpaceState>();

		// on each board
		foreach(Board board in gameState.Island.Boards) {

			var spaces = board.Spaces.Tokens().ToArray();
			// add 1 City to a Coastal land without City
			var coastWithoutCity =  spaces.FirstOrDefault(s=>s.Space.IsCoastal && s.Sum(Human.City)==0);
			if( coastWithoutCity != null)
				newCitySpaces.Add( coastWithoutCity );

			// and 1 Town to the 3 Inland lands with the fewest Blight
			newTownSpaces.AddRange( spaces
				.Where(x=> !x.Space.IsCoastal && !x.Space.IsOcean)
				.OrderBy( x=>x.Blight.Count )
				.Take(3)
			);
		}

		// Take action
		await using var actionScope = await ActionScope.Start(ActionCategory.Invader); // ??? is this really an action?
		foreach(var newTownSpace in newTownSpaces)
			await newTownSpace.AddDefaultAsync( Human.Town, 1, AddReason.Build );

		foreach(var citySpace in newCitySpaces)
			await citySpace.AddDefaultAsync( Human.City, 1, AddReason.Build ); // What AddReason do we use for Escalation???

		// Log it
		var logParts = new List<string>();
		if(newCitySpaces.Count != 0)
			logParts.Add("1 city to "+ newCitySpaces.SelectLabels().Join(","));
		if(newTownSpaces.Count != 0)
			logParts.Add( "1 town to " + newTownSpaces.SelectLabels().Join( "," ) );
		ActionScope.Current.LogDebug("Wave of Immigration: Adding " + logParts.Join(" and "));
	}

	#endregion Level-5

}

class IrreperableDamage : AdversaryLossCondition {
	public IrreperableDamage():base(
		"Irreparable Damage: Track how many Blight come off the Blight Card during Ravages that do 8+ Damage to the land. If that number ever exceeds players, the Invaders win.",
		LossCheckImp
	) {
	}

	public override void Init( GameState gs ) {
		gs.AddWinLossCheck( LossCheckImp );
		gs.AddIslandMod( new TrackBadRavageBlight() );
	}

	static public void LossCheckImp( GameState gameState ) {
		int badBlightCount = _fakeBadBlightSpace.Tokens[Token.Blight];
		if(gameState.Spirits.Length < badBlightCount)
			GameOverException.Lost( $"Irreparable Damage - {badBlightCount} blight were added from 8+ land damage." );
	}

	class TrackBadRavageBlight : BaseModEntity, IReactToLandDamage {
		Task IReactToLandDamage.HandleDamageAddedAsync( SpaceState tokens, int count ) {
			bool shouldAddBadBadBlight = 8 <= tokens[LandDamage.Token];
			if(shouldAddBadBadBlight)
				_fakeBadBlightSpace.Tokens.Adjust( Token.Blight, 1 );
			return Task.CompletedTask;
		}
	}

	// not a real space, just used for counting blight.   And auto-saves in GameState for rewind.
	static readonly FakeSpace _fakeBadBlightSpace = new FakeSpace( ">8 Damage Ravge-Blight" );
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