using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using Xunit;

namespace SpiritIsland.Tests.BranchAndClaw.Spirits {

	public class Keeper_GrowthTests : GrowthTests {

		static Spirit InitSpirit() {
			return new Keeper {
				CardDrawer = new PowerProgression(
					PowerCard.For<VeilTheNightsHunt>(),
					PowerCard.For<ReachingGrasp>()
				),
			};
		}

		readonly GameState_BranchAndClaw gsbac;

		public Keeper_GrowthTests() : base( InitSpirit() ) {
			gsbac = new GameState_BranchAndClaw( spirit, board );
			gameState = gsbac;
		}

		// a) reclaim, +1 energy
		// b) +1 power card
		// c) add presense range 3 containing wilds or presence, +1 energy
		// d) -3 energy, +1 power card, add presense to land without blight range 3

		[Fact]
		public void A_Reclaim_Energy_B_Powercard() {
			// a) reclaim, +1 energy
			// b) +1 power card
			Given_HalfOfPowercardsPlayed();

			When_StartingGrowth();
			User_Activates_A();
			User_Activates_B();

			Assert_AllCardsAvailableToPlay( 1 + 4 );
			Assert_HasEnergy( 1 + 2 );
			Assert_HasPowerProgressionCard( 0 );
		}

		[Fact]
		public void A_Reclaim_Energy_C_Presence_Energy() {
			// a) reclaim, +1 energy
			// c) add presense range 3 containing (wilds or presense), +1 energy

			Given_HasPresence( board[3] );
			Given_HalfOfPowercardsPlayed();
			Given_HasWilds( board[8] ); // 3 spaces away

			When_StartingGrowth();
			User_Activates_A();
			User_Activates_C();

			Assert_AllCardsAvailableToPlay();      // A
			Assert_HasEnergy( 2 + 2 );             // A & C
			Assert_BoardPresenceIs( "A3A3" );      // C
		}


		[Fact]
		public void A_Reclaim_Energy_D_Presence_PowerCard_LoseEnergy() {
			spirit.Energy = 10; // so we can -3 it
			// a) reclaim, +1 energy
			// d) -3 energy, +1 power card, add presense to land without blight range 3

			// Given: presence on board A  (default island is Board A)
			Given_HalfOfPowercardsPlayed();
			Given_HasPresence( board[3] );
			Given_BlightEverywhereExcept7();

			When_StartingGrowth();
			User_Activates_A();
			User_Activates_D();

			Assert_AllCardsAvailableToPlay( 4+1);     // A
			Assert_HasEnergy( 10 + 2-3+1 );                // A & D
			Assert_HasPowerProgressionCard(0); // D
			Assert_BoardPresenceIs( "A3A7" );        // D

		}

		[Fact]
		public void B_Powercard_C_Presence_Energy() {
			// b) +1 power card
			// c) add presense range 3 containing (wilds or presense), +1 energy

			// Given: presence at A3  (default island is Board A)
			Given_HasPresence( board[3] );
			// Given: 1 wilds, 3 away
			Given_HasWilds( board[8] );

			When_StartingGrowth();
			User_Activates_B();
			User_Activates_C();

			Assert_HasPowerProgressionCard( 0); // B
			Assert_HasEnergy( 1 + 2 );             // C
			Assert_BoardPresenceIs( "A3A3" );     // C
		}

		[Fact]
		public void B_Powercard_D_Presence_PowerCard_LoseEnergy() {
			// b) +1 power card
			// d) -3 energy, +1 power card, add presense to land without blight range 3

			// Given: presence on board A  (default island is Board A)
			Given_HasPresence( board[3] );
			Given_BlightEverywhereExcept7();
			spirit.Energy = 10; // so we can do this option

			When_StartingGrowth();
			User_Activates_B();
			User_Activates_D();

			Assert_HasPowerProgressionCard( 0); // B
			Assert_HasPowerProgressionCard( 1 ); // B
			Assert_HasCardAvailable( "Reaching Grasp" ); // D
			Assert_BoardPresenceIs( "A3A7" );     // D
			Assert_HasEnergy( 10 + 2 - 3 );
		}

