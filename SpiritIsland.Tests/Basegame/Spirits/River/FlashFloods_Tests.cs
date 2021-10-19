using Shouldly;
using SpiritIsland.Basegame;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class FlashFloods_Tests : SpiritCards_Tests {

		// immutable
		readonly PowerCard flashFloodsCard = PowerCard.For<FlashFloods>();

		public FlashFloods_Tests():base(new RiverSurges() ) { }

		[Fact]
		public void FlashFloods_Inland() {

			Given_GameWithSpirits( spirit );

			//   And: a game on Board-A
			var board = Board.BuildBoardA();
			gameState.Island = new Island( board );

			//   And: Presence on A2 (city/costal)
			var presenceSpace = board[2];
			spirit.Presence.PlaceOn( presenceSpace );
			//   And: 1 of each type of Invaders in Inland space (A4)
			Space targetSpace = board[4];
			var counts = gameState.Tokens[targetSpace];
			counts.Adjust( Invader.City.Default, 1 );
			counts.Adjust( Invader.Town.Default, 1 );
			counts.Adjust( Invader.Explorer.Default, 1 );
			gameState.Assert_Invaders( targetSpace, "1C@3,1T@2,1E@1" );

			//   And: Purchased FlashFloods
			card = spirit.Hand.Single( c => c.Name == FlashFloods.Name );
			spirit.Energy = card.Cost;
			spirit.PurchaseAvailableCards_Test( card );
			Assert.Contains( card, spirit.GetAvailableActions( card.Speed ).OfType<PowerCard>().ToList() ); // is fast

			When_PlayingCard();

			User.TargetsLand( "A4" );
			User.SelectsDamageRecipient( 1, "C@3,T@2,(E@1)" ); // select damage option

			User.Assert_Done();
			gameState.Assert_Invaders( targetSpace, "1C@3,1T@2" );
		}

		[Fact]
		public void FlashFloods_Costal() {
			// Given: River
			//   And: a game on Board-A
			var board = Board.BuildBoardA();
			gameState = new GameState(spirit,board);
			//   And: Presence on A2 (city/costal)
			var presenceSpace = board[2];
			spirit.Presence.PlaceOn(presenceSpace);
			//   And: 1 of each type of Invaders in Costal space (A2)
			Space targetSpace = board[2];
			var grp = gameState.Tokens[targetSpace];
			grp.Adjust(Invader.City.Default,1);
			grp.Adjust(Invader.Town.Default, 1);
			grp.Adjust(Invader.Explorer.Default, 1);
			gameState.Assert_Invaders(targetSpace, "1C@3,1T@2,1E@1" );

			//   And: Purchased FlashFloods
			card = spirit.Hand.Single(c=>c.Name == FlashFloods.Name);
			spirit.Energy = card.Cost;
			spirit.PurchaseAvailableCards_Test(card);
			Assert.Contains(card,spirit.GetAvailableActions(card.Speed).OfType<PowerCard>().ToList()); // is fast

			When_PlayingCard();

			//  Select: A2
			User.TargetsLand("A2");

			// Then: can apply 2 points of damage
			User.SelectsDamageRecipient( 2, "(C@3),T@2,E@1" );
			User.SelectsDamageRecipient( 1, "(C@2),T@2,E@1" );

			// And: apply doesn't throw an exception
			User.Assert_Done();
			gameState.Assert_Invaders(targetSpace, "1C@1,1T@2,1E@1" );
		}

		//[Fact(Skip = "not implemented")]
		//public void OnCoast_Damage2DifferntInvaders(){}

		//[Fact(Skip = "not implemented")]
		//public void CanDamage_AlreadyDamagedCities(){}


		[Fact]
		public void FlashFloods_Stats() {
			Assert_CardStatus( flashFloodsCard, 2, Speed.Fast, "SW" );
		}

	}

}



