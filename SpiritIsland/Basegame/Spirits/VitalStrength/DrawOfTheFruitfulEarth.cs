using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame.Spirits.VitalStrength {

	class DrawOfTheFruitfulEarth {

		[SpiritCard("Draw of the Fruitful Earth",1,Speed.Slow,Element.Earth,Element.Plant,Element.Animal)]
		[FromPresence(1)]
		static public async Task Act(ActionEngine eng,Space target){
			await eng.GatherUpToNDahan(target,2);
			await eng.GatherUpToNInvaders(target,2,Invader.Explorer);
		}
	}
}
