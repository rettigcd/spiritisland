namespace SpiritIsland.PromoPack1;


[InnatePower("Rain and Mud Suppress Conflict"), Fast, Yourself]
internal class RainAndMudSupressConflict {

	[InnateOption( "1 air,3 water", "Each of your Presence grants Defend 1 and lowers Dahan counterattack damage by 1. (Total, in its land.)", 0 )]
	static public Task Option1( SelfCtx ctx ) {

		// Each of your Presence grants Defend 1
		ctx.GameState.Tokens.RegisterDynamic( ( gs, sp ) => ctx.Self.Presence.CountOn( sp ), TokenType.Defend, false );
		// lowers Dahan counterattack damage by 1
		LowerDahanCounterAttackPerPresence( ctx );
		return Task.CompletedTask;
	}

	[InnateOption( "5 water,1 earth", "Each of your Presence grants Defend 1 and lowers Dahan counterattack damage by 1.", 1 )]
	static public Task Option2( SelfCtx ctx ) {
		// Each of your Presence grants Defend 1
		ctx.GameState.Tokens.RegisterDynamic( ( gs, sp ) => ctx.Self.Presence.CountOn( sp ), TokenType.Defend, false );
		// lowers Dahan counterattack damage by 1
		LowerDahanCounterAttackPerPresence( ctx );
		return Task.CompletedTask;
	}

	[InnateOption( "3 air,9 water,2 earth", "2 Fear. In your lands, Invaders and Dahan have -1 Health (min 1)", 2 )]
	static public async Task Option3( SelfCtx ctx ) {

		// 2 fear
		ctx.AddFear(2);

		// In your lands, Invaders and Dahan have -1 Health( min 1 )
		foreach(var space in ctx.Self.Presence.Spaces)
			await ctx.Target(space).AdjustTokensHealthForRound(-1,TokenType.Dahan,Invader.Explorer,Invader.Town,Invader.City);
	
	}

	static void LowerDahanCounterAttackPerPresence( SelfCtx ctx ) {
		// !! This is suppose to be dynamic as presence is added / removed.
		ctx.GameState.PreRavaging.ForRound.Add( args => {
			foreach(var space in args.Spaces)
				args.GameState.ModifyRavage( space, cfg => cfg.AttackersDefend += ctx.Self.Presence.CountOn( space ) );
		} );
	}

}

