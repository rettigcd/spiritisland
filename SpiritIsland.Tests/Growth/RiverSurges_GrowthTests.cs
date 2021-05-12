using System;
using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class RiverSurges_GrowthTests : GrowthTests{

		public RiverSurges_GrowthTests():base( new RiverSurges() ){}

		[Fact]
		public void Reclaim_DrawCard_Energy() {

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard( 1 );
			Assert_GainEnergy( 1 );

		}

		[Fact]
		public void TwoPresence() {

			Given_HasPresence( board[1] );
			Assert.Equal(1,spirit.RevealedEnergySpaces);

			When_Growing( 1, Resolve_PlacePresence(
				"A1A1;A1A2;A1A4;A1A5;A1A6;"
				+"A2A2;A2A3;A2A4;A2A5;A2A6;"
				+"A3A4;"
				+"A4A4;A4A5;A4A6;"
				+"A5A5;A5A6;A5A7;A5A8;"
				+"A6A6;A6A8", Track.Energy, Track.Energy
			) );

			Assert_GainPowercard( 0 );
			Assert_GainEnergy( 0 );
			Assert.Equal(3,spirit.RevealedEnergySpaces);
		}

		[Fact]
		public void Power_Presence() {

			// +1 power card, 
			// +1 presense range 2
			Assert.Equal(1,spirit.RevealedEnergySpaces);
			Given_HasPresence( board[3] );

			When_Growing( 2, Resolve_PlacePresence("A1;A2;A3;A4;A5") );

			Assert_GainPowercard( 1 );
			Assert_GainEnergy( 0 );
			Assert.Equal(2,spirit.RevealedEnergySpaces);
		}

		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,2)]
		[InlineDataAttribute(3,2)]
		[InlineDataAttribute(4,3)]
		[InlineDataAttribute(5,4)]
		[InlineDataAttribute(6,4)]
		[InlineDataAttribute(7,5)]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth ){
			// energy:	1 2 2 3 4 4 5

			spirit.RevealedEnergySpaces = revealedSpaces;
			Assert_PresenceTracksAre(expectedEnergyGrowth,1);
		}

		[Theory]
		[InlineDataAttribute(1,1,false)]
		[InlineDataAttribute(2,2,false)]
		[InlineDataAttribute(3,2,false)]
		[InlineDataAttribute(4,3,false)]
		[InlineDataAttribute(5,3,true)]
		[InlineDataAttribute(6,4,true)]
		[InlineDataAttribute(7,5,true)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount, bool canReclaim1 ){
			// cards:	1 2 2 3 reclaim-1 4 5

			Given_HasPresence( board[3] );


			spirit.RevealedCardSpaces = revealedSpaces;
			Assert_PresenceTracksAre(1,expectedCardPlayCount);

			void Grow(){
				When_Growing(2,Resolve_Reclaim(0),Resolve_PlacePresence("A1;A2;A3;A4;A5"));
			}

			if(canReclaim1)
				Grow();
			else
				Assert.Throws<Exception>( Grow );
		}


	}

}
