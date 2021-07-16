﻿using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.VitalStrength {
	
	class RitualsOfDestruction {

		[SpiritCard("Rituals of Destruction",3,Speed.Slow,Element.Sun,Element.Moon,Element.Fire,Element.Earth,Element.Plant)]
		[FromSacredSite(1,Filter.Dahan)]
		static public async Task ActAsync(ActionEngine eng,Space target){

			bool hasBonus = 3 <= eng.GameState.GetDahanOnSpace(target);
			
			eng.GameState.DamageInvaders(target, hasBonus ? 5 : 2);

			if(hasBonus)
				eng.GameState.AddFear(2);

		}

	}

}
