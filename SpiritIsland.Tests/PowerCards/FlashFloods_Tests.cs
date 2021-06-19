using SpiritIsland.PowerCards;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {
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
			spirit.Presence.Add(presenceSpace);
			//   And: 1 of each type of Invaders in Inland space (A4)
			Space targetSpace = board[4];
			gameState.Adjust(Invader.City,targetSpace,1);
			gameState.Adjust(Invader.Town,targetSpace,1);
			gameState.Adjust(Invader.Explorer,targetSpace,1);
			Assert.Equal("1C@3,1T@2,1E@1",gameState.GetInvaderGroup(targetSpace).ToString());

			//   And: Purchased FlashFloods
			var card = spirit.AvailableCards.Single(c=>c.Name == FlashFloods.Name);
			spirit.Energy = card.Cost;
			spirit.BuyAvailableCards(card);
			Assert.Contains(card,spirit.UnresolvedActions.OfType<PowerCard>().ToList()); // is fast

			//  When: activating flash flood
			var action = (FlashFloods)card.Bind(spirit,gameState);

			// Then: can target any land within 1 of presence.
			Assert.False(action.IsResolved);
			string actionOptions = action.Options.Select(x=>x.Text).OrderBy(x=>x).Join(",");
			string expectedOptions = presenceSpace.SpacesWithin(1)
				.Where(s=>s.IsLand)
				.Select(s=>s.Label)
				.OrderBy(x=>x)
				.Join(",");
			Assert.Equal(expectedOptions,actionOptions);

			// When: selecting 
			action.Select( targetSpace );

			// Then: can apply 1 points of damage
			Assert.False(action.IsResolved);
			var damageOptions = action.Options;
			Assert.Equal("1>C@3,1>E@1,1>T@2",damageOptions.Select(x=>x.Text).OrderBy(x=>x).Join(","));

			// When: selecting a damage optin
			action.Select( "1>E@1" );

			// Then: resolved
			Assert.True(action.IsResolved);

			// And: apply doesn't throw an exception
			action.Apply();
			Assert.Equal("1C@3,1T@2",gameState.GetInvaderGroup(targetSpace).ToString());
		}

		[Fact]
		public void FlashFloods_Costal() {
			// Given: River
			var spirit = new RiverSurges();
			//   And: a game on Board-A
			var board = Board.BuildBoardA();
			var gameState = new GameState(spirit){ Island = new Island(board) };
			//   And: Presence on A2 (city/costal)
			var presenceSpace = board[4];
			spirit.Presence.Add(presenceSpace);
			//   And: 1 of each type of Invaders in Costal space (A2)
			Space targetSpace = board[2];
			gameState.Adjust(Invader.City,targetSpace,1);
			gameState.Adjust(Invader.Town,targetSpace,1);
			gameState.Adjust(Invader.Explorer,targetSpace,1);
			Assert.Equal("1C@3,1T@2,1E@1",gameState.GetInvaderGroup(targetSpace).ToString());

			//   And: Purchased FlashFloods
			var card = spirit.AvailableCards.Single(c=>c.Name == FlashFloods.Name);
			spirit.Energy = card.Cost;
			spirit.BuyAvailableCards(card);
			Assert.Contains(card,spirit.UnresolvedActions.OfType<PowerCard>().ToList()); // is fast

			//  When: activating flash flood
			var action = (FlashFloods)card.Bind(spirit,gameState);

			// Then: can target any land within 1 of presence.
			Assert.False(action.IsResolved);
			Assert_Options(action,presenceSpace.SpacesWithin(1).Where(s=>s.IsLand));
			action.Select( targetSpace );

			// Then: can apply 2 points of damage
			Assert.False(action.IsResolved);
			Assert_Options(action,"1>C@3","1>E@1","1>T@2","2>C@3","2>T@2");
			action.Select( "2>C@3" );


			// And: apply doesn't throw an exception
			Assert.True(action.IsResolved);
			action.Apply();
			Assert.Equal("1C@1,1T@2,1E@1",gameState.GetInvaderGroup(targetSpace).ToString());
		}

		[Fact(Skip = "not implemented")]
		public void OnCoast_Damage2DifferntInvaders(){

		}

		[Fact(Skip = "not implemented")]
		public void CanDamage_AlreadyDamagedCities(){

		}


		[Fact]
		public void FlashFloods_Stats() {
			Assert_CardStatus( flashFloodsCard, 2, Speed.Fast, "SW" );
		}

	}

}



