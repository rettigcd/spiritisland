using Shouldly;
using Xunit;

namespace SpiritIsland.Tests {

	public class PowerCardDeck_Tests {

		[Fact]
		public void Minor36Count() {
			var minorCards = PowerCard.GetMinors();
			minorCards.Length.ShouldBe( 36 );
		}

		[Fact]
		public void Major22Count() {
			var majorCards = PowerCard.GetMajors();
			majorCards.Length.ShouldBe( 22 );
		}


	}
}
