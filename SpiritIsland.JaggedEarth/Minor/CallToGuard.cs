namespace SpiritIsland.JaggedEarth;

public class CallToGuard{ 
	const string Name = "Call to Guard";

	[MinorCard(Name,0,Element.Sun,Element.Air,Element.Earth), Fast, FromPresence(1)]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// Gather up to 1 Dahan.
		await ctx.GatherUpToNDahan( 1 );

		// Then, if Dahan are present, either:
		if(ctx.Dahan.Any)
			await ctx.SelectActionOption( Cmd.Defend1PerDahan, DamageAddedOrMovedInvaders );
	}

	static SpaceAction DamageAddedOrMovedInvaders => new SpaceAction("After Invaders are added or moved to target land, 1 Damage to each added or moved Invader"
		, (ctx) => {
			ctx.Tokens.Adjust( new TokenAddedHandler(Name, args => ctx.Invaders.ApplyDamageTo1(1, (HealthToken)args.Token )), 1 );
		} );

}