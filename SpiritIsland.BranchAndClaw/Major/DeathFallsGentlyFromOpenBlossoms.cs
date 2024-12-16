namespace SpiritIsland.BranchAndClaw;

public class DeathFallsGentlyFromOpenBlossoms {

	[MajorCard("Death Falls Gently From Open Blossoms",4, Element.Moon,Element.Air,Element.Plant),Slow,FromPresence(3,[Filter.Jungle, Filter.Sands] )]
	[Instructions( "4 Damage. If any Invaders remain, add 1 Disease. -If you have- 3 Air, 3 Plant: 3 Fear. Add 1 Disease to 2 adjacent lands with Invaders." ), Artist( Artists.GrahamStermberg )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 4 damage.
		await ctx.DamageInvaders(4);

		// If any invaders remain, add 1 disease
		if( ctx.Space.InvaderTokens().Any())
			await ctx.Disease.AddAsync(1);

		// if 3 air and 3 plant:  
		if( await ctx.YouHave("3 air,3 plant")) {
			// 3 fear.
			await ctx.AddFear(3);
			// Add 1 disease to 2 adjacent lands with invaders.
			var options = ctx.Space.Adjacent.Where( x => x.HasInvaders() );
			for(int i = 0; i < 2; ++i) {
				Space space = await ctx.Self.SelectAsync( A.SpaceDecision.ToPlaceToken( $"Add disease to ({i + 1} of 2)", options, Present.Always, ctx.Space.Disease.Default ) );
				if( space != null )
					await ctx.Target(space).Disease.AddAsync( 1 );
			}
		}
	}

}