using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class ConfoundingMists {

		[MinorCard( "Confounding Mists", 1, Speed.Fast, Element.Air, Element.Water )]
		[FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// Defend 4
			ctx.Defend( 4 );

			// OR

			// each invader added to target land this turn may be immediatley pushed to any adjacent land
			// !!!

		}


	}

}
