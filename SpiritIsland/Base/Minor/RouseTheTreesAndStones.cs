using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class RouseTheTreesAndStones {

		[MinorCard("Rouse the Trees and Stones",1,Speed.Slow,Element.Fire,Element.Earth,Element.Plant)]
		static public async Task ActAsync(ActionEngine engine){
			var (spirit,gs) = engine;

			var target = await engine.Api.TargetSpace_SacredSite(1,s=>!gs.HasBlight(s));
			// 2 damage
			gs.DamageInvaders(target,2);
			// push 1 explorer
			await engine.PushUpToNInvaders(target,1,Invader.Explorer);
		}

	}
}
