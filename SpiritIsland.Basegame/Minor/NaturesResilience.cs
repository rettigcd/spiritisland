using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class NaturesResilience {

		public const string Name = "Nature's Resilience";

		[MinorCard(NaturesResilience.Name,1,Element.Earth,Element.Plant,Element.Animal)]
		[Fast]
		[FromSacredSite(1)]
		static public async Task Act(TargetSpaceCtx ctx){

			await ctx.SelectActionOption(
				new ActionOption("Defend 6", ()=>ctx.Defend(6)),
				new ActionOption("Remove 1 blight", ()=>ctx.RemoveBlight(), await ctx.YouHave("2 water") && ctx.HasBlight )
			);

		}

	}
}
