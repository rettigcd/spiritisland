using SpiritIsland.PowerCards;
using Xunit;

namespace SpiritIsland.Tests {

	public class RiversBounty_Tests : SpiritCards_Tests {

		[Fact]
		public void RiversBounty_Stats() {
			var card = PowerCard.For<RiversBounty>();
			Assert_CardStatus( card, 0, Speed.Slow, "SWB" );
		}


		// 1 target, 0 dahan, nothing to gather => resolved, no change
		// 1 target, 0 dahan, 1 to gather       => resolved, dahan gathered, no child
		// 1 target, 0 dahan, 2 to gather       => select 1st source, resolved, dahan gathered, child!
		// 1 target, 0 dahan, 3 to gather       => select 1st source, select 2nd source, resolved, gathered, child

		// 1 target, 1 dahan, nothing to gather  => resolved, no change
		// 1 target, 1 dahan, 1 to gather        => resolved, dahan gathered, child!
		// 1 target, 1 dahan, 2 to gather        => select 1st source, resolved, dahan gathered, child!

		// 1 target, 2 dahan, nothing to gather  => resolved, child!
		// 1 target, 2 dahan, 2 to gather        => select 1st source, resolved, gathered, child



		// !!! Allow not gathering all availble - Select # to Gather
	}

}
