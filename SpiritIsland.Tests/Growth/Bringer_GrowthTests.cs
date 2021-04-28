﻿using System;
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
			spirit.InitPresence( board[4] );

			When_Growing(1, "A4", Reclaim1.Resolve(spirit.PlayedCards[0]) );

			Assert.That( spirit.AvailableCards.Count,Is.EqualTo(3) );

			Assert_NewPresenceOptions();

		}

		[Test] 
		public void PowerCard_Presence(){
			// +1 power card, +1 pressence range 1
			When_Growing(2);
			Assert_GainPowercard(1);
			Assert_Add1Presence_Range1();
		}

		[Test] 
		public void PresenseOnPieces_Energy(){

			// add presense range 4 Dahan or Invadors, +2 energy
			When_Growing(3,"T6;T7;T8;T9");

			Assert_GainEnergy(2);

			// Then: place presense, range 4, Dahan or Invadors
			board = LineBoard.MakeBoard();
			spirit.InitPresence(board[5]);
			gameState.AddDahan(board[6]);
			gameState.AddExplorer(board[7]);
			gameState.AddTown(board[8]);
			gameState.AddCity(board[9]);
			Assert_NewPresenceOptions();
		}

	}
}
