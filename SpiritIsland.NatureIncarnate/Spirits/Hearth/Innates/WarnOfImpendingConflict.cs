namespace SpiritIsland.NatureIncarnate;

[InnatePower(Name),Fast, Yourself]
public class WarnOfImpendingConflict {

	public const string Name = "Warn of Impending Conflict";

	[InnateTier( "2 sun,1 earth", "In 1 of your lands, 1 Dahan deals Damage before Invaders. Choose land during ravage." )]
	static public Task Option1Async( SelfCtx ctx ) {
		GameState.Current.Tokens.AddIslandMod( new WarnToken( ctx.Self, 1 ) );
		return Task.CompletedTask;
	}

	[InnateTier( "3 sun,1 earth", "In that land, another Dahan deals Damage before Invaders." )]
	static public Task Option2Async( SelfCtx ctx ) {
		GameState.Current.Tokens.AddIslandMod( new WarnToken( ctx.Self, 2 ) );
		return Task.CompletedTask;
	}

	[InnateTier( "4 sun,2 earth", "In that land, all Dahan deal Damage before Invaders." )]
	static public Task Option3Async( SelfCtx ctx ) {
		GameState.Current.Tokens.AddIslandMod( new WarnToken( ctx.Self, 100 ) );
		return Task.CompletedTask;
	}

	[InnateTier( "5 sun,3 earth", "Instead, in all your lands, all Dahan deal Damage before Invaders." )]
	static public Task Option4Async( SelfCtx ctx ) {
		GameState.Current.Tokens.AddIslandMod( new WarnToken( ctx.Self, 100, true ) );
		return Task.CompletedTask;
	}

}

class WarnToken : BaseModEntity, ISkipRavages, IEndWhenTimePasses {

	int _dahanToGoEarly;
	readonly Spirit _spirit;
	readonly bool _allSpaces;

	public WarnToken( Spirit spirit, int dahanToGoEarly, bool allSpaces = false ) {
		_spirit = spirit;
		_dahanToGoEarly = dahanToGoEarly;
		_allSpaces = allSpaces;
	}
	public UsageCost Cost => UsageCost.Something;

	public async Task<bool> Skip( SpaceState space ) {
		int remainingGoEarlyCount = Math.Min( _dahanToGoEarly, space.Dahan.CountAll );
		if(remainingGoEarlyCount != 0
			&& _spirit.Presence.IsOn( space )
			&& (_allSpaces || await UserWantsToUseOnThisSpace( space, remainingGoEarlyCount ))
		) {
			// Speed Up Dahan
			while(0 < remainingGoEarlyCount)
				MakeDahanGoFast( space, ref remainingGoEarlyCount );

			// Restore
			RestoreDahanSpeed( space );

			if(!_allSpaces)
				_dahanToGoEarly = 0; // mark it used
		}


		return false;
	}

	static void MakeDahanGoFast( SpaceState space, ref int remainingGoEarlyCount ) {
		var goFastDahan = space.OfCategory( TokenCategory.Dahan ).Cast<HumanToken>()
			// since Invaders kill Dahan with least health first, make them attack before they are destroyed.
			.OrderBy( x => x.RemainingHealth )
			.First();
		int thisTokenGoEarlyCount = Math.Min( remainingGoEarlyCount, space[goFastDahan] );
		space.Adjust( goFastDahan.SetRavageOrder( RavageOrder.Ambush ), thisTokenGoEarlyCount );
		space.Adjust( goFastDahan, -thisTokenGoEarlyCount );
		remainingGoEarlyCount -= thisTokenGoEarlyCount;
	}

	static void RestoreDahanSpeed( SpaceState space ) {
		ActionScope.Current.AtEndOfThisAction( scope => {
			var tokensToRestore = space.OfCategory( TokenCategory.Dahan ).Cast<HumanToken>()
				.Where( h => h.RavageOrder != RavageOrder.DahanTurn )
				.ToArray();
			foreach(var token in tokensToRestore) {
				space.Adjust( token.SetRavageOrder( RavageOrder.DahanTurn ), space[token] );
				space.Init( token, 0 );
			}
		} );
	}

	private async Task<bool> UserWantsToUseOnThisSpace( SpaceState space, int goEarlyCount ) => await _spirit.UserSelectsFirstText( $"Have {goEarlyCount} Dahan attack first on {space.Space.Text}?", "Yes, attack early", "No, save for another ravage." );
}
