using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {
	
	public class AbsorbEssence {

	[SpiritCard("Absorb Essence",2,Element.Moon, Element.Fire, Element.Water, Element.Earth),Fast]
	[TargetSpirit]

		static public Task ActAsync(TargetSpiritCtx ctx) {
			// gain 3 energy.
			ctx.Self.Energy += 3;

			// move 1 of target spirit's presence from the board to your 'Deep Slumber' track.
			// Absorbed presence cannot be returned to play.
			// !!!

			// Target spirit gains 1 ANY and 1 energy
			ctx.Other.Energy += 1;
			ctx.Other.Elements[Element.Any] ++;
			
			return Task.CompletedTask;
		}

	}


}
