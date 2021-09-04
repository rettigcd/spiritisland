using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class SkyStretchesToShore {

		[MinorCard( "Sky Stretches to Shore", 1, Speed.Fast, Element.Sun, Element.Air, Element.Water, Element.Earth )]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {

			// this turn, target spirit may use 1 slow power as if it wer fast
			// or vice versa

			// Target Spirit gains +3 range for targeting costal lands only

			throw new NotImplementedException();
		}



	}
}
