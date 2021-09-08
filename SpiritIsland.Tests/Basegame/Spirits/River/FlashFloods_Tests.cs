using Shouldly;
using SpiritIsland.Basegame;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class FlashFloods_Tests : SpiritCards_Tests {

		// immutable
		readonly PowerCard flashFloodsCard = PowerCard.For<FlashFloods>();

		[Fact]
		public void FlashFloods_Inland() {

			Given_GameWithSpirits( new RiverSurges() );

			//   And: a game on Board-A
			var board = Board.BuildBoardA();
			gameState.Island = new Island(board);

			//   And: Presence on A2 (city/costal)
			var presenceSpace = board[2];
			spirit.Presence.PlaceOn(presenceSpace);
			//   And: 1 of each type of Invaders in Inland space (A4)
			Space targetSpace = board[4];
			var counts = gameState.Tokens[targetSpace];
			counts.Adjust(Invader.City.Default,1);
			counts.Adjust(Invader.Town.Default,1);
			counts.Adjust(Invader.Explorer.Default,1);
			gameState.Assert_Invaders(targetSpace, "1C@3,1T@2,1E@1" );

			//   And: Purchased FlashFloods
			var card = spirit.Hand.Single(c=>c.Name == FlashFloods.Name);
			spirit.Energy = card.Cost;
			spirit.PurchaseAvailableCards(card);
			Assert.Contains(card,spirit.GetAvailableActions(card.Speed).OfType<PowerCard>().ToList()); // is fast

			//  When: activating flash flood
			card.ActivateAsync( spirit, gameState );
			action = spirit.Action;

			// Then: Auto selecting only target space avaialbe

			// When: selecting a damage optin
			Assert.False(action.IsResolved);
			Assert_Options( "C@3","E@1","T@2" );
			action.Choose( "E@1" );

			// Then: resolved => Applu
			Assert.True(action.IsResolved);
			gameState.Assert_Invaders(targetSpace, "1C@3,1T@2" );
		}

		[Fact]
		public void FlashFloods_Costal() {
			// Given: River
			var spirit = new RiverSurges();
			//   And: a game on Board-A
			var board = Board.BuildBoardA();
			var gameState = new GameState(spirit,board);
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
			var card = spirit.Hand.Single(c=>c.Name == FlashFloods.Name);
			spirit.Energy = card.Cost;
			spirit.PurchaseAvailableCards(card);
			Assert.Contains(card,spirit.GetAvailableActions(card.Speed).OfType<PowerCard>().ToList()); // is fast

			//  When: activating flash flood
			card.ActivateAsync( spirit, gameState );
			action = spirit.Action;

			// Then: can apply 2 points of damage
			action.IsResolved.ShouldBeFalse();
			Assert_Options("C@3","E@1","T@2");
			action.Choose( "C@3" );

			// And: apply doesn't throw an exception
			action.IsResolved.ShouldBeTrue();
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



