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
			gameState.Phase = Phase.Fast;


			//   And: a game on Board-A
			var board = Board.BuildBoardA();
			gameState.Island = new Island( board );

			//   And: Presence on A2 (city/costal)
			var presenceSpace = board[2];
			spirit.Presence.PlaceOn( presenceSpace, gameState );
			//   And: 1 of each type of Invaders in Inland space (A4)
			Space targetSpace = board[4];
			var counts = gameState.Tokens[targetSpace];
			counts.AdjustDefault( Invader.City, 1 );
			counts.AdjustDefault( Invader.Town, 1 );
			counts.AdjustDefault( Invader.Explorer, 1 );
			gameState.Assert_Invaders( targetSpace, "1C@3,1T@2,1E@1" );

			//   And: Purchased FlashFloods
			card = spirit.Hand.Single( c => c.Name == FlashFloods.Name );
			spirit.Energy = card.Cost;
			PlayCard();
			Assert.Contains( card, spirit.GetAvailableActions( card.DisplaySpeed ).OfType<PowerCard>().ToList() ); // is fast

			When_PlayingCard();

			User.TargetsLand( FlashFloods.Name, "A4" );
			User.SelectsDamageRecipient( 1, "C@3,T@2,(E@1)" ); // select damage option

			User.Assert_Done();
			gameState.Assert_Invaders( targetSpace, "1C@3,1T@2" );
		}

		[Fact]
		public void FlashFloods_Costal() {
			// Given: River
			//   And: a game on Board-A
			var board = Board.BuildBoardA();
			gameState = new GameState( spirit, board ) {
				Phase = Phase.Fast
			};
			//   And: Presence on A2 (city/costal)
			var presenceSpace = board[2];
			spirit.Presence.PlaceOn(presenceSpace, gameState);
			//   And: 1 of each type of Invaders in Costal space (A2)
			Space targetSpace = board[2];
			var grp = gameState.Tokens[targetSpace];
			grp.AdjustDefault(Invader.City,1);
			grp.AdjustDefault(Invader.Town, 1);
			grp.AdjustDefault(Invader.Explorer, 1);
			gameState.Assert_Invaders(targetSpace, "1C@3,1T@2,1E@1" );

			//   And: Purchased FlashFloods
			card = spirit.Hand.Single(c=>c.Name == FlashFloods.Name);
			spirit.Energy = card.Cost;
			PlayCard();
			Assert.Contains(card,spirit.GetAvailableActions(card.DisplaySpeed).OfType<PowerCard>().ToList()); // is fast

			When_PlayingCard();

			//  Select: A2
			User.TargetsLand(FlashFloods.Name,"A2");

			// Then: can apply 2 points of damage
			User.SelectsDamageRecipient( 2, "(C@3),T@2,E@1" );
			User.SelectsDamageRecipient( 1, "(C@2),T@2,E@1" );

			// And: apply doesn't throw an exception
			User.Assert_Done();
			gameState.Assert_Invaders(targetSpace, "1C@1,1T@2,1E@1" );
		}

		[Fact]
		public void FlashFloods_Stats() {
			Assert_CardStatus( flashFloodsCard, 2, Phase.Fast, "SW" );
		}

	}

}



