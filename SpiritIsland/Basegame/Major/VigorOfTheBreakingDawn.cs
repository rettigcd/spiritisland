using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class VigorOfTheBreakingDawn {

		[MajorCard("Vigor of the Breaking Dawn",3,Speed.Fast,Element.Sun,Element.Animal)]
		[FromPresence(2,Target.Dahan)]
		public static async Task ActAsync(ActionEngine engine,Space target){
			var (spirit,gs) = engine;

			// 2 damage per dahan in target land
			gs.DamageInvaders(target,2*gs.GetDahanOnSpace(target));

			if( spirit.Elements.Contains("3 sun,2 animal") ){

				// you may push up to 2 dahan.
				var pushedToLands = await engine.PushUpToNDahan(target, 2);

				// 2 damage per dahan
				foreach(var neighbor in pushedToLands )
					gs.DamageInvaders( neighbor, 2*gs.GetDahanOnSpace(neighbor) );
			}
		}

	}

}
