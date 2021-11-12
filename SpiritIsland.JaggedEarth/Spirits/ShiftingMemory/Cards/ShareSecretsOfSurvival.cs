using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class ShareSecretsOfSurvival {

		[SpiritCard("Share Secrets of Survival",0,Element.Sun,Element.Air,Element.Earth),Fast,FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			var destroy2FewerDahan = new ActionOption("Each time Dahan would be Destroyed in target land, Destroy 2 fewer Dahan.", () => { 
				ctx.GameState.ModifyRavage(ctx.Space, cfg => { 
					var oldDestroy = cfg.DestroyDahan;
					cfg.DestroyDahan = (dahan,count,health) => oldDestroy(dahan,count-2,health);
				} ); 
			});

			var gatherUpTo2Dahan = new ActionOption( "Gather up to 2 Dahan", () => ctx.GatherUpToNDahan( 2 ) );

			// If you have 3 air
			if(await ctx.YouHave("3 air")) {
				// you may do both
				await gatherUpTo2Dahan.Action();
				await destroy2FewerDahan.Action();
			} else {
				// otherwise just pick one
				await ctx.SelectActionOption( destroy2FewerDahan, gatherUpTo2Dahan );
			}
		}

	}

}
