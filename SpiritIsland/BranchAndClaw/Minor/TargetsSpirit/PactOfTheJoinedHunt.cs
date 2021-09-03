using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class PactOfTheJoinedHunt {

		[MinorCard( "Pact of the Joined Hunt", 1, Speed.Slow, Element.Sun, Element.Plant, Element.Animal )]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {
			// Target spirit gathers 1 dahan into one of their lands

			// 1 damage in that land per dahan present

			throw new NotImplementedException();
		}

	}
}
