using Shouldly;
using Xunit;

namespace SpiritIsland.Tests {

	public class PowerCardDeck_Tests {

		[Fact]
		public void Minor32Count() {
			var minorCards = PowerCard.GetMinors();
			minorCards.Length.ShouldBe( 36 );
		}

		[Fact]
		public void Major32Count() {
			var majorCards = PowerCard.GetMajors();
			const int notImplementedCount = 3;
			majorCards.Length.ShouldBe( 22-notImplementedCount );
		}


	}
}
