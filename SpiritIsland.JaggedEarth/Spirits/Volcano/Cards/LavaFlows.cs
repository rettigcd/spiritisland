using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class LavaFlows {

		[SpiritCard("Lava Flows", 1, Element.Fire, Element.Earth), Slow, FromPresence(1)]
		public static Task ActAsync(TargetSpaceCtx ctx ) { 
			return ctx.SelectActionOption(
				new ActionOption("+1 badland, +1 wilds", () => { ctx.Badlands.Add(1); ctx.Wilds.Add(1); }),
				new ActionOption("1 damage", () => ctx.DamageInvaders(1) )
			);
		}


	}

}
