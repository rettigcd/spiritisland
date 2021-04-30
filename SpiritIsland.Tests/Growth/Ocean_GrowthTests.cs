using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {
	
	[TestFixture]
	public class Ocean_GrowthTests : GrowthTests {

		[SetUp]
		public void SetUp_Ocean() => Given_SpiritIs(new Ocean());

		[Test]
		public void ReclaimGather(){
			// reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
			
			// Given: 3-board island
			gameState.Island = new Island(Board.A,Board.B,Board.C);

			// Nothing on board to gather - no error
			// 1 thing on board to gather - specify it - success
			// 1 thing on board to gather - don't specify it - fail!
			// 2 things on board to gather - can select either - success

			// multiple boards

			// ????????     mut


			When_Growing(0);

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
		}

		[Test]
		public void TwoPresenceInOceans(){
			// +1 presence range any ocean, +1 presense in any ociean, +1 energy

			// Given: island has 2 boards, hence 2 oceans
			gameState.Island = new Island(Board.A,Board.B);

			When_Growing(1,"A0A0;A0B0;B0B0");
			
			Assert_GainEnergy(1);
		}

		[Test]
		public void PowerPlaceAndPush(){
			// gain power card, push 1 presense from each ocian,  add presense on costal land range 1
		}

	}
}
