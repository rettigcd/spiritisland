using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GuardianSerpents {

		[MinorCard( "Guardian Serpents", 1, Speed.Fast, Element.Sun, Element.Moon, Element.Earth, Element.Animal )]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {
			// Add 1 beast in one of target spirits lands

			// if target spirit has a SS in that land, defend 4 there
			throw new NotImplementedException();
		}


	}
}
