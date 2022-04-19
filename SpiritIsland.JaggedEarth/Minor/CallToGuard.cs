namespace SpiritIsland.JaggedEarth;

public class CallToGuard{ 
		
	[MinorCard("Call to Guard",0,Element.Sun,Element.Air,Element.Earth), Fast, FromPresence(1)]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// Gather up to 1 Dahan.
		await ctx.GatherUpToNDahan( 1 );

		// Then, if Dahan are present, either:
		if(ctx.Dahan.Any)
			await ctx.SelectActionOption( Cmd.Defend1PerDahan, DamageAddedOrMovedInvaders );
	}

	static SpaceAction DamageAddedOrMovedInvaders => new SpaceAction("After Invaders are added or moved to target land, 1 Damage to each added or moved Invader"
		, (ctx) => {
			ctx.GameState.Tokens.TokenAdded.ForRound.Add( async (args)=> {
				if(args.Space == ctx.Space)
					await ctx.Invaders.ApplyDamageTo1(1, (HealthToken)args.Token );
			} );

			ctx.GameState.Tokens.TokenMoved.ForRound.Add( async (args)=> {
				if(args.AddedTo == ctx.Space)
					await ctx.Invaders.ApplyDamageTo1(1, (HealthToken)args.Token );
			} );
		} );

}