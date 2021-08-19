using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland.SinglePlayer;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class MassiveFlooding_Tests : RiverGame {


		readonly Spirit spirit;
		readonly GameState gs;

		public MassiveFlooding_Tests(){
			// Given: River
			spirit = new RiverSurges().UsePowerProgression();
			gs = new GameState( spirit ) {
				Island = new Island( Board.BuildBoardA() ),
				InvaderDeck = InvaderDeck.Unshuffled()
			};
			game = new SinglePlayerGame( gs );

		}

		// Not enought elements -> nothing
		[Fact]
		public void InsufficientElements() {

			Game_SelectGrowthOption( 0 ); // reclaim
			Game_SelectPowerCardsDone();

			//   And: in slow phase

			//  Then: no massive flooding in Unresolved list
			Assert.Empty( game.Spirit.GetUnresolvedActionFactories(Speed.Slow) );

		}

		[Fact]
		public void Level1_Pushes1TownOrExplorer() { // 1-Sun, 2-Water

			Game_SelectGrowthOption( 1 ); // PP-1 PP-1

			Game_PlacePresence1("Energy","A4");
			Game_PlacePresence1("Card","A2");

			Game_BuyPowerCards( FlashFloods.Name ); // fast
			Game_BuyPowerCards( RiversBounty.Name );// slow

			Game_DoneWith( Speed.Fast );

			game.Decision.AssertDecision( "Select Slow to resolve:", "River's Bounty,Massive Flooding,Done", "Massive Flooding" );
			game.Decision.AssertDecision( "Select space to target.", "A2,A3,A5,A8", "A8" );
			game.Decision.AssertDecision( "Select invader to push", "T@2,E@1,Done", "T@2" );
			game.Decision.AssertDecision( "Push T@2 to", "A5,A6,A7", "A5" );
		}

		[Fact]
		public void Level2_2DamagePush3TownOrExplorers() { // 2-Sun, 3-Water

			Given_SpiritCardPlayCount( 3 );
			Given_SpiritGetMoney( 5 );

			Game_SelectGrowthOption( 0 ); // Reclaim

			Game_BuyPowerCards( FlashFloods.Name ); // fast - sun, water
			Game_BuyPowerCards( RiversBounty.Name ); // slow - sun, water, animal
			Game_BuyPowerCards( BoonOfVigor.Name ); // fast - sun, water, plant

			Game_DoneWith( Speed.Fast );

			game.Decision.AssertDecision( "Select Slow to resolve:", "River's Bounty,Massive Flooding,Done", "Massive Flooding" );
			game.Decision.AssertDecision( "Select space to target.", "A5,A8", "A8" );
			game.Decision.AssertDecision( "Select invader to push", "E@1,Done", "E@1" );
			game.Decision.AssertDecision( "Push E@1 to", "A5,A6,A7", "A5" );
		}

		[Fact]
		public void Level3_2DamageToEachInvader() { // 3 sun, 4 water, 1 earth
			// Instead, 2 damage to each invader

			Given_SpiritCardPlayCount( 4 );
			Given_SpiritGetMoney( 5 );

			// 1 city, 5 towns, 5 invaders on A4 (range 1 from SS)
			var space = game.Spirit.SacredSites.First().Adjacent.First();
			game.GameState.Adjust(space,InvaderSpecific.City,1);
			game.GameState.Adjust( space, InvaderSpecific.Town, 5 );
			game.GameState.Adjust( space, InvaderSpecific.Explorer, 5 );

			Game_SelectGrowthOption( 0 ); // Reclaim

			Game_BuyPowerCards( FlashFloods.Name );  // fast - sun, water
			Game_BuyPowerCards( RiversBounty.Name ); // slow - sun, water, animal
			Game_BuyPowerCards( BoonOfVigor.Name );  // fast - sun, water, plant
			Game_BuyPowerCards( WashAway.Name );     // slow -      water, earth

			Game_DoneWith( Speed.Fast );

			Game_SelectOption( "Select Slow to resolve", "Massive Flooding" );
			Game_SelectOption( "Select space to target.", space.Label);
			
			Assert.Equal("1C@1",game.GameState.InvadersOn(space).ToString());
		}


		void Given_SpiritCardPlayCount( int target ) {
			while(game.Spirit.NumberOfCardsPlayablePerTurn < target)
				game.Spirit.Presence.CardPlays.RevealedCount++;
		}

		void Given_SpiritGetMoney( int target ) {
			while(game.Spirit.EnergyPerTurn < target)
				game.Spirit.Presence.Energy.RevealedCount++;
		}

	}

}
