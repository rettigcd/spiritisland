using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {
	public class GiftOfThePrimordialDeeps {

		[SpiritCard("Gift of the Primordial Deeps", 1, Element.Moon,Element.Earth), Fast, TargetSpirit]
		public static Task ActAsync( TargetSpiritCtx ctx ) {
			// target spirit gains a minor power.
			// Target spirit chooses to either:
					// play it immediately by paying its cost
					// OR gains 1 moon and 1 earth.
			return Task.CompletedTask;
		}
	}


}
