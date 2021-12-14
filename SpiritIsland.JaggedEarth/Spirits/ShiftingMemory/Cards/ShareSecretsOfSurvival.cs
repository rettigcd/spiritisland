using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class ShareSecretsOfSurvival {

		[SpiritCard("Share Secrets of Survival",0,Element.Sun,Element.Air,Element.Earth),Fast,FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			var destroy2FewerDahan = Cmd.Destroy2FewerDahan;
			var gatherUpTo2Dahan = Cmd.GatherUpToNDahan( 2 );

			// If you have 3 air
			if(await ctx.YouHave("3 air")) {
				// you may do both
				await gatherUpTo2Dahan.Execute(ctx);
				await destroy2FewerDahan.Execute(ctx);
			} else {
				// otherwise just pick one
				await ctx.SelectActionOption( destroy2FewerDahan, gatherUpTo2Dahan );
			}
		}

	}

}
