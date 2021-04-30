using System;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Growth {
	
	public class Ocean_GrowthTests : GrowthTests {

		public Ocean_GrowthTests(){
			Given_SpiritIs(new Ocean());
		}

		[Theory]
		[InlineData("A0","A0")]
		[InlineData("A0B0","A0B0")]
		[InlineData("A0B0C0","A0B0C0")]
		public void ReclaimGather_GatherParts(string starting, string ending){

			// Given: 3-board island
			gameState.Island = new Island(BoardA,BoardB,BoardC);
			// one presence in A0 - ocean
			Given_HasPresence( starting );

			// Nothing on board to gather - no error
			// 1 thing on board to gather - specify it - success
			// 1 thing on board to gather - don't specify it - fail!
			// 2 things on board to gather - can select either - success

			When_Growing(0);

			// Then: nothing to gather
			Assert_BoardPresenceIs(ending);
		}

		void Assert_BoardPresenceIs( string expected ) {
			var actual = spirit.Presence.Select(s=>s.Label).OrderBy(l=>l).Join();
			Assert.Equal(expected, actual); // , Is.EqualTo(expected),"Presence in wrong place");
		}

		[Fact]
		public void ReclaimGather_NonGatherParts(){
			// reclaim, +1 power, gather 1 presense into EACH ocean, +2 energy
			
			When_Growing(0);
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
			Assert_GainEnergy(2);
		}

		[Fact]
		public void TwoPresenceInOceans(){
			// +1 presence range any ocean, +1 presense in any ociean, +1 energy

			// Given: island has 2 boards, hence 2 oceans
			gameState.Island = new Island(BoardA,BoardB);

			When_Growing(1,"A0A0;A0B0;B0B0");
			
			Assert_GainEnergy(1);
		}

		[Fact]
		public void PowerPlaceAndPush(){
			// gain power card, push 1 presense from each ocian,  add presense on costal land range 1
		}

	}
}
