namespace SpiritIsland.PromoPack1;


[InnatePower("Rain and Mud Suppress Conflict"), Fast, Yourself]
internal class RainAndMudSupressConflict {

	const string Tier1Elements = "1 air,3 water";

	[InnateOption( Tier1Elements, "Each of your Presence grants Defend 1 and lowers Dahan counterattack damage by 1. (Total, in its land.)", 0 )]
	static public Task Option1( SelfCtx ctx ) {
		MakeThingsMuddy( ctx, 1 );
		return Task.CompletedTask;
	}


	[InnateOption( "5 water,1 earth", "Each of your Presence grants Defend 1 and lowers Dahan counterattack damage by 1.", 0 )]
	static public async Task Option2( SelfCtx ctx ) {
		if(await StopAfterLevel1(ctx)){
			MakeThingsMuddy( ctx, 1 ); 
			return;
		}
		MakeThingsMuddy( ctx, await ctx.YouHave( Tier1Elements ) ? 2 : 1 );
	}

	[InnateOption( "3 air,9 water,2 earth", "2 Fear. In your lands, Invaders and Dahan have -1 Health (min 1)", 0 )]
	static public async Task Option3( SelfCtx ctx ) {

		if(await StopAfterLevel1( ctx )){
			MakeThingsMuddy( ctx, 1 );
			return;
		}

		MakeThingsMuddy( ctx, await ctx.YouHave( Tier1Elements ) ? 2 : 1 );

		// Tier 3 part

		// 2 fear
		ctx.AddFear(2);

		//!!! should we be able to opt-out of this?

		// In your lands, Invaders and Dahan have -1 Health( min 1 )
		foreach(var space in ctx.Presence.Spaces)
			await ctx.Target(space).AdjustTokensHealthForRound(-1,TokenType.Dahan,Invader.Explorer,Invader.Town,Invader.City);
	
	}

	static public void MakeThingsMuddy( SelfCtx ctx, int count ) {
		// Each of your Presence grants Defend 1
		ctx.GameState.Tokens.Dynamic.ForRound.Register( sp => ctx.Self.Presence.CountOn( sp ), TokenType.Defend );
		// lowers Dahan counterattack damage by 1
		LowerDahanCounterAttackPerPresence( ctx, 1 );
	}

	static void LowerDahanCounterAttackPerPresence( SelfCtx ctx, int count ) {
		ctx.GameState.AdjustTempTokenForAll( new MudToken(ctx.Self, count) );
	}

	// Replace with DoRavage
	//	 sets based on presence
	//   doesn't remove self (so works for multiple ravages)

	static async Task<bool> StopAfterLevel1( SelfCtx ctx ) => await ctx.YouHave( Tier1Elements )
			&& !await ctx.Self.UserSelectsFirstText( "Apply Tier 2 and beyond?", "Yes, we want more mud.", "No, thank you." );

}


class MudToken : SkipBase, ISkipRavages {
	Spirit _self;
	int _count;
	public MudToken( Spirit self, int count ):base("RainAndMud"){
		_self = self;
		_count = count;
	}

	Task<bool> ISkipRavages.Skip( GameState gameState, SpaceState space ) {
		gameState.ModifyRavage( space.Space, cfg => cfg.AttackersDefend += (_self.Presence.CountOn( space ) * _count) );
		// Doesn't remove self so it is in place for all ravages
		// removed because it is inserted as a temp token.
		return Task.FromResult( false );
	}
}
