using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class EntwinedPower {


		[MajorCard( "Entwined Power", 2, Speed.Fast, Element.Moon, Element.Water, Element.Plant )]
		[TargetSpirit]
		static public async Task ActAsync( ActionEngine engine, Spirit target ) {
			var (self, gs) = engine;
			// You and target spirit may use each other's presence to target powers.

			// Target spirit gains a power Card.
			await target.CardDrawer.Draw( engine );

			// You gain one of the power Cards they did not keep.

			// !!!!if you have 2 water, 4 plant, you and target spirit each gain 3 energy and may gift the other 1 power from hand.
		}

	}
}
