using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class ShadowsFlicker_GrowthTests : GrowthTests {

		public ShadowsFlicker_GrowthTests (){
			Given_SpiritIs(new Shadows());
		}


		[Fact]
		public void Reclaim(){
			// reclaim, gain power Card
			When_Growing( 0 );
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
		}

		[Fact]
		public void PowerAndPresence(){
			// gain power card, add a presense range 1
			Given_HasPresence( board[1] );
			When_Growing(1,"A1;A2;A4;A5;A6");
			Assert_GainPowercard(1);
		}

		[Fact]
		public void PresenceAndEnergy(){
			// add a presence withing 3, +3 energy
			Given_HasPresence( board[3] );
			When_Growing( 2,"A1;A2;A3;A4;A5;A6;A7;A8" );

			Assert_GainEnergy(3);
		}

	}
}
