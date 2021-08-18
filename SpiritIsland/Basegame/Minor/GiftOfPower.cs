using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class GiftOfPower {

		[MinorCard( "Gift of Power", 0, Speed.Slow,"moon, water, earth, plant")]
		[TargetSpirit]
		static public Task ActAsync( IMakeGamestateDecisions engine, Spirit target ) {
			// gain a minor power card
			return target.CardDrawer.DrawMinor(target,engine.GameState,null); 
		}

	}
}
