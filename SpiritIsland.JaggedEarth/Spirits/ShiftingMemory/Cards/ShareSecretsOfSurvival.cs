namespace SpiritIsland.JaggedEarth;

public class ShareSecretsOfSurvival {

	[SpiritCard("Share Secrets of Survival",0,Element.Sun,Element.Air,Element.Earth),Fast,FromSacredSite(1)]
	[Instructions( "Each time Dahan would be Destroyed in target land, Destroy 2 fewer Dahan. -or- Gather up to 2 Dahan.  -If you have- 3 Air: You may do both." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		var gatherUpTo2Dahan = Cmd.GatherUpToNDahan( 2 );
		var destroyFewer = Cmd.EachTimeDestroy2FewerDahan;

		// If you have 3 air
		if(await ctx.YouHave("3 air")) {
			// you may do both
			await gatherUpTo2Dahan.ActAsync(ctx);
			await destroyFewer.ActAsync(ctx);
		} else {
			// otherwise just pick one
			await ctx.SelectActionOption( destroyFewer, gatherUpTo2Dahan );
		}
	}

}