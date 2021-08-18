using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class GiftOfProliferation {

		[SpiritCard( "Gift of Proliferation", 1, Speed.Fast, Element.Moon, Element.Plant )]
		[TargetSpirit]
		static public Task ActionAsync( IMakeGamestateDecisions eng, Spirit target ) {
			// target spirit adds 1 presense up to 1 from their presesnse
			return target.MakeDecisionsFor( eng.GameState ).PlacePresence(1,Target.Any);
		}

	}
}
