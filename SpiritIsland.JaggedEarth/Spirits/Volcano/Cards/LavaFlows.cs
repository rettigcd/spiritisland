using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class LavaFlows {

		[SpiritCard("Lava Flows", 1, Element.Fire, Element.Earth), Slow, FromPresence(1)]
		public static Task ActAsync(TargetSpaceCtx ctx ) { 
			// Add 1 badland and 1 disease.
			ctx.Badlands.Count++;
			ctx.Disease.Count++;
			return Task.CompletedTask;
		}


	}

}
