using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class GiftOfPower {

		[MinorCard( "Gift of Power", 0, Speed.Slow,"moon, water, earth, plant")]
		[TargetSpirit]
		static public Task ActAsync( ActionEngine engine, Spirit target ) {
			// gain a minor power card
			return engine.Self.DrawPowerCard(engine,"minor"); 
		}

	}
}
