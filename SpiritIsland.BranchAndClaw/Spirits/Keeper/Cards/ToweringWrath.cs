namespace SpiritIsland.BranchAndClaw;

public class ToweringWrath {

	[SpiritCard("Towering Wrath",3,Element.Sun,Element.Fire,Element.Plant),Slow,FromSacredSite(1)]
	[Instructions( "2 Fear. For each of your SacredSite in / adjacent to target land, 2 Damage. Destroy all Dahan." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		// 2 fear
		ctx.AddFear( 2 );

		// for each of your SS in / adjacent to target land, 2 damage
		int sacredSiteCount = ctx.Tokens.InOrAdjacentTo.Intersect( ctx.Self.Presence.SacredSites ).Count() ; // In/adjacent
		await ctx.DamageInvaders( 2 * sacredSiteCount );

		// destroy all dahan
		await ctx.Dahan.Destroy( int.MaxValue );
	}

}