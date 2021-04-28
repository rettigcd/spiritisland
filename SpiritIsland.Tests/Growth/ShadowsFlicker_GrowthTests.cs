using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {
	[TestFixture]
	public class ShadowsFlicker_GrowthTests : GrowthTests {

		[SetUp]
		public void SetUp_Shadows() => Given_SpiritIs(new Shadows());

		[Test]
		public void Reclaim(){
			// reclaim, gain power Card
			When_Growing( 0 );
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
		}

		[Test]
		public void PowerAndPresence(){
			// gain power card, add a presense range 1
			When_Growing( 1 );
			Assert_GainPowercard(1);
			Assert_Add1Presence_Range1();
		}

		[Test]
		public void PresenceAndEnergy(){
			// add a presence withing 3, +3 energy
			When_Growing( 2 );
			Assert_GainEnergy(3);
			Assert_AddPresense_Range3();
		}

	}
}
