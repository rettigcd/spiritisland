﻿using System.Collections.Generic;
using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class Keeper_GrowthTests : GrowthTests {

		public Keeper_GrowthTests():base(new Keeper()){}

		// a) reclaim, +1 energy
		// b) +1 power card
		// c) add presense range 3 containing wilds or presense, +1 energy
		// d) -3 energy, +1 power card, add presense to land without blight range 3

		[Fact]
		public void A_Reclaim_Energy_B_Powercard() {
			// a) reclaim, +1 energy
			// b) +1 power card

			When_Growing( 0 );

			Assert_AllCardsAvailableToPlay();
			Assert_GainEnergy( 1 );
			Assert_GainPowercard( 1 );
		}

		[Fact]
		public void A_Reclaim_Energy_C_Presence_Energy() {
			// a) reclaim, +1 energy
			// c) add presense range 3 containing (wilds or presense), +1 energy

			Given_HasPresence( board[3] );
			base.Given_HasWilds( board[8] ); // 3 spaces away

			When_Growing( 1, Resolve_PlacePresence( "A3;A8" ) );

			Assert_AllCardsAvailableToPlay();   // A
			Assert_GainEnergy( 2 );             // A & C
			Assert_BoardPresenceIs( "A3A3" );     // C
		}

		[Fact]
		public void A_Reclaim_Energy_D_Presence_PowerCard_LoseEnergy(){
			// a) reclaim, +1 energy
			// d) -3 energy, +1 power card, add presense to land without blight range 3

			// Given: presence on board A  (default island is Board A)
			Given_HasPresence( board[3] );
			Given_BlightEverywhereExcept7();

			When_Growing( 2, Resolve_PlacePresence("A7") );

			Assert_AllCardsAvailableToPlay();   // A
			Assert_GainEnergy( -2 );            // A & D
			Assert_GainPowercard( 1 );          // D
			Assert_BoardPresenceIs("A3A7");     // D
						
		}

		[Fact]
		public void B_Powercard_C_Presence_Energy() {
			// b) +1 power card
			// c) add presense range 3 containing (wilds or presense), +1 energy

			// Given: presence at A3  (default island is Board A)
			Given_HasPresence( board[3] );
			// Given: 1 wilds, 3 away
			Given_HasWilds( board[8] );

			When_Growing( 3, Resolve_PlacePresence( "A3;A8" ) );

			Assert_GainPowercard( 1 );          // B
			Assert_GainEnergy( 1 );             // C
			Assert_BoardPresenceIs( "A3A3" );     // C
		}

		[Fact]
		public void B_Powercard_D_Presence_PowerCard_LoseEnergy() {
			// b) +1 power card
			// d) -3 energy, +1 power card, add presense to land without blight range 3

			// Given: presence on board A  (default island is Board A)
			Given_HasPresence( board[3] );
			Given_BlightEverywhereExcept7();

			When_Growing( 4, Resolve_PlacePresence( "A7" ) );

			Assert_GainPowercard( 2 );          // B & D
			Assert_GainEnergy( -3 );            // D
			Assert_BoardPresenceIs( "A3A7" );     // D

		}

		[Fact]
		public void C_Presence_Energy_D_Presence_PowerCard_LoseEnergy() {
			// c) add presense range 3 containing (wilds or presense), +1 energy
			// d) -3 energy, +1 power card, add presense to land without blight range 3

			// Given: presence on board A  (default island is Board A)
			Given_HasPresence( board[3] );
			Given_HasWilds( board[8] );
			Given_BlightEverywhereExcept7();

			When_Growing( 5, Resolve_PlacePresence( "A3;A7;A8" ) );

			Assert_GainEnergy( -2 );            // C & D
			Assert_GainPowercard( 1 );          // D
			Assert_BoardPresenceIs( "A3A3" ); // D

		}

		[Theory]
		[InlineDataAttribute(1,2,"")]
		[InlineDataAttribute(2,2,"S")]
		[InlineDataAttribute(3,4,"S")]
		[InlineDataAttribute(4,5,"S")]
		[InlineDataAttribute(5,5,"SP")]
		[InlineDataAttribute(6,7,"SP")]
		[InlineDataAttribute(7,8,"SP")]
		[InlineDataAttribute(8,9,"SP")]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			// energy:	2 sun 4 5 plant 7 8 9
			spirit.RevealedEnergySpaces = revealedSpaces;
			Assert_EnergyTrackIs( expectedEnergyGrowth );
			Assert_BonusElements( elements );
		}

		[Theory]
		[InlineDataAttribute(1,1)]
		[InlineDataAttribute(2,2)]
		[InlineDataAttribute(3,2)]
		[InlineDataAttribute(4,3)]
		[InlineDataAttribute(5,4)]
		[InlineDataAttribute(6,5)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount){
			// card:	1 2 2 3 4 5
			spirit.RevealedCardSpaces = revealedSpaces;
			Assert_CardTrackIs(expectedCardPlayCount);
		}


		void Given_BlightEverywhereExcept7() {
			gameState.AddBlight( board[1] );
			gameState.AddBlight( board[2] );
			gameState.AddBlight( board[3] );
			gameState.AddBlight( board[4] );
			gameState.AddBlight( board[5] );
			gameState.AddBlight( board[6] );
			gameState.AddBlight( board[8] );
		}

	}

}
