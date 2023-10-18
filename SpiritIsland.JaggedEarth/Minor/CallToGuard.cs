namespace SpiritIsland.JaggedEarth;

public class CallToGuard{ 
	const string Name = "Call to Guard";

	[MinorCard(Name,0,Element.Sun,Element.Air,Element.Earth), Fast, FromPresence(1)]
	[Instructions( "Gather up to 1 Dahan. Then, if Dahan are present, either: Defend 1 per Dahan. -or- After Invaders are added or moved to target land, 1 Damage to each added or moved Invader." ), Artist( Artists.KatGuevara )]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// Gather up to 1 Dahan.
		await ctx.GatherUpToNDahan( 1 );

		// Then, if Dahan are present, either:
		if(ctx.Dahan.Any)
			await ctx.SelectActionOption( 
				Cmd.Defend1PerDahan, 
				DamageAddedOrMovedInvaders
			);
	}

	static SpaceCmd DamageAddedOrMovedInvaders => new SpaceCmd(
		"After Invaders are added or moved to target land, 1 Damage to each added or moved Invader"
		, (ctx) => ctx.Tokens.Adjust( new DamageNewInvaders(), 1 )
	);

	class DamageNewInvaders : BaseModEntity, IHandleTokenAddedAsync, IEndWhenTimePasses {
		public Task HandleTokenAddedAsync( ITokenAddedArgs args )
			=> args.To.Invaders.ApplyDamageTo1( 1, args.Added.AsHuman() );
	}
}