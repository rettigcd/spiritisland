using Xunit;

namespace SpiritIsland.Tests.Growth {

	public class SharpFangs_GrowthTests : GrowthTests {

		public SharpFangs_GrowthTests():base( new SharpFangs() ){
		
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

			When_Growing( 0, Resolve_PlacePresence("A3;A7;A8") );

			Assert_AllCardsAvailableToPlay();  // A
			Assert_GainEnergy( -1 );           // A
			Assert_GainPowercard( 1 );         // A

			Assert_BoardPresenceIs("A2A3");    // B
		}

		[Fact]
		public void AC(){
			// a) cost -1, reclaim cards, gain +1 power card
			// c) gain power card, gain +1 energy

			When_Growing( 1 );

			Assert_AllCardsAvailableToPlay();  // A
			Assert_GainEnergy( 0 );            // A & C
			Assert_GainPowercard( 2 );         // A & C

		}

		[Fact]
		public void AD(){
			// a) cost -1, reclaim cards, gain +1 power card
			// d) +3 energy

			When_Growing( 2 );

			Assert_AllCardsAvailableToPlay();  // A
			Assert_GainPowercard( 1 );         // A
			Assert_GainEnergy( 2 );            // A & D

		}

		[Fact]
		public void BC(){
			// b) add a presense to jungle or a land with beasts ( range 3)
			// c) gain power card, gain +1 energy

			When_Growing( 3, Resolve_PlacePresence("A3;A7;A8") );

			Assert_BoardPresenceIs("A2A3");    // B
			Assert_GainEnergy( 1 );            // C
			Assert_GainPowercard( 1 );         // C
		}

		[Fact]
		public void BD(){
			// b) add a presense to jungle or a land with beasts ( range 3)
			// d) +3 energy

			When_Growing( 4, Resolve_PlacePresence("A3;A7;A8") );

			Assert_BoardPresenceIs("A2A3");    // B
			Assert_GainEnergy( 3 );            // D
		}

		[Fact]
		public void CD(){
			// c) gain power card, gain +1 energy
			// d) +3 energy

			When_Growing( 5 );

			Assert_GainPowercard( 1 );         // C
			Assert_GainEnergy( 1+3 );          // C + D
		}


	}

}
