namespace SpiritIsland.PromoPack1;


[InnatePower("Rain and Mud Suppress Conflict"), Fast, Yourself]
internal class RainAndMudSupressConflict {

	const string Tier1Elements = "1 air,3 water";
	[InnateOption( Tier1Elements, "Each of your Presence grants Defend 1 and lowers Dahan counterattack damage by 1. (Total, in its land.)", 0 )]
	static public Task MakeThingsMuddy( SelfCtx ctx ) {
		// Each of your Presence grants Defend 1
		ctx.GameState.Tokens.Dynamic.ForRound.Register( sp => ctx.Self.Presence.CountOn( sp.Space ), TokenType.Defend );
		// lowers Dahan counterattack damage by 1
		LowerDahanCounterAttackPerPresence( ctx );
		return Task.CompletedTask;
	}

	// Grouping 2 and 3 together so that if we cancel, we don't get the 3rd tier

	static async Task<bool> StopAfterLevel1( SelfCtx ctx ) {
		return await ctx.YouHave( Tier1Elements ) 
			&& !await ctx.Self.UserSelectsFirstText("Apply Tier 2 and beyond?","Yes, we want more mud.","No, thank you.");
	}

	[InnateOption( "5 water,1 earth", "Each of your Presence grants Defend 1 and lowers Dahan counterattack damage by 1.", 1 )]
	static public async Task Option2( SelfCtx ctx ) {
		if(await StopAfterLevel1(ctx)) return;
		await MakeThingsMuddy( ctx );
	}

	[InnateOption( "3 air,9 water,2 earth", "2 Fear. In your lands, Invaders and Dahan have -1 Health (min 1)", 1 )]
	static public async Task Option3( SelfCtx ctx ) {

		if(await StopAfterLevel1( ctx )) return;

		await MakeThingsMuddy( ctx ); // Do Tier 2

		// Tier 3 part

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

