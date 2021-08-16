using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class RouseTheTreesAndStones {

		[MinorCard("Rouse the Trees and Stones",1,Speed.Slow,Element.Fire,Element.Earth,Element.Plant)]
		[FromSacredSite(1,Target.NoBlight)]
		static public async Task ActAsync(ActionEngine engine,Space target){
			// 2 damage
			engine.GameState.DamageInvaders(target,2);
			// push 1 explorer
			await engine.PushUpToNInvaders(target,1,Invader.Explorer);
		}

	}
}
