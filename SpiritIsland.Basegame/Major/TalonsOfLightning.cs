namespace SpiritIsland.Basegame;

public class TalonsOfLightning {

	[MajorCard( "Talons of Lightning", 6, Element.Fire, Element.Air )]
	[Fast]
	[ExtendableRange( From.Presence, 1, "3 fire,3 air", 2, Target.Mountain, Target.Wetland )]
	static public async Task ActionAsync( TargetSpaceCtx ctx ) {

		// 3 fear
		ctx.AddFear(3);

		// 5 damage
		await ctx.DamageInvaders(5);

		if( await ctx.YouHave("3 fire,3 air" )){
			// destroy 1 town in each adjacent land
			foreach(var neighbor in ctx.Tokens.Adjacent)
				await ctx.Target(neighbor.Space).Invaders.Destroy( 1,Invader.Town);

		}

	}

}