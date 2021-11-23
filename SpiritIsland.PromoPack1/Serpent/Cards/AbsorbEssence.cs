using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {
	
	public class AbsorbEssence {

		[SpiritCard("Absorb Essence",2,Element.Moon, Element.Fire, Element.Water, Element.Earth),Fast]
		[AnotherSpirit]
		static public async Task ActAsync(TargetSpiritCtx ctx) {
			// gain 3 energy.
			ctx.Self.Energy += 3;

			// if card is traded to someone else, don't crash
			if(ctx.Self.Presence is not SerpentPresence serpentPresence) return;

			if(6 <= serpentPresence.AbsorbedPresences.Count) return;

			// move 1 of target spirit's presence from the board to your 'Deep Slumber' track.
			// Absorbed presence cannot be returned to play.
			var space = await ctx.OtherCtx.Presence.SelectDeployed("Select presence to be absorbed");
			ctx.OtherCtx.Presence.RemoveFrom(space);
			serpentPresence.AbsorbedPresences.Add(ctx.Other);

			// Target spirit gains 1 ANY and 1 energy
			ctx.Other.Energy += 1;
			ctx.Other.Elements[Element.Any]++;  // !!! ??? When are ANY converted?  Should spirit do that now instead of adding an ANY element?
		}

	}


}
