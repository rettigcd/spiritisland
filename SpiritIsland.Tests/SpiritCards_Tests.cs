using SpiritIsland.PowerCards;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class SpiritCards_Tests {

		// immutable
		readonly PowerCard flashFloodsCard = PowerCard.For<FlashFloods>();

		FlashFloods FlashFloods => (FlashFloods)flashFloodsCard.Bind(null,null);

		#region BoonOfVigor

		[Fact]
		public void BoonOfVigor_TargetSelf() {

			// Given: River
			var spirit = new RiverSurges();
			//   And: a game
			var gameState = new GameState();
			//   And: Purchased Boon of Vigor
			var card = spirit.AvailableCards.Single(c=>c.Name == BoonOfVigor.Name);
			spirit.BuyAvailableCards(card);
			//   And: card is fast (ready to play now)
			Assert.Contains(card,spirit.UnresolvedActions.OfType<PowerCard>().ToList());

			// When: targetting self
			var action = (BoonOfVigor)card.Bind(spirit,gameState);
			Assert.False(action.IsResolved);
			action.Target = spirit;
			Assert.True(action.IsResolved);
			action.Apply();

			// Then: received 1 energy
			Assert.Equal(1,spirit.Energy);

		}

		[Theory]
		[InlineData( 0 )]
		[InlineData( 3 )]
		[InlineData( 10 )]
		public void BoonOfVigor_TargetOther( int expectedEnergyBonus ) {

			// Given: River
			var spirit = new RiverSurges();

			//   And: a second spirit
			var other = new Lightning();
			//  That: purchase N cards
			for(int i=0;i<expectedEnergyBonus;++i){
				var otherCard = new PowerCard("Fake-"+i,0,Speed.Slow);
				other.ActiveCards.Add(otherCard);
				other.UnresolvedActions.Add(otherCard);
			}

			//   And: a game
			var gameState = new GameState();

			//   And: Purchased Boon of Vigor
			var card = spirit.AvailableCards.Single(c=>c.Name == BoonOfVigor.Name);
			spirit.BuyAvailableCards(card);

			//   And: card is fast (ready to play now)
			Assert.Contains(card,spirit.UnresolvedActions.OfType<PowerCard>().ToList());

			// When: targetting other
			var action = (BoonOfVigor)card.Bind(spirit,gameState);
			Assert.False(action.IsResolved);
			action.Target = other;
			Assert.True(action.IsResolved);
			action.Apply();

			// Then: received 1 energy
			Assert.Equal(expectedEnergyBonus,other.Energy);

		}

		[Fact]
		public void BoonOfVigor_Stats() {
			AssertCardStatus( PowerCard.For<BoonOfVigor>(), 0, Speed.Fast, "SWP" );
		}

		#endregion BoonOfVigor

		#region FlashFloods

		[Fact]
		public void FlashFloods_Inland() {
			// Given: River
			var spirit = new RiverSurges();
			//   And: a game on Board-A
			var board = Board.BuildBoardA();
			var gameState = new GameState{ Island = new Island(board) };
			//   And: Presence on A2 (city/costal)
			var presenceSpace = board[2];
			spirit.Presence.Add(presenceSpace);
			//   And: 1 of each type of Invaders in Inland space (A4)
			Space targetSpace = board[4];
			gameState.AddCity(targetSpace);
			gameState.AddTown(targetSpace);
			gameState.AddExplorer(targetSpace);
			Assert.Equal("1C@3,1T@2,1E@1",gameState.GetInvaderSummary(targetSpace));

			//   And: Purchased FlashFloods
			var card = spirit.AvailableCards.Single(c=>c.Name == FlashFloods.Name);
			spirit.Energy = card.Cost;
			spirit.BuyAvailableCards(card);
			Assert.Contains(card,spirit.UnresolvedActions.OfType<PowerCard>().ToList()); // is fast

			//  When: activating flash flood
			var action = (FlashFloods)card.Bind(spirit,gameState);

			// Then: can target any land within 1 of presence.
			Assert.False(action.IsResolved);
			string actionOptions = action.GetOptions().Select(x=>x.Text).OrderBy(x=>x).Join(",");
			string expectedOptions = presenceSpace.SpacesWithin(1)
				.Where(s=>s.Terrain!=Terrain.Ocean)
				.Select(s=>s.Label)
				.OrderBy(x=>x)
				.Join(",");
			Assert.Equal(expectedOptions,actionOptions);

			// When: selecting 
			action.Select( targetSpace );

			// Then: can apply 1 points of damage
			Assert.False(action.IsResolved);
			var damageOptions = action.GetOptions();
			Assert.Equal("1>C@3,1>E@1,1>T@2",damageOptions.Select(x=>x.Text).OrderBy(x=>x).Join(","));

			// When: selecting a damage optin
			action.Select(damageOptions.Single(x=>x.Text=="1>E@1"));

			// Then: resolved
			Assert.True(action.IsResolved);

			// And: apply doesn't throw an exception
			action.Apply();
			Assert.Equal("1C@3,1T@2",gameState.GetInvaderSummary(targetSpace));
		}

		[Fact]
		public void FlashFloods_Costal() {
			// Given: River
			var spirit = new RiverSurges();
			//   And: a game on Board-A
			var board = Board.BuildBoardA();
			var gameState = new GameState{ Island = new Island(board) };
			//   And: Presence on A2 (city/costal)
			var presenceSpace = board[4];
			spirit.Presence.Add(presenceSpace);
			//   And: 1 of each type of Invaders in Costal space (A2)
			Space targetSpace = board[2];
			gameState.AddCity(targetSpace);
			gameState.AddTown(targetSpace);
			gameState.AddExplorer(targetSpace);
			Assert.Equal("1C@3,1T@2,1E@1",gameState.GetInvaderSummary(targetSpace));

			//   And: Purchased FlashFloods
			var card = spirit.AvailableCards.Single(c=>c.Name == FlashFloods.Name);
			spirit.Energy = card.Cost;
			spirit.BuyAvailableCards(card);
			Assert.Contains(card,spirit.UnresolvedActions.OfType<PowerCard>().ToList()); // is fast

			//  When: activating flash flood
			var action = (FlashFloods)card.Bind(spirit,gameState);

			// Then: can target any land within 1 of presence.
			Assert.False(action.IsResolved);
			string actionOptions = action.GetOptions().Select(x=>x.Text).OrderBy(x=>x).Join(",");
			string expectedOptions = presenceSpace.SpacesWithin(1)
				.Where(s=>s.Terrain!=Terrain.Ocean)
				.Select(s=>s.Label)
				.OrderBy(x=>x)
				.Join(",");
			Assert.Equal(expectedOptions,actionOptions);

			// When: selecting 
			action.Select( targetSpace );

			// Then: can apply 2 points of damage
			Assert.False(action.IsResolved);
			var damageOptions = action.GetOptions();
			Assert.Equal("1>C@3,1>E@1,1>T@2,2>C@3,2>T@2",damageOptions.Select(x=>x.Text).OrderBy(x=>x).Join(","));

			// When: selecting a damage optin
			action.Select(damageOptions.Single(x=>x.Text=="2>C@3"));

			// Then: resolved
			Assert.True(action.IsResolved);

			// And: apply doesn't throw an exception
			action.Apply();
			Assert.Equal("1C@1,1T@2,1E@1",gameState.GetInvaderSummary(targetSpace));
		}

		[Fact]
		public void FlashFloods_Stats() {
			AssertCardStatus( flashFloodsCard, 2, Speed.Fast, "SW" );
		}

		#endregion FlashFloods


		[Fact]
		public void RiversBounty_Stats() {
			var card = PowerCard.For<RiversBounty>();
			AssertCardStatus( card, 0, Speed.Slow, "SWB" );
		}


		void AssertCardStatus( PowerCard card, int expectedCost, Speed expectedSpeed, string expectedElements ) {
			Assert.Equal( expectedCost, card.Cost );
			Assert.Equal( expectedSpeed, card.Speed );

			var cardElements = card.Elements
				.Select(x=> Growth.GrowthTests.ElementChars[x]);
			Assert.Equal( expectedElements, string.Join("",cardElements));

		}

	}

}



