using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SpiritIsland.Tests.Growth {
	
	[TestFixture]
	public class Bringer_GrowthTests : GrowthTests {

		[SetUp]public void SetUp_Bringer() => Given_SpiritIs(new Bringer());

		[Test] 
		public void ReclaimAllAndPowerCard(){
			// reclaim, +1 power card
			When_Growing(0);
			Assert_AllCardsAvailableToPlay();
			Assert_GainPowercard(1);
		}

		[Test] 
		public void Reclaim1AndPresence(){
			// reclaim 1, add presense range 0

			When_Growing(1, Reclaim1.Resolve(playerState.PlayedCards[0]) );

			Assert.That( playerState.AvailableCards.Count,Is.EqualTo(3) );

			Assert_Add1Presence_Range0();


		}

		[Test] 
		public void PowerCardAndPresence(){
			// +1 power card, +1 pressence range 1
			When_Growing(2);
			Assert_GainPowercard(1);
			Assert_Add1Presence_Range1();
		}

		[Test] 
		public void AddPresenseOnPiecesAndEnergy(){

			// add presense range 4 Dahan or Invadors, +2 energy
			When_Growing(3);

			Assert_GainEnergy(2);

			// Then: place presense, range 4, Dahan or Invadors
			board = LineBoard.MakeBoard();
			playerState.Presence.Add(board.spaces[5]);
			gameState.AddDahan(board.spaces[6]);
			gameState.AddExplorer(board.spaces[7]);
			gameState.AddTown(board.spaces[8]);
			gameState.AddCity(board.spaces[9]);
			Assert_NewPresenceOptions("T6;T7;T8;T9");
		}

	}
}
