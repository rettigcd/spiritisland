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

		[Test]
		public void G2(){
			// +1 presence range any ocean, +1 presense in any ociean, +1 energy
			When_Growing(1);
			
		//	Assert_NewPresenceOptions("A0A0");
			Assert_GainEnergy(1);
		}

		[Test]
		public void G3(){
			// gain power card, push 1 presense from each ocian,  add presense on costal land range 1
		}

	}
}
