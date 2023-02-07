namespace SpiritIsland.FeatherAndFlame;


[InnatePower( RainAndMudSupressConflict.Name), Fast, Yourself]
internal class RainAndMudSupressConflict {

	const string Name = "Rain and Mud Suppress Conflict";

	// Use unit of work to coordinate between tier levels
	static bool WasUsed( SelfCtx ctx) => ctx.ActionScope.ContainsKey(Name);
	static void MarkAsUsed( SelfCtx ctx ) => ctx.ActionScope[ Name ] = true;

	const string Tier1Elements = "1 air,3 water";

	[InnateOption( Tier1Elements, "Each of your Presence grants Defend 1 and lowers Dahan counterattack damage by 1. (Total, in its land.)", 0 )]
	static public Task Option1( SelfCtx ctx ) {
		MakeThingsMuddy( ctx );
		return Task.CompletedTask;
	}


	[InnateOption( "5 water,1 earth", "Each of your Presence grants Defend 1 and lowers Dahan counterattack damage by 1.", 1 )]
	static public async Task Option2( SelfCtx ctx ) {
		if( await DontWantMoreMud(ctx) ) return;
		MakeThingsMuddy( ctx );
	}

	[InnateOption( "3 air,9 water,2 earth", "2 Fear. In your lands, Invaders and Dahan have -1 Health (min 1)", 0 )]
	static public async Task Option3( SelfCtx ctx ) {
		if( await DontWantMoreMud( ctx ) ) return; 

		// 2 fear
		ctx.AddFear(2);

		// In your lands, Invaders and Dahan have -1 Health( min 1 )
		foreach(var space in ctx.Presence.ActiveSpaceStates) {
			var targetCtx = ctx.Target( space );
			await targetCtx.AdjustTokensHealthForRound( -1,Human.Dahan );
			await targetCtx.AdjustTokensHealthForRound( -1, Human.Invader );
		}
	}

	static public void MakeThingsMuddy( SelfCtx ctx ) {
		// Each of your Presence grants Defend 1
		ctx.GameState.Tokens.Dynamic.ForRound.Register( sp => ctx.Self.Presence.CountOn( sp ), Token.Defend );
		// lowers Dahan counterattack damage by 1
		ctx.GameState.AddToAllActiveSpaces( new MudToken( ctx.Self, 1 ) );
		MarkAsUsed( ctx );
	}

	// Replace with DoRavage
	//	 sets based on presence
	//   doesn't remove self (so works for multiple ravages)

	static async Task<bool> DontWantMoreMud( SelfCtx ctx ) => WasUsed( ctx )
			&& !await ctx.Self.UserSelectsFirstText( "Apply Additional Tier?", "Yes, we want more mud.", "No, thank you." );

}


class MudToken : SelfCleaningToken, ISkipRavages {
	readonly Spirit _self;
	readonly int _count;
	public MudToken( Spirit self, int count ):base() {
		_self = self;
		_count = count;
	}

	/// <summary> Used by skips to determine which skip to use. </summary>
	public UsageCost Cost => UsageCost.Free;

	Task<bool> ISkipRavages.Skip( SpaceState space ) {
		space.AccessGameState().ModifyRavage( space.Space, cfg => cfg.AttackersDefend += (_self.Presence.CountOn( space ) * _count) ); // !!! move this into SpaceState so we can remove accessing GameState
		// Doesn't remove self so it is in place for all ravages
		// removed because it is inserted as a temp token.
		return Task.FromResult( false );
	}
}
