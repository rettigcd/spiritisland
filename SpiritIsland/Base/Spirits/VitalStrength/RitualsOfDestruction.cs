using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.VitalStrength {
	
	class RitualsOfDestruction {

		[SpiritCard("Rituals of Destruction",3,Speed.Slow,Element.Sun,Element.Moon,Element.Fire,Element.Earth,Element.Plant)]
		static public async Task ActAsync(ActionEngine eng){

			var target = await eng.Api.TargetSpace_SacredSite(1,eng.GameState.HasDahan);

			bool hasBonus = 3 <= eng.GameState.GetDahanOnSpace(target);
			
			eng.GameState.DamageInvaders(target, hasBonus ? 5 : 2);

			if(hasBonus)
				eng.GameState.AddFear(2);

		}

	}

}
