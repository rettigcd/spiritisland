using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame.Spirits.VitalStrength {
	
	class RitualsOfDestruction {

		[SpiritCard("Rituals of Destruction",3,Speed.Slow,Element.Sun,Element.Moon,Element.Fire,Element.Earth,Element.Plant)]
		[FromSacredSite(1,Target.Dahan)]
		static public Task ActAsync(ActionEngine eng,Space target){

			bool hasBonus = 3 <= eng.GameState.GetDahanOnSpace(target);

			if(hasBonus)
				eng.GameState.AddFear( 2 );

			return eng.DamageInvaders(target, hasBonus ? 5 : 2);

		}

	}

}
