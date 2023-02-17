namespace SpiritIsland.JaggedEarth;

class HasburgBuilder : BuildEngine {

	public bool ReplaceInlandCityWith2Towns { get; set; }
	public bool HasMigratoryHerders { get; set; }

	public override async Task ActivateCard( InvaderCard card, GameState gameState ) {
		await base.ActivateCard( card, gameState );
		
		// Migratory Herders ( After the normal Build Step...)
		if(HasMigratoryHerders)
			await MigratoryHerders( card, gameState );

	}

	static async Task MigratoryHerders( InvaderCard card, GameState gameState ) {
		SpaceState[] cardDependentBuildSpaces = GetSpacesMatchingCard( card, gameState );
		ActionScope actionScope = new ActionScope( ActionCategory.Invader );// ??? !! should we reuse the original action?

		// In each land matching a Build Card
		foreach(SpaceState spaceState in cardDependentBuildSpaces) {
			Spirit spirit = spaceState.Space.Board.FindSpirit();
			await spaceState.Gather( spirit )
				// Gather 1 Town 
				.AddGroup( 1, Human.Town )
				// from a land not matching a Build Card.
				.FilterSource( ss => !cardDependentBuildSpaces.Contains( ss ) )
				.GatherN();
		}
	}

	public override Task Do1Build( GameState gameState, SpaceState spaceState ) 
		=> ReplaceInlandCityWith2Towns 
			? new HasburgSpaceBuilder( gameState, spaceState ).Exec()
			: new BuildOnceOnSpace( gameState, spaceState ).Exec();

	class HasburgSpaceBuilder : BuildOnceOnSpace {
		public HasburgSpaceBuilder( GameState gs, SpaceState spaceState ) : base( gs, spaceState ) { }
		protected override (int, HumanTokenClass) DetermineWhatToAdd() {  
			var (count,tokenClass) = base.DetermineWhatToAdd();
			return (tokenClass == Human.City && !_tokens.Space.IsCoastal && !_tokens.Space.IsOcean)
				? (2,Human.Town)
				: (count,tokenClass);
		}
	}
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