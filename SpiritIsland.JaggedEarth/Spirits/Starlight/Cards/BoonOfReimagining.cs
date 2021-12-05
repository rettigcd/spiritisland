using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class BoonOfReimagining {

		[SpiritCard("Boon of Reimagining", 1, Element.Moon), Slow, AnySpirit]
		public static async Task ActAsync( TargetSpiritCtx ctx ) {
			var otherCtx = ctx.OtherCtx;

			// Target Spirit may Forget a Power Card from hand or discard.
			var powerCard = await otherCtx.Self.ForgetPowerCard( Present.Done );

			// If they do, they draw 6 minor Power Cards and gain 2 of them.
			if( powerCard != null )
				await otherCtx.Self.DrawMinor(otherCtx.GameState, 6, 2);

			// If you target another Spirit, they gain 1 Energy.
			if( ctx.Self != ctx.Other )
				ctx.Other.Energy++;
		}

	}

}
