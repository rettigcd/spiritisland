using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {
	
	[TestFixture]
	public class Ocean_GrowthTests : GrowthTests {

		[SetUp]
		public void SetUp_Ocean() => Given_SpiritIs(new Ocean());

		[Test]
		public void G1(){
			// reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
		}

		[TestCase("A0A0")]
		[TestCase("A0B0")]
		[TestCase("B0B0")]
		public void TwoPresenceInOceans(string newPresenceLocation){
			// +1 presence range any ocean, +1 presense in any ociean, +1 energy

			// Given: island has 2 boards, hence 2 oceans
			gameState.Island = new Island(Board.GetBoardA(),Board.GetBoardB());

			When_Growing(1,newPresenceLocation);
			
			Assert_GainEnergy(1);
		}

		[Test]
		public void G3(){
			// gain power card, push 1 presense from each ocian,  add presense on costal land range 1
		}

	}
}
