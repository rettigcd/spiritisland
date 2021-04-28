using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {
	[TestFixture]
	public class Lightning_GrowthTests : GrowthTests{

		[SetUp]
		public void SetUp_Lightning() => Given_SpiritIs(new Lightning());

		[Test]
		public void Lightning_Reclaim(){
			// * reclaim, +1 power card, +1 energy

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
			Assert_GainEnergy(1);

		}

		[Test]
		public void Lightning_PresenseAndEnergy() {
			// +1 presense range 1, +3 energy

			When_Growing( 2 );

			Assert_GainEnergy( 3 );
			Assert_Add1Presence_Range1();
		}

		[Test]
		public void Lightning_2Presence(){
			// +1 presense range 2, +1 prsense range 0

			When_Growing( 1 );

			Assert_GainEnergy( 0 );

			spirit.Presence.Add( board.spaces[3] ); 
			Assert_NewPresenceOptions( "A1A1;A1A3;A2A2;A2A3;A3A3;A3A4;A3A5;A4A4;A5A5" );

		}


	}

}
