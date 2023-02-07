namespace SpiritIsland.JaggedEarth;

// Durable Towns - Have +2 health, Destroy does 2 points of damage.
class HabsburgDurableToken 
	: HumanToken
	, IHandleRemovingToken  // removing Durable, switch to normal
	, IHandleTokenAdded     // check for Blight, switch to normal

{
	public HabsburgDurableToken( HumanToken orig )
		: base( orig.Class, orig._healthPenaltyHolder, orig.FullHealth + 2, orig.Damage, orig.StrifeCount ) { }
	protected HabsburgDurableToken( HumanTokenClass tokenClass, IHaveHealthPenaltyPerStrife penaltyHolder, int rawFullHealth, int damage, int strifeCount, int nightmareDamage )
		: base( tokenClass, penaltyHolder, rawFullHealth, damage, strifeCount, nightmareDamage ) { }
	public override async Task<int> Destroy( SpaceState tokens, int count ) {
		count = Math.Min( count, tokens[this] ); // clip
		if(0 < count) {
			HumanToken damagedToken = this.AddDamage( 2 );
			if(damagedToken.IsDestroyed)
				await base.Destroy( tokens, count );
			else
				tokens.ReplaceNWith( count, this, damagedToken );
		}
		return count;
	}
	protected override HumanToken NewHealthToken( HumanTokenClass tokenClass, IHaveHealthPenaltyPerStrife penaltyHolder, int rawFullHealth, int damage = 0, int strifeCount = 0, int nightmareDamage = 0 )
		=> new HabsburgDurableToken( tokenClass, penaltyHolder, rawFullHealth, damage, strifeCount, nightmareDamage );
	public HumanToken Restore() => new HumanToken( Class, _healthPenaltyHolder, FullHealth - 2, Damage, StrifeCount );

	#region Restoring Tokens to normal when (a) Removing from Space or (b) Adding first Blight
	public async Task HandleTokenAdded( ITokenAddedArgs args ) {
		// If adding first blight
		if(args.Token == Token.Blight && args.AddedTo.Blight.Count == 1) {
			// switch back to normal
			HumanToken restored = Restore();
			if(restored.IsDestroyed)
				await DestroyAll( args.AddedTo );
			else
				args.AddedTo.ReplaceAllWith( this, restored );
		}
	}
	public async Task ModifyRemoving( RemovingTokenArgs args ) {
		// If removing this (Durable) token from space
		if(args.Token == this) {
			// switch it back to normal.
			var restored = Restore();
			if(restored.IsDestroyed) {
				await base.Destroy( args.Space, args.Count ); // must call Base to ensure it gets destroyed
				args.Count = 0;
			} else {
				args.Space.ReplaceNWith( args.Count, this, restored );
				args.Token = restored;
			}
		}
	}
	#endregion
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