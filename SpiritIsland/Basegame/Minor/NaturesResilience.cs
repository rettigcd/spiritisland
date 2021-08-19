using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class NaturesResilience {

		public const string Name = "Nature's Resilience";

		[MinorCard(NaturesResilience.Name,1,Speed.Fast,Element.Earth,Element.Plant,Element.Animal)]
		[FromSacredSite(1)]
		static public async Task Act(TargetSpaceCtx ctx){

			// if 2 water, you may INSTEAD remove 1 blight
			bool removeBlight = ctx.Self.Elements.Contains("2 water")
				&& await ctx.Self.UserSelectsFirstText("Select option", "Remove Blight", "Defend 6" );

			if(removeBlight)
				ctx.RemoveBlight();
			else 
				ctx.Defend(6);

		}

	}
}
