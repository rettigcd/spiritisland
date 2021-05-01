﻿using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class RampantGreen_GrowthTests : GrowthTests {

		public RampantGreen_GrowthTests(){
			Given_SpiritIs( new RampantGreen() );
		}

		// +1 presense to jungle or wetland - range 2(Always do this + one of the following)
		// reclaim, +1 power card
		// +1 presense range 1, play +1 extra card this turn
		// +1 power card, +3 energy

		[Fact]
		public void Reclaim_PowerCard_JWPresence() {
			// +1 presense to jungle or wetland - range 2(Always do this + one of the following)
			// reclaim, +1 power card
			Given_HasPresence( board[2] );

			When_Growing( 0, "A2;A3;A5" );

			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard( 1 );
		}

		[Fact]
		public void PlayExtraCard_2Presence(){
			// +1 presense to jungle or wetland - range 2
			// +1 presense range 1, play +1 extra card this turn

			// Presense Options
			Given_HasPresence( board[2] );

			Assert.Equal(1, spirit.NumberOfCardsPlayablePerTurn); // ,"Rampant Green should start with 1 card.");

			When_Growing( 1,"A1A2;A1A3;A1A5;A1A8;A2A2;A2A3;A2A4;A2A5;A3A3;A3A4;A3A5;"
				+"A4A5;A4A8;A5A5;A5A6;A5A7;A5A8"
			);

			// Player Gains +1 card to play this round
			Assert.Equal(2, spirit.NumberOfCardsPlayablePerTurn); // , "Should gain 1 card to play this turn.");
			// But count drops back down after played
			spirit.PlayAvailableCards(0);
			// Back to original
			Assert.Equal(1, spirit.NumberOfCardsPlayablePerTurn); // ,"Available card count should be back to original");

		}

		[Fact]
		public void GainEnergy_PowerCard_JWPresence(){
			// +1 presense to jungle or wetland - range 2
			// +1 power card, +3 energy
			Given_HasPresence( board[2] );

			When_Growing( 2, "A2;A3;A5" );

			Assert_GainEnergy(3);
			Assert_GainPowercard(1);
		}
		
	}
}