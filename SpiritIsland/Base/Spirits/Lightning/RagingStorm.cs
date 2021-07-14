
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {
	
	[Core.SpiritCard(RagingStorm.Name,3,Speed.Slow,Element.Fire,Element.Air,Element.Water)]
	public class RagingStorm : BaseAction {
		public const string Name = "Raging Storm";

		public RagingStorm(Spirit spirit,GameState gs):base(spirit,gs){_ = Act();}

		async Task Act(){
			// range 1, any
			var target = await engine.Api.TargetSpace_Presence(1);

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
