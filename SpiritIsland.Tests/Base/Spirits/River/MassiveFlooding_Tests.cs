using SpiritIsland.Base;
using SpiritIsland.Core;
using SpiritIsland.SinglePlayer;
using Xunit;

namespace SpiritIsland.Tests.Base.Spirits.River {

	public class RiverGame : GameBaseTests {
		protected void Game_PlacePresence1( string sourceTrack, string destinationSpace ) {
			Game_SelectOption( "Select Growth to resolve", "PlacePresence(1)" );
			Game_SelectOption( "PlacePresence(1) - Select Presence to place", sourceTrack );
			Game_SelectOption( "PlacePresence(1) - Where would you like", destinationSpace );
		}

		protected void Game_Reclaim1( string cardToReclaim ) {
			Game_SelectOption( "Select Growth to resolve", "Reclaim(1)" );
			Game_SelectOption( "Reclaim(1) - Select card", cardToReclaim );
		}


	}

	public class MassiveFlooding_Tests : RiverGame {

		public MassiveFlooding_Tests(){
			// Given: River
			var gameState = new GameState( new RiverSurges() ) {
				Island = new Island( Board.BuildBoardA() )
			};
			gameState.InvaderDeck = InvaderDeck.Unshuffled();
			game = new SinglePlayerGame( gameState );

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

			Game_SelectOption( "Select Slow to resolve", "Massive Flooding" );
			Game_SelectOption( "Massive Flooding - Select target", "A8" ); // always a town on A8
			Game_SelectOption( "Massive Flooding - Select invader to push", "T@2" );
			Game_SelectOption( "Massive Flooding - Select destination for T@2", "A5" );
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

			Game_SelectOption( "Select Slow to resolve", "Massive Flooding" );
			Game_SelectOption( "Massive Flooding - Select Innate option", MassiveFlooding.k2);
			Game_SelectOption( "Massive Flooding - Select target space", "A8");
			Game_SelectOption( "Massive Flooding - Select invader to push.", "E@1" ); // always a town on A8
			Game_SelectOption( "Massive Flooding - Select destination for E@1", "A5" ); // always a town on A8
		}

		[Fact]
		public void Level3_2DamageToEachInvader() { // 3 sun, 4 water, 1 earth
			// Instead, 2 damage to each invader

			Given_SpiritCardPlayCount( 4 );
			Given_SpiritGetMoney( 5 );

			Game_SelectGrowthOption( 0 ); // Reclaim

			Game_BuyPowerCards( FlashFloods.Name );  // fast - sun, water
			Game_BuyPowerCards( RiversBounty.Name ); // slow - sun, water, animal
			Game_BuyPowerCards( BoonOfVigor.Name );  // fast - sun, water, plant
			Game_BuyPowerCards( WashAway.Name );     // slow -      water, earth

			Game_DoneWith( Speed.Fast );

			Game_SelectOption( "Select Slow to resolve", "Massive Flooding" );
			Game_SelectOption( "Massive Flooding - Select Innate option", MassiveFlooding.k3);
			Game_SelectOption( "Massive Flooding - Select target space", "A8");
		}


		void Given_SpiritCardPlayCount( int target ) {
			while(game.Spirit.NumberOfCardsPlayablePerTurn < target)
				game.Spirit.RevealedCardSpaces++;
		}

		void Given_SpiritGetMoney( int target ) {
			while(game.Spirit.EnergyPerTurn < target)
				game.Spirit.RevealedEnergySpaces++;
		}


		// !!!!!!!!!   Rivers special rules -> presence in water is SS

	}

}
