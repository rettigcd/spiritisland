using System;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Growth {
	
	public class Ocean_GrowthTests : GrowthTests {

		public Ocean_GrowthTests():base(new Ocean()){}

		[Theory]
		[InlineData("A0","","A0")]
		[InlineData("A0B0","","A0B0")]
		[InlineData("A0B0C0","","A0B0C0")]
		[InlineData("A1","","A0")]
		[InlineData("A1B1","","A0B0")]
		[InlineData("A1B1C1","","A0B0C0")]
		[InlineData("A1A2","A1>A0","A0A2")]    // need to define which presence to move
		[InlineData("A1A2","A2>A0","A0A1")]    // need to define which presence to move
		[InlineData("A1A2B1C1C2","A2>A0,C1>C0","A0A1B0C0C2")]    // need to define which presence to move
		public void ReclaimGather_GatherParts(string starting, string select, string ending) {
			Given_IslandIsABC();
			Given_HasPresence( starting );

			When_Growing( 0, Resolve_GatherPresence( select ) );

			// Then: nothing to gather
			Assert_BoardPresenceIs( ending );
		}


		[Theory]
		[InlineData("A1A2")]    // need to define which presence to move
		public void ReclaimGather_GatherParts_Unresolved(string starting){

			// Given: 3-board island
			gameState.Island = new Island(BoardA,BoardB,BoardC);

			Given_HasPresence( starting );

			Assert.Throws<InvalidOperationException>(()=>When_Growing(0));
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

			When_Growing(1,Resolve_PlacePresence("A0A0;A0B0;B0B0"));
			
			Assert_GainEnergy(1);
		}

		[Theory]
		[InlineData("A0","A0>A2","A1;A2;A3","A1A2")]
		public void PowerPlaceAndPush( string starting, string push, string placeOptions, string ending ){
			// gain power card, push 1 presense from each ocean,  add presense on costal land range 1
			gameState.Island = new Island(BoardA,BoardB,BoardC);
			Given_HasPresence( starting );

			var resolvers = Resolve_PushPresence( push )
				.Include( Resolve_PlacePresence(placeOptions) );
			When_Growing(2,resolvers);

			Assert_GainPowercard(1);
			Assert_BoardPresenceIs(ending);
		}

		#region helpers

		void Given_IslandIsABC() {
			// Given: 3-board island
			gameState.Island = new Island( BoardA, BoardB, BoardC );
		}

		#endregion

	}
}
