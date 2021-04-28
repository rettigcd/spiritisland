using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {

	[TestFixture]
	public class RiverSurges_GrowthTests : GrowthTests{

		[SetUp]
		public void SetUp_RiverSurges() => Given_SpiritIs( new RiverSurges() );

		[Test]
		public void Reclaim_DrawCard_Energy() {

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard( 1 );
			Assert_GainEnergy( 1 );

		}

		[Test]
		public void TwoPresence() {

			When_Growing( 1, "A1A1;A1A2;A1A4;A1A5;A1A6;"
				+"A2A2;A2A3;A2A4;A2A5;A2A6;"
				+"A3A4;"
				+"A4A4;A4A5;A4A6;"
				+"A5A5;A5A6;A5A7;A5A8;"
				+"A6A6;A6A8" );

			Assert_GainPowercard( 0 );
			Assert_GainEnergy( 0 );

			spirit.InitPresence( board[1] );
			Assert_NewPresenceOptions(); // connected land, but not ocean

		}

		[Test]
		public void Power_Presence() {

			// +1 power card, 
			// +1 presense range 2

			When_Growing( 2, "A1;A2;A3;A4;A5" );

			Assert_GainPowercard( 1 );
			Assert_GainEnergy( 0 );

			spirit.InitPresence( board[3] );
			Assert_NewPresenceOptions();

		}

	}

}
