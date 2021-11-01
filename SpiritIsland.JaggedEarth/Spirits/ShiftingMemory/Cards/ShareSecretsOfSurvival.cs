using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.ShiftingMemory {
	public class ShareSecretsOfSurvival {

		[SpiritCard("Share Secrets of Survival",0,Element.Sun,Element.Air,Element.Earth),Fast,FromSacredSite(1)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {
			return ctx.SelectActionOption(
				new ActionOption("Each time Dahan would be Destroyed in target land, Destroy 2 fewer Dahan.", () => { 
					ctx.GameState.ModifyRavage(ctx.Space, cfg => { 
						var oldDestroy = cfg.DestroyDahan;
						cfg.DestroyDahan = (dahan,count,health) => oldDestroy(dahan,count-2,health);
					} ); 
				}),
				new ActionOption("Gather up to 2 Dahan", () => ctx.GatherUpToNDahan(2))
			);
		}

	}

}
