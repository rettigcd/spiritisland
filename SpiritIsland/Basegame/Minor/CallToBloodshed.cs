using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	class CallToBloodshed {

		[MinorCard("Call to Bloodshed",1,Speed.Slow,Element.Sun,Element.Fire,Element.Animal)]
		[FromPresence(2,Target.Dahan)]
		static public async Task Act(ActionEngine engine,Space target){
			var (_,gameState)=engine;

			const string key1 = "1 damage per dahan";
			const string key2 = "gather up to 3 dahan";
			string opt = await engine.SelectText("Select option",key1,key2);

			if(opt==key1){
				// opt 1 - 1 damage per dahan
				gameState.DamageInvaders(target,gameState.GetDahanOnSpace(target));
				return;
			}

			// opt 2 - gather up to 3 dahan
			await engine.GatherUpToNDahan(target,3);
		}
	}
}
