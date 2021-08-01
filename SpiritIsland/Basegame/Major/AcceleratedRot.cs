using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	public class AcceleratedRot {

		public const string Name = "Accelerated Rot";

		[MajorCard(AcceleratedRot.Name,4,Speed.Slow,Element.Sun,Element.Water,Element.Plant)]
		[FromPresence(2,Filter.JungleOrWetland)]
		static public Task ActAsync(ActionEngine engine, Space target){
			var (spirit,gameState) = engine;

			// 2 fear, 4 damage
			int damageToInvaders = 4;
			gameState.AddFear(2);

			if(spirit.Elements.Has(
				Element.Sun,Element.Sun,Element.Sun,
				Element.Water,Element.Water,
				Element.Plant,Element.Plant,Element.Plant
			)){
				// +5 damage, remove 1 blight
				damageToInvaders += 5;
				gameState.AddBlight(target,-1);
			}				

			gameState.DamageInvaders(target, damageToInvaders);
			return Task.CompletedTask;
		}

	}

}
