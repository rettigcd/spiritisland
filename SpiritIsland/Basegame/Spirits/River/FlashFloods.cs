using System;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class FlashFloods {

		public const string Name = "Flash Floods";
		[SpiritCard(FlashFloods.Name,2,Speed.Fast,Element.Sun,Element.Water)]
		[FromPresence(1,Filter.Invader)]
		static public async Task ActionAsync(ActionEngine engine,Space target){
			var (_,gameState) = engine;	

			// +1 damage, if costal +1 additional damage
			int damage = target.IsCostal ? 2 : 1;

			var group = gameState.InvadersOn(target);
			while(damage>0){
				var invader = await engine.SelectInvader("Select invader to damage.",group.InvaderTypesPresent.ToArray());
				if(invader == null) break;

				damage -= group.ApplyDamageMax(invader,damage);
			}

			gameState.UpdateFromGroup(group);
		}

	}

}