		[Fact]
		public void C_Presence_Energy_D_Presence_PowerCard_LoseEnergy() {
			const int startingEnergy = 10;
			spirit.Energy = startingEnergy;
			// c) add presense range 3 containing (wilds or presense), +1 energy
			// d) -3 energy, +1 power card, add presense to land without blight range 3

			// Given: presence on board A  (default island is Board A)
			Given_HasPresence( board[3] );
			Given_HasWilds( board[8] );
			Given_BlightEverywhereExcept7();

			When_StartingGrowth();
			User_Activates_C();
			User_Activates_D();

			Assert_HasEnergy( startingEnergy + spirit.EnergyPerTurn - 2  );          // C & D
			Assert_HasPowerProgressionCard(0); // D

		}

		[Fact]
		public void SacredSitesPushDahan() {
			// Given: space with 2 dahan
			var space = board[5];
			gameState.DahanAdjust(space,2);
			//   and presence on that space
			spirit.Presence.PlaceOn( space );

			// When: we place a presence on that space
			_ = spirit.Presence.PlaceFromBoard( spirit.Presence.Energy.Next, space, gameState );

			User.PushesTokensTo("D@2","A1,(A4),A6,A7,A8",2);
			User.PushesTokensTo("D@2","A1,A4,A6,(A7),A8");

			spirit.SacredSites.ShouldContain(space);
			gameState.DahanGetCount(space).ShouldBe(0,"SS should push dahan from space");
		}


		[Theory]
		[InlineDataAttribute( 1, 2, "" )]
		[InlineDataAttribute( 2, 2, "S" )]
		[InlineDataAttribute( 3, 4, "S" )]
		[InlineDataAttribute( 4, 5, "S" )]
		[InlineDataAttribute( 5, 5, "SP" )]
		[InlineDataAttribute( 6, 7, "SP" )]
		[InlineDataAttribute( 7, 8, "SP" )]
		[InlineDataAttribute( 8, 9, "SP" )]
		public void EnergyTrack( int revealedSpaces, int expectedEnergyGrowth, string elements ) {
			// energy:	2 sun 4 5 plant 7 8 9
			spirit.Presence.Energy.RevealedCount = revealedSpaces;

			When_StartingGrowth();
			User_Activates_A();
			User_Activates_B();

			Assert_EnergyTrackIs( expectedEnergyGrowth );
			Assert_BonusElements( elements );
		}

		[Theory]
		[InlineDataAttribute( 1, 1 )]
		[InlineDataAttribute( 2, 2 )]
		[InlineDataAttribute( 3, 2 )]
		[InlineDataAttribute( 4, 3 )]
		[InlineDataAttribute( 5, 4 )]
		[InlineDataAttribute( 6, 5 )]
		public void CardTrack( int revealedSpaces, int expectedCardPlayCount ) {
			// card:	1 2 2 3 4 5
			spirit.Presence.CardPlays.RevealedCount = revealedSpaces;
			Assert_CardTrackIs( expectedCardPlayCount );
		}


		void Given_BlightEverywhereExcept7() {
			gameState.AddBlight( board[1] );
			gameState.AddBlight( board[2] );
			gameState.AddBlight( board[3] );
			gameState.AddBlight( board[4] );
			gameState.AddBlight( board[5] );
			gameState.AddBlight( board[6] );
			gameState.AddBlight( board[8] );
			gameState.GetBlightOnSpace( board[7] ).ShouldBe( 0 );
		}

		void Given_HasWilds( Space space ) {
			gameState.Tokens[space].Wilds().Count++;
		}

		void User_Activates_A() {
			User.SelectsGrowthOption( "ReclaimAll / GainEnergy(1)" );
			User.ReclaimsAll();
			User.GainsEnergy();
		}

		void User_Activates_B() {
			User.SelectsGrowthOption( "DrawPowerCard" );
			User.DrawsPowerCard();
		}

		void User_Activates_C() {
			User.SelectsGrowthOption( "GainEnergy(1) / PlacePresence(3,presence or wilds)" );
			User.GainsEnergy();
			User.PlacesPresence( "A3;A8", spirit.Presence.Energy.Next );
		}

		void User_Activates_D() {
			User.SelectsGrowthOption( "GainEnergy(-3) / DrawPowerCard / PlacePresence(3,no blight)" );
			User.GainsEnergy();
			User.DrawsPowerCard();
			User.PlacesPresence( "A7", spirit.Presence.CardPlays.Next );
		}

	}

}
