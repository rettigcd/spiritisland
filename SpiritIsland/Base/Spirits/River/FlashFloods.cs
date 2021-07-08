using System;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[SpiritCard(FlashFloods.Name,2,Speed.Fast,Element.Sun,Element.Water)]
	public class FlashFloods : BaseAction {

		public const string Name = "Flash Floods";

		public FlashFloods(Spirit spirit,GameState gameState):base(gameState){
			_ = ActionAsync(spirit);
		}

		async Task ActionAsync(Spirit spirit){

			bool LandHasInvaders(Space space) => space.IsLand && gameState.InvadersOn(space).InvaderTypesPresent.Any();

			var target = await engine.SelectSpace(
				"Select target space."
				,spirit.Presence.Range(1).Where(LandHasInvaders)
			);

			// +1 damage, if costal +1 additional damage
			int damage = target.IsCostal ? 2 : 1;

			var group = gameState.InvadersOn(target);
			while(damage>0){
				var invader = await engine.SelectInvader("Select invader to damage.",group.InvaderTypesPresent.ToArray());
				if(invader == null) break;

				int damageToInvader = Math.Min(invader.Health,damage);
				var plan = new DamagePlanAction(group.Space,damageToInvader,invader);
				// apply it to working-copy 
				group[plan.Invader]--;
				group[plan.DamagedInvader]++;
				damage -= damageToInvader;
			}

			gameState.UpdateFromGroup(group);
		}

	}

}