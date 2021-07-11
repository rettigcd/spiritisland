using System;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class FlashFloods {

		public const string Name = "Flash Floods";
		[SpiritCard(FlashFloods.Name,2,Speed.Fast,Element.Sun,Element.Water)]
		static public async Task ActionAsync(ActionEngine engine, Spirit self,GameState gameState){

			bool LandHasInvaders(Space space) => space.IsLand && gameState.InvadersOn(space).InvaderTypesPresent.Any();

			var target = await engine.SelectSpace(
				"Select target space."
				,self.Presence.Range(1).Where(LandHasInvaders)
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