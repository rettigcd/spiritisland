using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InstrumentsOfTheirOwnRuin {

		[MajorCard( "Instruments of Their Own Ruin", 4, Speed.Fast, Element.Sun, Element.Fire, Element.Air, Element.Animal )]
		[FromSacredSite( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// add 1 strife
			await ctx.AddStrife();

			// each invader with strife deals damage to other invaders in target land

			// !!! Ravage status must be known at start of round in order to detect this.

			// if you have 4 sun, 2 fire 2 animal:
			// Instead, if invaders ravage in target land, they damage invaders in adjacent lands instead of dahan and the land.
			// dahan in target land do not fight back.
		}

	}

}
