using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {

	[TestFixture]
	public class VitalStrength_GrowthTests : GrowthTests {

		[SetUp]
		public void SetUp_VitalStrength() => Given_SpiritIs(new VitalStrength());

		[Test]
		public void ReclaimAndPresence(){
			// reclaim, +1 presense range 2
			Given_HasPresence( board[3] );

			When_Growing( 0, "A1;A2;A3;A4;A5" );

			this.Assert_AllCardsAvailableToPlay();

		}

		[Test]
		public void PowercardAndPresence() {
			// +1 power card, +1 presense range 0
			Given_HasPresence( board[4] );

			When_Growing( 1, "A4" );

			Assert_GainPowercard( 1 );
		}

		[Test]
		public void PresenseAndEnergy(){
			// +1 presence range 1, +2 energy
			Given_HasPresence( board[1] );
			When_Growing(2,"A1;A2;A4;A5;A6");
			Assert_GainEnergy(2);
		}

	}

}
