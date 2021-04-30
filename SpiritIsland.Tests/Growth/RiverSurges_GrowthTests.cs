using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class RiverSurges_GrowthTests : GrowthTests{

		public RiverSurges_GrowthTests(){
			Given_SpiritIs( new RiverSurges() );
		}

		[Fact]
		public void Reclaim_DrawCard_Energy() {

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard( 1 );
			Assert_GainEnergy( 1 );

		}

		[Fact]
		public void TwoPresence() {

			Given_HasPresence( board[1] );

			When_Growing( 1, "A1A1;A1A2;A1A4;A1A5;A1A6;"
				+"A2A2;A2A3;A2A4;A2A5;A2A6;"
				+"A3A4;"
				+"A4A4;A4A5;A4A6;"
				+"A5A5;A5A6;A5A7;A5A8;"
				+"A6A6;A6A8" );

			Assert_GainPowercard( 0 );
			Assert_GainEnergy( 0 );

		}

		[Fact]
		public void Power_Presence() {

			// +1 power card, 
			// +1 presense range 2
			Given_HasPresence( board[3] );

			When_Growing( 2, "A1;A2;A3;A4;A5" );

			Assert_GainPowercard( 1 );
			Assert_GainEnergy( 0 );


		}

	}

}
