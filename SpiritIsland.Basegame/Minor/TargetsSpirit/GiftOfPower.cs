using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class GiftOfPower {

		[MinorCard( "Gift of Power", 0, Speed.Slow,"moon, water, earth, plant")]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {
			// gain a minor power card
			return ctx.Target.DrawMinor(ctx.GameState); 
		}

	}
}
