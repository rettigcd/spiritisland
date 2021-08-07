using System.Collections.Generic;
using SpiritIsland.Basegame;
using SpiritIsland.Core;
using Xunit;

namespace SpiritIsland.Tests.Base.Spirits {

	public class Thunderspeaker_GrowthTests : GrowthTests{

		public Thunderspeaker_GrowthTests():base( new ThunderSpeaker() ){
		}

		[Fact]
		public void ReclaimAnd2PowerCards() {
			// Growth Option 1 - Reclaim All, +2 Power cards
			Given_HalfOfPowercardsPlayed();

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay(6);
			Assert_HasEnergy(1);
		}

		[Theory]
		[InlineData( "3,5,8", "A3;A5" )]
		[InlineData( "3,4,8", "A3;A4" )]
		[InlineData( "4,8", "A4" )]
		[InlineData( "1,4,8", "A1;A4" )]
		public void TwoPresence( string initialDahanSquares, string expectedPresenseOptions ) {
			// +1 presense within 2 - contains dahan
			// +1 presense within 1 - contains dahan
			Given_HasPresence( board[3] );
			//	 And: dahan on initial spot
			foreach(string s in initialDahanSquares.Split( ',' ))
				gameState.AddDahan( board[int.Parse( s )] );

			When_Growing( 1 );
			Resolve_PlacePresence( expectedPresenseOptions, "PlacePresence(2,dahan)" );
			// PlacePresence(2,dahan)

			Assert_HasEnergy( 0 );

		}

		[Fact]
		public void PresenseAndEnergy() {
			// +1 presense within 1, +4 energy

			Given_HasPresence( board[1] );
			When_Growing(2);
			Resolve_PlacePresence( "A1;A2;A4;A5;A6");

			Assert.Equal(1,spirit.EnergyPerTurn);
			Assert_HasEnergy( 4+1 );

		}

		[Theory]
		[InlineDataAttribute(1,1,"")]
		[InlineDataAttribute(2,1,"A")]
		[InlineDataAttribute(3,2,"A")]
		[InlineDataAttribute(4,2,"AF")]
		[InlineDataAttribute(5,2,"AFS")]
		[InlineDataAttribute(6,3,"AFS")]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			// energy:	1 air 2 fire sun 3
			spirit.RevealedEnergySpaces = revealedSpaces;
			Assert_PresenceTracksAre( expectedEnergyGrowth, 1 );
			Assert_BonusElements( elements );
		}

		[Theory]
		[InlineDataAttribute(1,1,false)]
		[InlineDataAttribute(2,2,false)]
		[InlineDataAttribute(3,2,false)]
		[InlineDataAttribute(4,3,false)]
		[InlineDataAttribute(5,3,true)]
		[InlineDataAttribute(6,3,true)]
		[InlineDataAttribute(7,4,true)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount, bool canReclaim1 ){
			// card:	1 2 2 3 reclaim-1 3 4
			Given_HalfOfPowercardsPlayed();

			Given_HasPresence( board[3] );

			spirit.RevealedCardSpaces = revealedSpaces;
			Assert_PresenceTracksAre(1,expectedCardPlayCount);

			When_Growing(2);
			Resolve_PlacePresence( "A2;A3;A4");

			if( canReclaim1 )
				AndWhen_ReclaimingFirstCard();

		}

	}

}
