using System.Collections.Generic;
using SpiritIsland.BranchAndClaw;
using SpiritIsland;
using Xunit;

namespace SpiritIsland.Tests.BranchAndClaw.Spirits {

	public class SharpFangs_GrowthTests : GrowthTests {

		public SharpFangs_GrowthTests():base( new SharpFangs { CardDrawer = new IncrementCountCardDrawer() } ) {
		
			// Setup for growth option B
			Given_HasPresence( board[2] ); // wetlands
			gameState.AddBeast(board[7]); // add beast to sand (not jungle)

		}

		// a) cost -1, reclaim cards, gain +1 power card
		// b) add a presense to jungle or a land with beasts
		// c) gain power card, gain +1 energy
		// d) +3 energy


		[Fact]
		public void AB(){
			// a) cost -1, reclaim cards, gain +1 power card
			// b) add a presense to jungle or a land with beasts ( range 3)
			Given_HalfOfPowercardsPlayed();

			When_Growing( 0 );
			Resolve_PlacePresence( "A3;A7;A8");

			Assert_AllCardsAvailableToPlay();  // A
			Assert_HasEnergy( -1 + 1);           // A  -- ??? !!! can we grow if we don't have the energy
			Assert_GainPowercard( 1 );         // A

			Assert_BoardPresenceIs("A2A3");    // B
		}

		[Fact]
		public void AC(){
			// a) cost -1, reclaim cards, gain +1 power card
			// c) gain power card, gain +1 energy

			Given_HalfOfPowercardsPlayed();
			When_Growing( 1 );

			Assert_AllCardsAvailableToPlay();  // A
			Assert_HasEnergy( 0+1 );            // A & C
			Assert_GainPowercard( 2 );         // A & C

		}

		[Fact]
		public void AD(){
			// a) cost -1, reclaim cards, gain +1 power card
			// d) +3 energy

			Given_HalfOfPowercardsPlayed();

			When_Growing( 2 );

			Assert_AllCardsAvailableToPlay();  // A
			Assert_GainPowercard( 1 );         // A
			Assert_HasEnergy( 2+1 );            // A & D

		}

		[Fact]
		public void BC(){
			// b) add a presense to jungle or a land with beasts ( range 3)
			// c) gain power card, gain +1 energy

			When_Growing( 3 );
			Resolve_PlacePresence( "A3;A7;A8");

			Assert_BoardPresenceIs("A2A3");    // B
			Assert_HasEnergy( 1 + 1 );            // C
			Assert_GainPowercard( 1 );         // C
		}

		[Fact]
		public void BD(){
			// b) add a presense to jungle or a land with beasts ( range 3)
			// d) +3 energy

			When_Growing( 4 );
			Resolve_PlacePresence( "A3;A7;A8");

			Assert_BoardPresenceIs("A2A3");    // B
			Assert_HasEnergy( 3 + 1 );            // D
		}

		[Fact]
		public void CD(){
			// c) gain power card, gain +1 energy
			// d) +3 energy

			When_Growing( 5 );

			Assert_GainPowercard( 1 );         // C
			Assert_HasEnergy( 1+3 + 1 );          // C + D
		}

		[Theory]
		[InlineDataAttribute(1,1,"")]
		[InlineDataAttribute(2,1,"B")]
		[InlineDataAttribute(3,1,"BP")]
		[InlineDataAttribute(4,2,"BP")]
		[InlineDataAttribute(5,2,"BPB")]
		[InlineDataAttribute(6,3,"BPB")]
		[InlineDataAttribute(7,4,"BPB")]
		public void EnergyTrack(int revealedSpaces, int expectedEnergyGrowth, string elements ){
			// energy:	1 animal plant 2 animal 3 4
			spirit.Presence.Energy.RevealedCount = revealedSpaces;
			Assert_PresenceTracksAre(expectedEnergyGrowth,2);
			When_Growing( 5 ); // finish growth to trigger elements
			Assert_BonusElements( elements);
		}

		[Theory]
		[InlineDataAttribute(1,2,0)]
		[InlineDataAttribute(2,2,0)]
		[InlineDataAttribute(3,3,0)]
		[InlineDataAttribute(4,3,1)]
		[InlineDataAttribute(5,4,1)]
		[InlineDataAttribute(6,5,2)]
		public void CardTrack(int revealedSpaces, int expectedCardPlayCount, int reclaimCount){
			// cards:	2 2 3 reclaim-1 4 5&reclaim-1
			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_PresenceTracksAre(1,expectedCardPlayCount);
			Given_HalfOfPowercardsPlayed();

			// Test the reclaim bit
			Given_HasPresence( board[3] );

			When_Growing(5);

			while(reclaimCount-->0)
				AndWhen_ReclaimingFirstCard();
		}

	}

}
