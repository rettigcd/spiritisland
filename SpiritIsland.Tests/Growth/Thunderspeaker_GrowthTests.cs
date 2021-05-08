﻿using Xunit;

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
		[InlineData( "3,5,8", "A3A3;A3A5;A5A5;A5A8" )]
		[InlineData( "3,4,8", "A3A3;A3A4;A4A4;A4A8" )]
		[InlineData( "4,8", "A4A4;A4A8" )]
		[InlineData( "1,4,8", "A1A1;A1A4;A4A4;A4A8" )]
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

	}

}
