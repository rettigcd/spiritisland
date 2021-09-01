﻿using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.SinglePlayer;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests.BranchAndClaw.Spirits {

	public class SharpFangs_GrowthTests : GrowthTests {

		static Spirit InitSpirit() {
			return new SharpFangs {
				CardDrawer = new PowerProgression(
					PowerCard.For<RainOfBlood>(),
					PowerCard.For<GnawingRootbiters>()
				),
			};
		}

		readonly GameState_BranchAndClaw gsbac;

		public SharpFangs_GrowthTests() : base( InitSpirit() ) {

			gsbac = new GameState_BranchAndClaw( spirit, board );
			gameState = gsbac;

			// Setup for growth option B
			Given_HasPresence( board[2] ); // wetlands
			gsbac.Beasts.AddOneTo( board[7] ); // add beast to sand (not jungle)

		}

		// a) cost -1, reclaim cards, gain +1 power card
		// b) add a presense to jungle or a land with beasts
		// c) gain power card, gain +1 energy
		// d) +3 energy

		[Fact]
		public void AB() {
			// a) cost -1, reclaim cards, gain +1 power card
			// b) add a presense to jungle or a land with beasts ( range 3)
			Given_HalfOfPowercardsPlayed();

			When_SharpFangsGrow( 5 );
			Activate_A();
			Activate_B();

			Assert_AllCardsAvailableToPlay( 4+1);  // A
			Assert_HasEnergy( -1 + 1 );         // A
			Assert_HasPowerProgressionCard( 0 );    // A

			Assert_BoardPresenceIs( "A2A3" );    // B
		}

		[Fact]
		public void AC() {
			// a) cost -1, reclaim cards, gain +1 power card
			// c) gain power card, gain +1 energy

			Given_HalfOfPowercardsPlayed();
			When_SharpFangsGrow( 0 );
			Activate_A();
			Activate_C();

			Assert_AllCardsAvailableToPlay( 5 + 1 );  // A
			Assert_HasEnergy( 0 + 1 );            // A & C
			Assert_HasPowerProgressionCard( 0 );  // A
			Assert_HasPowerProgressionCard( 1 );  // C
		}

		[Fact]
		public void AD() {
			// a) cost -1, reclaim cards, gain +1 power card
			// d) +3 energy

			Given_HalfOfPowercardsPlayed();

			When_SharpFangsGrow( 1 );
			Activate_A();
			Activate_D();

			Assert_AllCardsAvailableToPlay(5);   // A
			Assert_HasPowerProgressionCard( 0 );    // A
			Assert_HasEnergy( 2 + 1 );          // A & D

		}

		[Fact]
		public void BC() {
			// b) add a presense to jungle or a land with beasts ( range 3)
			// c) gain power card, gain +1 energy

			When_SharpFangsGrow( 2 );
			Activate_B();
			Activate_C();

			Assert_BoardPresenceIs( "A2A3" );  // B
			Assert_HasEnergy( 1 + 1 );         // C
			Assert_HasPowerProgressionCard( 0 );    // A
		}

		[Fact]
		public void BD() {
			// b) add a presense to jungle or a land with beasts ( range 3)
			// d) +3 energy

			When_SharpFangsGrow( 3 );
			Activate_B();
			Activate_D();

			Assert_BoardPresenceIs( "A2A3" );  // B
			Assert_HasEnergy( 3 + 1 );         // D
		}

		[Fact]
		public void CD() {
			// c) gain power card, gain +1 energy
			// d) +3 energy

			When_SharpFangsGrow( 4 );
			Activate_C();
			Activate_D();

			Assert_HasPowerProgressionCard( 0 );    // C
			Assert_HasEnergy( 1 + 3 + 1 );     // C + D
		}

		[Theory]
		[InlineDataAttribute( 1, 1, "" )]
		[InlineDataAttribute( 2, 1, "B" )]
		[InlineDataAttribute( 3, 1, "BP" )]
		[InlineDataAttribute( 4, 2, "BP" )]
		[InlineDataAttribute( 5, 2, "BPB" )]
		[InlineDataAttribute( 6, 3, "BPB" )]
		[InlineDataAttribute( 7, 4, "BPB" )]
		public void EnergyTrack( int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			// energy:	1 animal plant 2 animal 3 4
			spirit.Presence.Energy.RevealedCount = revealedSpaces;

			When_SharpFangsGrow( 4 );
			Activate_C();
			Activate_D();

			Assert_PresenceTracksAre( expectedEnergyGrowth, 2 );
			Assert_BonusElements( elements );
		}

		[Theory]
		[InlineDataAttribute( 1, 2, 0 )]
		[InlineDataAttribute( 2, 2, 0 )]
		[InlineDataAttribute( 3, 3, 0 )]
		[InlineDataAttribute( 4, 3, 1 )]
		[InlineDataAttribute( 5, 4, 1 )]
		[InlineDataAttribute( 6, 5, 2 )]
		public void CardTrack( int revealedSpaces, int expectedCardPlayCount, int reclaimCount ) {
			// cards:	2 2 3 reclaim-1 4 5&reclaim-1
			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_PresenceTracksAre( 1, expectedCardPlayCount );
			Given_HalfOfPowercardsPlayed();

			// Test the reclaim bit
			Given_HasPresence( board[3] ); // added extra presence, need to 

			When_SharpFangsGrow( 4 );

			Activate_C();
			Activate_D();

			while(reclaimCount-- > 0)
				AndWhen_ReclaimingFirstCard();
		}


		Task When_SharpFangsGrow( int index ) {
			When_Growing( index );
			var task = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();

			// remove the Replace-presnece-with-beast action
			spirit.Action.Choose( "ReplacePresenceWithBeast" );
			if(spirit.Presence.Placed.Count>1)
				spirit.Action.Choose("Done"); // skip

			return task;
		}

		void Activate_A() {
			spirit.Activate_GainEnergy();    // A
			spirit.Activate_ReclaimAll();    // A
			spirit.Activate_DrawPowerCard(); // A
		}

		void Activate_B() {
			spirit.Activate_PlacePresence( "A3;A7;A8", spirit.Presence.Energy.Next ); // B
		}

		void Activate_C() {
			spirit.Activate_GainEnergy();                                             // C
			spirit.Activate_DrawPowerCard();                                          // C
		}

		void Activate_D() {
			spirit.Activate_GainEnergy();                                             // D
		}

	}

}
