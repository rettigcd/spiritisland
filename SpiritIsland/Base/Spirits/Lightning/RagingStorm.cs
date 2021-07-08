
using SpiritIsland.Core;

namespace SpiritIsland.Base {
	
	[Core.SpiritCard(RagingStorm.Name,3,Speed.Slow,Element.Fire,Element.Air,Element.Water)]
	public class RagingStorm : TargetSpaceAction {
		public const string Name = "Raging Storm";

		// range 1, any
		public RagingStorm(Spirit spirit,GameState gs):base(spirit,gs,1,From.Presence){ }

		protected override void SelectSpace( Space space ) {
			var orig = gameState.InvadersOn(space);

			// 1 damange to each invader.
			var changing = gameState.InvadersOn(space);
			foreach(var invader in orig.InvaderTypesPresent){
				int count = orig[invader];
				changing[invader] -= count;
				changing[invader.Damage(1)] += count; 
			}
			gameState.UpdateFromGroup(changing);

		}


	}

}
