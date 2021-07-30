using System.Collections.Generic;
using SpiritIsland.Basegame;
using Xunit;

namespace SpiritIsland.Tests.Base.Spirits {

	public class VitalStrength_GrowthTests : GrowthTests {

		public VitalStrength_GrowthTests():base( new VitalStrength() ){}

		[Fact]
		public void ReclaimAndPresence(){
			// reclaim, +1 presense range 2
			Given_HalfOfPowercardsPlayed();
			Given_HasPresence( board[3] );

			When_Growing( 0 );
			Resolve_PlacePresence( "A1;A2;A3;A4;A5");

			this.Assert_AllCardsAvailableToPlay();

		}

		[Fact]
		public void PowercardAndPresence() {
			// +1 power card, +1 presense range 0
			Given_HasPresence( board[4] );

			When_Growing( 1 );
			Resolve_PlacePresence( "A4");

			Assert.Equal(5,spirit.Hand.Count);
//			Assert_GainPowercard( 1 );
		}

		[Fact]
		public void PresenseAndEnergy(){
			// +1 presence range 1, +2 energy
			Given_HasPresence( board[1] );
			When_Growing(2);
			Resolve_PlacePresence( "A1;A2;A4;A5;A6",Track.Energy);
			Assert.Equal(3,spirit.EnergyPerTurn);
			Assert_HasEnergy(3+2);
		}

		[Theory]
		[InlineDataAttribute(1,2)]
		[InlineDataAttribute(2,3)]
		[InlineDataAttribute(3,4)]
		[InlineDataAttribute(4,6)]
		[InlineDataAttribute(5,7)]
		[InlineDataAttribute(6,8)]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth) {
			// energy:	2 3 4 6 7 8
			spirit.RevealedEnergySpaces = revealedSpaces;
			Assert_EnergyTrackIs( expectedEnergyGrowth );
		}

		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,1)]
		[InlineDataAttribute(3,2)]
		[InlineDataAttribute(4,2)]
		[InlineDataAttribute(5,3)]
		[InlineDataAttribute(6,4)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount){
			//	card:  1 1 2 2 3 4
			spirit.RevealedCardSpaces = revealedSpaces;
			Assert_CardTrackIs( expectedCardPlayCount );
		}




	}

}
