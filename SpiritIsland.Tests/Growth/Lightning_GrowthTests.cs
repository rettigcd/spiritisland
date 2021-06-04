using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class Lightning_GrowthTests : GrowthTests{

		public Lightning_GrowthTests()
			:base(new Lightning()){}

		[Fact]
		public void Reclaim_Power_Energy(){
			// * reclaim, +1 power card, +1 energy

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
			Assert_GainEnergy(1);

		}

		[Fact]
		public void Presense_Energy() {
			// +1 presense range 1, +3 energy

			Given_HasPresence( board[1] );
			When_Growing( 2, Resolve_PlacePresence("A1;A2;A4;A5;A6") );

			Assert_GainEnergy( 3 );
		}

		[Fact]
		public void TwoPresence(){
			// +1 presense range 2, +1 prsense range 0
			Given_HasPresence( board[3] ); 

			When_Growing( 1, Resolve_PlacePresence("A1;A2;A3;A4;A5",0) );

			Assert_GainEnergy( 0 );

		}

		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,1)]
		[InlineDataAttribute(3,2)]
		[InlineDataAttribute(4,2)]
		[InlineDataAttribute(5,3)]
		[InlineDataAttribute(6,4)]
		[InlineDataAttribute(7,4)]
		[InlineDataAttribute(8,5)]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth) {
			// energy: 1 1 2 2 3 4 4 5
			spirit.RevealedEnergySpaces = revealedSpaces;
			Assert_EnergyTrackIs( expectedEnergyGrowth );
		}

		[Theory]
		[InlineDataAttribute(1,2)]
		[InlineDataAttribute(2,3)]
		[InlineDataAttribute(3,4)]
		[InlineDataAttribute(4,5)]
		[InlineDataAttribute(5,6)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount){
			// card:   2 3 4 5 6
			spirit.RevealedCardSpaces = revealedSpaces;
			Assert_CardTrackIs( expectedCardPlayCount );
		}


	}

}
