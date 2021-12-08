using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallOnMidnightsDream {


		[SpiritCard("Call on Midnight's Dream",0, Element.Moon,Element.Animal)]
		[Fast]
		[FromPresence(0,Target.DahanOrInvaders)] // adding dahan or invaders so that card does something.
		static public Task ActAsync(TargetSpaceCtx ctx) {

			return ctx.SelectActionOption(
				new ActionOption("Draw Major Power", ()=>DrawMajorOrGetEnergy(ctx), ctx.Dahan.Any ),
				new ActionOption("2 fear", () => ctx.AddFear(2), ctx.HasInvaders )
			);

		}

		static async Task DrawMajorOrGetEnergy( TargetSpaceCtx ctx ) {
			// if target land has dahan, gain a major power.
			var major = (await ctx.DrawMajor(true)).Selected;
			// If you Forget this Power, gain energy equal to dahan and you may play the major power immediately paying its cost
			if(!ctx.Self.Hand.Contains( major )) // because you discarded it
				ctx.Self.Energy += ctx.Dahan.Count;
		}
	}
}
