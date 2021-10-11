using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class NaturesResilience {

		public const string Name = "Nature's Resilience";

		[MinorCard(NaturesResilience.Name,1,Element.Earth,Element.Plant,Element.Animal)]
		[Fast]
		[FromSacredSite(1)]
		static public Task Act(TargetSpaceCtx ctx){

			return ctx.SelectActionOption(
				new ActionOption("Defend 6", ()=>ctx.Defend(6)),
				new ActionOption("Remove 1 blight", ()=>ctx.RemoveBlight(), ctx.YouHave("2 water") && ctx.HasBlight )
			);

		}

	}
}
