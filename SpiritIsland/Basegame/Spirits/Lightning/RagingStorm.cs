
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {
	
	public class RagingStorm {
		public const string Name = "Raging Storm";

		[SpiritCard(RagingStorm.Name,3,Speed.Slow,Element.Fire,Element.Air,Element.Water)]
		[FromPresence(1)]
		static public async Task Act(ActionEngine engine,Space target){
			var (_,gameState) = engine;
			var orig = gameState.InvadersOn(target);

			// 1 damange to each invader.
			var changing = gameState.InvadersOn(target);
			foreach(var invader in orig.InvaderTypesPresent){
				int count = orig[invader];
				changing[invader] -= count;
				changing[invader.Damage(1)] += count; 
			}
			gameState.UpdateFromGroup(changing);

		}

	}

}
