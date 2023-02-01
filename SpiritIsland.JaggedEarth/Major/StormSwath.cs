namespace SpiritIsland.JaggedEarth;

public class StormSwath {

	[MajorCard("Storm-Swath",3,Element.Fire,Element.Air,Element.Water), Slow]
	[ExtendableRange(From.SacredSite,1,"2 fire,3 air,2 water",1 )]
	public static async Task ActAsync(TargetSpaceCtx ctx ) {
		// 2 fear.
		ctx.AddFear(2);

		// in both origin land and target land: 1 damage to each invader.
		var origin = await FindOriginLand_SS( ctx, 1 );
		await ctx.DamageEachInvader(1);
		await ctx.Target(origin).DamageEachInvader(1);

		// if you have 2 fire 3 air 2 water:
		if(await ctx.YouHave("2 fire,3 air,2 water" )) {
			// +1 fear.
			ctx.AddFear( 1 );

			// This Power has +1 range. (see attribute)

			// in a land adjacent to both origin and target
			Space thirdSpace = await SelectLandAdjacentToBoth( ctx, origin );

			// 1 damage to each invader.
			await ctx.Target( thirdSpace ).DamageEachInvader( 1 );

			// in lands where you did Damage, Destroy 1 town
			foreach(var space in new[] { ctx.Space, origin, thirdSpace }.Distinct())
				await ctx.Target( space ).Invaders.DestroyNOfClass( 1, Human.Town );
		}
	}

	static async Task<Space> SelectLandAdjacentToBoth( TargetSpaceCtx ctx, Space origin ) {

		return await ctx.Decision( new Select.Space(
			"Select land for 1 damge to each invader",
			ctx.Adjacent
				.Intersect( ctx.Target( origin ).Adjacent )
				.Where( s => ctx.Target( s ).IsInPlay ),
			Present.AutoSelectSingle
		) );
	}

	static Task<Space> FindOriginLand_SS( TargetSpaceCtx ctx, int range ) {
		return ctx.Decision( new Select.Space(
			"Select Origin land",
			ctx.Range(range).Where( s => ctx.Presence.IsSelfSacredSite ),
			Present.AutoSelectSingle
		));
	}

}