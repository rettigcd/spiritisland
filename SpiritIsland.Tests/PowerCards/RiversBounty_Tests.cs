using SpiritIsland.PowerCards;
using Xunit;

namespace SpiritIsland.Tests {
	public class RiversBounty_Tests : SpiritCards_Tests {

		[Fact]
		public void RiversBounty_Stats() {
			var card = PowerCard.For<RiversBounty>();
			Assert_CardStatus( card, 0, Speed.Slow, "SWB" );
		}

	}

}
