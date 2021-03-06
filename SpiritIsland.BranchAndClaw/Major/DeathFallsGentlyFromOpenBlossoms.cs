namespace SpiritIsland.BranchAndClaw;

public class DeathFallsGentlyFromOpenBlossoms {

	[MajorCard("Death Falls Gently from Open Blossoms",4, Element.Moon,Element.Air,Element.Plant)]
	[Slow]
	[FromPresence(3,Target.JungleOrSand)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// 4 damage.
		await ctx.DamageInvaders(4);

		// If any invaders remain, add 1 disease
		if( ctx.Tokens.InvaderTokens().Any())
			await ctx.Disease.Add(1);

		// if 3 air and 3 plant:  
		if( await ctx.YouHave("3 air,3 plant")) {
			// 3 fear.
			ctx.AddFear(3);
			// Add 1 disease to 2 adjacent lands with invaders.
			for(int i = 0; i < 2; ++i) {
				var adjCtx = await ctx.SelectAdjacentLand( $"Add disease to ({i+1} of 2)", x=>x.Tokens.HasInvaders() );
				await adjCtx.Disease.Add(1);
			}
		}
	}

}