
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {
	
	public class RagingStorm {
		public const string Name = "Raging Storm";

		[SpiritCard(RagingStorm.Name,3,Speed.Slow,Element.Fire,Element.Air,Element.Water)]
		[FromPresence(1)]
		static public Task Act(ActionEngine engine,Space target){
			var (_,gameState) = engine;

			// 1 damange to each invader.
			var grp = gameState.InvadersOn(target);
			grp.ApplyDamageToEach(1, grp.HealthyInvaderTypesPresent.ToArray() );
			return Task.CompletedTask;
		}

	}

}
