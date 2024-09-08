using SpiritIsland.Invaders.Ravage;

namespace SpiritIsland.NatureIncarnate;

[InnatePower(Name),Fast, Yourself]
public class WarnOfImpendingConflict {

	public const string Name = "Warn of Impending Conflict";

	[InnateTier( "2 sun,1 earth", "In 1 of your lands, 1 Dahan deals Damage before Invaders. Choose land during ravage." )]
	static public Task Option1Async( Spirit self ) {
		GameState.Current.Tokens.AddIslandMod( new WarnToken( self, 1 ) );
		return Task.CompletedTask;
	}

	[InnateTier( "3 sun,1 earth", "In that land, another Dahan deals Damage before Invaders." )]
	static public Task Option2Async( Spirit self ) {
		GameState.Current.Tokens.AddIslandMod( new WarnToken( self, 2 ) );
		return Task.CompletedTask;
	}

	[InnateTier( "4 sun,2 earth", "In that land, all Dahan deal Damage before Invaders." )]
	static public Task Option3Async( Spirit self ) {
		GameState.Current.Tokens.AddIslandMod( new WarnToken( self, 100 ) );
		return Task.CompletedTask;
	}

	[InnateTier( "5 sun,3 earth", "Instead, in all your lands, all Dahan deal Damage before Invaders." )]
	static public Task Option4Async( Spirit self ) {
		GameState.Current.Tokens.AddIslandMod( new WarnToken( self, 100, true ) );
		return Task.CompletedTask;
	}

}

/// <summary>
/// Ravage-Config - (some or all) of the Dahan attack before invaders.
/// </summary>
class WarnToken( Spirit spirit, int dahanToGoEarly, bool allSpaces = false ) 
	: BaseModEntity, IConfigRavagesAsync, IEndWhenTimePasses
{

	int _dahanToGoEarly = dahanToGoEarly;
	readonly Spirit _spirit = spirit;
	readonly bool _allSpaces = allSpaces;

	async Task IConfigRavagesAsync.ConfigAsync( Space space ) {
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
	}

	static void MakeDahanGoFast( Space space, ref int remainingGoEarlyCount ) {
		var goFastDahan = space.HumanOfTag( TokenCategory.Dahan )
			// since Invaders kill Dahan with least health first, make them attack before they are destroyed.
			.OrderBy( x => x.RemainingHealth )
			.First();
		int thisTokenGoEarlyCount = Math.Min( remainingGoEarlyCount, space[goFastDahan] );

		space.Humans( thisTokenGoEarlyCount, goFastDahan ).Adjust( Ambush );

		remainingGoEarlyCount -= thisTokenGoEarlyCount;
	}
	static HumanToken Ambush( HumanToken h ) => h.SetRavageOrder( RavageOrder.Ambush );

	static void RestoreDahanSpeed( Space space ) {
		ActionScope.Current.AtEndOfThisAction( scope => {
			var tokensToRestore = space.HumanOfTag(TokenCategory.Dahan)
				.Where( h => h.RavageOrder != RavageOrder.DahanTurn )
				.ToArray();
			foreach(var token in tokensToRestore)
				space.AllHumans(token).Adjust(RavageDuringDahanTurn);
		} );
	}
	static HumanToken RavageDuringDahanTurn(HumanToken token) => token.SetRavageOrder( RavageOrder.DahanTurn );

	private async Task<bool> UserWantsToUseOnThisSpace( Space space, int goEarlyCount ) => await _spirit.UserSelectsFirstText( $"Have {goEarlyCount} Dahan attack first on {space.Label}?", "Yes, attack early", "No, save for another ravage." );
}
