using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class InstrumentsOfTheirOwnRuin {

		[MajorCard( "Instruments of Their Own Ruin", 4, Speed.Fast, Element.Sun, Element.Fire, Element.Air, Element.Animal )]
		[FromSacredSite( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// add 1 strife
			// each invader with strife deals damage to other invaders in target land
			// if you have 4 sun, 2 fire 2 animal: Instead, if invaders ravage in target land, they damage invaders in adjacent lands instead of dahan and the land.  dahan in target land do not fight back.
			return Task.CompletedTask;
		}

	}

}
