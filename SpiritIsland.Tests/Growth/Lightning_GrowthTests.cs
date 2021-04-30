using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {
	[TestFixture]
	public class Lightning_GrowthTests : GrowthTests{

		[SetUp]
		public void SetUp_Lightning() => Given_SpiritIs(new Lightning());

		[Test]
		public void Reclaim(){
			// * reclaim, +1 power card, +1 energy

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
			Assert_GainEnergy(1);

		}

		[TestCase("A1")]
		[TestCase("A2")]
		[TestCase("A4")]
		[TestCase("A5")]
		[TestCase("A6")]
		public void Presense_Energy(string targetLand) {
			// +1 presense range 1, +3 energy

			spirit.InitPresence( board[1] );
			When_Growing(2,targetLand);

			Assert_GainEnergy( 3 );
		}

		[TestCase("A0")] // ocean
		[TestCase("A3")]
		[TestCase("A7")]
		[TestCase("A8")]
		public void Presense_Energy_InvalidLocations(string targetLand) {
			// +1 presense range 1, +3 energy
			spirit.InitPresence( board[1] );
			Assert.Throws<InvalidPresenceLocation>(()=>When_Growing(2,targetLand));

		}


		[Test]
		public void TwoPresence(){
			// +1 presense range 2, +1 prsense range 0
			spirit.InitPresence( board[3] ); 

			When_Growing( 1, "A1A1;A1A3;A2A2;A2A3;A3A3;A3A4;A3A5;A4A4;A5A5" );

			Assert_GainEnergy( 0 );

		}


	}

}
