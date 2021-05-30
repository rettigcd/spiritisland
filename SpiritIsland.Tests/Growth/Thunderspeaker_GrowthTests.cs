using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class Thunderspeaker_GrowthTests : GrowthTests{

		public Thunderspeaker_GrowthTests():base( new ThunderSpeaker() ){
		}

		[Fact]
		public void ReclaimAnd2PowerCards() {
			// Growth Option 1 - Reclaim All, +2 Power cards

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(2);
			Assert_GainEnergy(0);
		}

		[Theory]
		[InlineData( "3,5,8", "A3A3;A3A5;A5A3;A5A5;A5A8" )]
		[InlineData( "3,4,8", "A3A3;A3A4;A4A3;A4A4;A4A8" )]
		[InlineData( "4,8", "A4A4;A4A8" )]
		[InlineData( "1,4,8", "A1A1;A1A4;A4A1;A4A4;A4A8" )]
		public void TwoPresence( string initialDahanSquares, string expectedPresenseOptions ) {
			// +1 presense within 2 - contains dahan
			// +1 presense within 1 - contains dahan
			Given_HasPresence( board[3] );
			//	 And: dahan on initial spot
			foreach(string s in initialDahanSquares.Split( ',' ))
				gameState.AddDahan( board[int.Parse( s )] );

			When_Growing( 1,Resolve_PlacePresence(expectedPresenseOptions) );

			Assert_GainEnergy( 0 );

		}

		[Fact]
		public void PresenseAndEnergy() {
			// +1 presense within 1, +4 energy

			Given_HasPresence( board[1] );
			When_Growing(2,Resolve_PlacePresence("A1;A2;A4;A5;A6"));

			Assert_GainEnergy( 4 );

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

			Given_HasPresence( board[3] );

			spirit.RevealedCardSpaces = revealedSpaces;
			Assert_PresenceTracksAre(1,expectedCardPlayCount);

			var list = new List<IResolver>{ Resolve_PlacePresence("A2;A3;A4") };
			if( canReclaim1 )
				list.Add( Resolve_Reclaim(0) );

			When_Growing(2,list.ToArray());

		}

	}

}
