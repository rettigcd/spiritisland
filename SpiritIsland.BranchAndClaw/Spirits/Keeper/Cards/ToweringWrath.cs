﻿namespace SpiritIsland.BranchAndClaw;

public class ToweringWrath {

	[SpiritCard("Towering Wrath",3,Element.Sun,Element.Fire,Element.Plant)]
	[Slow]
	[FromSacredSite(1)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		// 2 fear
		ctx.AddFear( 2 );

		// for each of your SS in / adjacent to target land, 2 damage
		int sacredSiteCount = ctx.Range(1).Intersect( ctx.Self.Presence.SacredSites ).Count() ;
		await ctx.DamageInvaders( 2 * sacredSiteCount );

		// destroy all dahan
		await ctx.DestroyDahan( int.MaxValue );
	}

}