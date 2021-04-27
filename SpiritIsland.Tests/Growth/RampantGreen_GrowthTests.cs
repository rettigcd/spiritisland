using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {
	[TestFixture]
	public class RampantGreen_GrowthTests : GrowthTests {

		[SetUp]
		public void SetUp_RampantGreen() => Given_SpiritIs( new RampantGreen() );

		// +1 presense to jungle or wetland - range 2(Always do this + one of the following)
		// reclaim, +1 power card
		// +1 presense range 1, play +1 extra card this turn
		// +1 power card, +3 energy
		[Test]
		public void RampantGreenG0_() {
			// +1 presense to jungle or wetland - range 2(Always do this + one of the following)
			// reclaim, +1 power card

			When_Growing( 0 );

			Assert_AddPresenseInJungleOrWetland_Range2();
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard( 1 );
		}

		[Test]
		public void RampantGreenG1_(){
			// +1 presense to jungle or wetland - range 2
			// +1 presense range 1, play +1 extra card this turn
			Assert.That(playerState.NumberOfCardsPlayablePerTurn, Is.EqualTo(1),"Rampant Green should start with 1 card.");

			When_Growing( 1 );

			// Presense Options
			playerState.Presence.Add( board.spaces[2] );
			Assert_NewPresenceOptions( "A1A2;A1A3;A1A5;A1A8;A2A2;A2A3;A2A4;A2A5;A3A3;A3A4;A3A5;"
				+"A4A5;A4A8;A5A5;A5A6;A5A7;A5A8" );

			// Player Gains +1 card to play this round
			Assert.That(playerState.NumberOfCardsPlayablePerTurn, Is.EqualTo(2), "Should gain 1 card to play this turn.");
			// But count drops back down after played
			playerState.PlayAvailableCards(0);
			// Back to original
			Assert.That(playerState.NumberOfCardsPlayablePerTurn, Is.EqualTo(1),"Available card count should be back to original");

		}

		[Test]
		public void RampantGreenG2_(){
			// +1 presense to jungle or wetland - range 2
			// +1 power card, +3 energy
			When_Growing( 2 );
			Assert_AddPresenseInJungleOrWetland_Range2();
			Assert_GainEnergy(3);
			Assert_GainPowercard(1);
		}
		
	}
}
