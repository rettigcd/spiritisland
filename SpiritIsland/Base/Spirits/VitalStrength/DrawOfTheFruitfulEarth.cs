using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.VitalStrength {

	class DrawOfTheFruitfulEarth {

		[SpiritCard("Draw of the Fruitful Earth",1,Speed.Slow,Element.Earth,Element.Plant,Element.Animal)]
		static public async Task Act(ActionEngine eng){
			var target = await eng.Api.TargetSpace_Presence(1);

			await eng.GatherUpToNDahan(target,2);
			await eng.GatherUpToNInvaders(target,2,Invader.Explorer);
		}
	}
}
