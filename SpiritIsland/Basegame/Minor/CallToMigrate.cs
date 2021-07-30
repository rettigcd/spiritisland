using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {
	class CallToMigrate {

		[MinorCard("Call to Migrate",1,Speed.Slow,Element.Fire,Element.Air,Element.Animal)]
		[FromPresence(1)]
		static public async Task ActAsync(ActionEngine engine,Space target){
			await engine.GatherUpToNDahan(target,3);
			await engine.PushUpToNDahan(target,3);
		}

	}
}
