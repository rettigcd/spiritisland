using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {
	
	[TestFixture]
	public class Bringer_GrowthTests : GrowthTests {

		[SetUp]public void SetUp_Bringer() => Given_SpiritIs(new Bringer());

		[Test] 
		public void ReclaimAll_PowerCard(){
			// reclaim, +1 power card
			When_Growing(0);
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
		}

		[Test] 
		public void Reclaim1_Presence(){
			// reclaim 1, add presense range 0
			Given_HasPresence( board[4] );

			When_Growing(1, "A4", Reclaim1.Resolve(spirit.PlayedCards[0]) );

			Assert.That( spirit.AvailableCards.Count,Is.EqualTo(3) );

		}

		[Test] 
		public void PowerCard_Presence(){
			// +1 power card, +1 pressence range 1
			Given_HasPresence( board[1] );
			When_Growing(2,"A1;A2;A4;A5;A6");
			Assert_GainPowercard(1);
		}

		[Test] 
		public void PresenseOnPieces_Energy(){

			board = LineBoard.MakeBoard();
			Given_HasPresence(board[5]);
			gameState.AddDahan(board[6]);
			gameState.AddExplorer(board[7]);
			gameState.AddTown(board[8]);
			gameState.AddCity(board[9]);

			// add presense range 4 Dahan or Invadors, +2 energy
			When_Growing(3,"T6;T7;T8;T9");

			Assert_GainEnergy(2);

		}

	}
}
