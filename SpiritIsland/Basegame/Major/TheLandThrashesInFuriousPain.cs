using SpiritIsland;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class TheLandThrashesInFuriousPain {

		[MajorCard("The Land Throashes in Furious Pain",4, Speed.Slow, Element.Moon, Element.Fire,Element.Earth)]
		[FromPresence(2,Target.Blight)]
		static public async Task ActAsync(ActionEngine engine,Space target) {
			var (self, gs) = engine;

			ApplyDamageFromBlight( target, gs );

			// if you have 3 moon 3 earth
			var el = self.Elements;
			if( 3<= el[Element.Moon] && 3<=el[Element.Earth]) {
				// repeat on an adjacent land.
				var alsoTarget = await engine.SelectSpace( "Select adjacent land to receive damage from blight", target.Adjacent);
				ApplyDamageFromBlight( alsoTarget, gs );
			}
		}

		static void ApplyDamageFromBlight( Space target, GameState gs ) {
			int damage = gs.GetBlightOnSpace( target ) * 2  // 2 damage per blight in target land
				+ target.Adjacent.Sum( x => gs.GetBlightOnSpace( x ) ); // +1 damage per blight in adjacent lands
			gs.DamageInvaders( target, damage );
		}
	}
}
