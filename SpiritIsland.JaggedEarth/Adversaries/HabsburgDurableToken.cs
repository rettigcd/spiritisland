namespace SpiritIsland.JaggedEarth;

// Durable Towns - Have +2 health, Destroy does 2 points of damage.
class HabsburgDurableToken 
	: HumanToken
	, IModifyRemovingTokenAsync  // removing Durable, switch to normal
	, IHandleTokenAddedAsync     // check for Blight, switch to normal

{

	protected HabsburgDurableToken( Props x ):base(x) { }

	static Props Add2Health(Props props ) { props._rawFullHealth+=2; return props; }

	public HabsburgDurableToken( HumanToken orig )
		: base( Add2Health(orig.GetProps()) ) { }

	protected override HumanToken MakeNew( Props x ) => new HabsburgDurableToken( x );

	public HumanToken GetRestoreToken() => new HumanToken( HumanClass, FullHealth - 2).AddDamage(Damage).AddStrife(StrifeCount);

	#region Restoring Tokens to normal when (a) Removing from Space or (b) Adding first Blight
	public async Task HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args ) {
		// If adding first blight
		if(args.Added == Token.Blight && to.Blight.Count == 1)
			// switch back to normal
			await to.AllHumans( this ).AdjustAsync( _ => GetRestoreToken() );
	}
	async Task IModifyRemovingTokenAsync.ModifyRemovingAsync( RemovingTokenArgs args ) {
		// If removing this (Durable) token from space
		if(args.Token != this) return;

		// Destroy command
		var town = args.Token.AsHuman();
		if( args.Reason == RemoveReason.Destroyed) {

			// If direct destroy, replace with 2 damage
			if( args is not DestroyingFromDamage && 2 < town.RemainingHealth ) {
				// Just do 2 damage
				args.From.Humans( args.Count, town ).Adjust( x => x.AddDamage( 2 ) );
				// Don't destroy it
				args.Count = 0;
			}

			return;
		}

		// This not a Destroy, it is something else
		// Restore non-durable health
		HumanToken restored = GetRestoreToken();
		if(restored.IsDestroyed) {
			// Just destroy it
			await args.From.Destroy( args.Token, args.Count );
			// Don't, try to remove it.
			args.Count = 0;
		} else {
			// Instead of removing the durable, now removing the regular.
			args.From.Humans( args.Count, this ).Adjust( _=>restored );
			args.Token = restored;
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