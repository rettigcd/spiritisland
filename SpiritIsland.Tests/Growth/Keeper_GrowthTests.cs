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

			// default island is Board A

			// Given: presence
			spirit.Presence.Add( board[3] );
			// Gvien: 1 wilds, 3 away
			gameState.AddWilds( board[8] );

			When_Growing( 1, new SpyOnPlacePresence("A3;A8") );

			Assert_AllCardsAvailableToPlay();   // A
			Assert_GainEnergy( 2 );             // A & C
			Assert_BoardPresenceIs("A3A3");     // C
		}

		[Fact]
		public void A_Reclaim_Energy_D_Presence_PowerCard_LoseEnergy(){
			// a) reclaim, +1 energy
			// d) -3 energy, +1 power card, add presense to land without blight range 3

			// Given: presence on board A  (default island is Board A)
			spirit.Presence.Add( board[3] );
			Given_BlightEverywhereExcept7();

			When_Growing( 2, new SpyOnPlacePresence("A7") );

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
			spirit.Presence.Add( board[3] );
			// Given: 1 wilds, 3 away
			gameState.AddWilds( board[8] );

			When_Growing( 3, new SpyOnPlacePresence("A3;A8") );

			Assert_GainPowercard( 1 );          // B
			Assert_GainEnergy( 1 );             // C
			Assert_BoardPresenceIs("A3A3");     // C
		}

		[Fact]
		public void B_Powercard_D_Presence_PowerCard_LoseEnergy() {
			// b) +1 power card
			// d) -3 energy, +1 power card, add presense to land without blight range 3

			// Given: presence on board A  (default island is Board A)
			spirit.Presence.Add( board[3] );
			Given_BlightEverywhereExcept7();

			When_Growing( 4, new SpyOnPlacePresence( "A7" ) );

			Assert_GainPowercard( 2 );          // B & D
			Assert_GainEnergy( -3 );            // D
			Assert_BoardPresenceIs( "A3A7" );     // D

		}

		[Fact]
		public void C_Presence_Energy_D_Presence_PowerCard_LoseEnergy() {
			// c) add presense range 3 containing (wilds or presense), +1 energy
			// d) -3 energy, +1 power card, add presense to land without blight range 3

			// Given: presence on board A  (default island is Board A)
			spirit.Presence.Add( board[3] );
			gameState.AddWilds( board[8] );
			Given_BlightEverywhereExcept7();

			When_Growing( 5, new SpyOnPlacePresence( "A3A7;A7A8" ) );

			Assert_GainEnergy( -2 );            // C & D
			Assert_GainPowercard( 1 );          // D
			Assert_BoardPresenceIs( "A3A3A7" ); // D

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
