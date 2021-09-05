using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.SinglePlayer;
using System;
using System.Linq;
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
			Activate_A();
			Activate_B();

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
			Activate_A();
			Activate_C();

			Assert_AllCardsAvailableToPlay();      // A
			Assert_HasEnergy( 2 + 2 );             // A & C
			Assert_BoardPresenceIs( "A3A3" );      // C
		}


		[Fact]
		public void A_Reclaim_Energy_D_Presence_PowerCard_LoseEnergy() {
			// a) reclaim, +1 energy
			// d) -3 energy, +1 power card, add presense to land without blight range 3

			// Given: presence on board A  (default island is Board A)
			Given_HalfOfPowercardsPlayed();
			Given_HasPresence( board[3] );
			Given_BlightEverywhereExcept7();

			When_StartingGrowth();
			Activate_A();
			Activate_D();

			Assert_AllCardsAvailableToPlay( 4+1);     // A
			Assert_HasEnergy( 0 );                   // A & D
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
			Activate_B();
			Activate_C();

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
			spirit.Energy = 1; // so we can do this option

			When_StartingGrowth();
			Activate_B();
			Activate_D();

			Assert_HasPowerProgressionCard( 0); // B
			Assert_HasPowerProgressionCard( 1 ); // B
			Assert_HasCardAvailable( "Reaching Grasp" ); // D
			Assert_BoardPresenceIs( "A3A7" );     // D

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
			Activate_C();
			Activate_D();

			Assert_HasEnergy( startingEnergy + spirit.EnergyPerTurn - 2  );          // C & D
			Assert_HasPowerProgressionCard(0); // D

		}

		// !!! test that Keeper can't choose growth option when they insufficent energy

		[Fact]
		public void SacredSitesPushDahan() {
			// Given: space with 2 dahan
			var space = board[5];
			gameState.DahanAdjust(space,2);
			//   and presence on that space
			spirit.Presence.PlaceOn( space );

			// When: we place a presence on that space
			_ = spirit.Presence.PlaceFromBoard( spirit.Presence.Energy.Next, space, gameState );

			spirit.Action.AssertDecision( "Push D@2 to", "A4");
			spirit.Action.AssertDecision( "Push D@2 to", "A7" );

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
			Activate_A();
			Activate_B();


			//When_Growing( 1 ); // (a&b) - Reclaim-All, +1 Energy, Gain PowerCard
			//_ = new ResolveActions( spirit, gameState, Speed.Growth ).ActAsync();
			//spirit.Activate_DrawPowerCard();
			//spirit.Activate_GainEnergy();
			//spirit.Activate_ReclaimAll();

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
			gameState.Tokens[space].Adjust(BacTokens.Wilds,1);
		}

		void Activate_A() {
			spirit.Action.Choose( "ReclaimAll / GainEnergy(1)" );
			spirit.Activate_ReclaimAll();    // A
			spirit.Activate_GainEnergy();    // A
		}

		void Activate_B() {
			spirit.Action.Choose( "DrawPowerCard" );
			spirit.Activate_DrawPowerCard(); // B
		}

		void Activate_C() {
			spirit.Action.Choose( "GainEnergy(1) / PlacePresence(3,presence or wilds)" );
			spirit.Activate_GainEnergy(); // C
			spirit.Activate_PlacePresence( "A3;A8", spirit.Presence.Energy.Next ); // C
		}

		void Activate_D() {
			spirit.Action.Choose( "GainEnergy(-3) / DrawPowerCard / PlacePresence(3,no blight)" );
			spirit.Activate_GainEnergy();    // D
			spirit.Activate_DrawPowerCard(); // D
			spirit.Activate_PlacePresence( "A7", spirit.Presence.CardPlays.Next );
		}

	}

}
