using SpiritIsland.PowerCards;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class SpiritCards_Tests {

		// immutable
		readonly PowerCard flashFloodsCard = PowerCard.For<FlashFloods>();

		Spirit spirit;
		GameState gameState;
		IAction action;

		#region BoonOfVigor

		[Fact]
		public void BoonOfVigor_TargetSelf() {

			Given_GameWithSpirits( new RiverSurges() );

			var card = Given_PurchasedCard(BoonOfVigor.Name);
			Assert_CardIsReady(card);

			// When: targetting self
			action = (BoonOfVigor)card.Bind(spirit, gameState);
			Assert.False(action.IsResolved);
			Assert.Equal(spirit, action.GetOptions().Single());
			action.Select(spirit);
			Assert.True(action.IsResolved);
			action.Apply();

			// Then: received 1 energy
			Assert.Equal(1, spirit.Energy);

		}

		[Theory]
		[InlineData( 0 )]
		[InlineData( 3 )]
		[InlineData( 10 )]
		public void BoonOfVigor_TargetOther( int expectedEnergyBonus ) {

			Given_GameWithSpirits(new RiverSurges(), new Lightning());

			//  That: purchase N cards
			var otherSpirit = gameState.Spirits[1];
			Given_PurchasedFakePowercards(otherSpirit, expectedEnergyBonus);

			//   And: Purchased Boon of Vigor
			PowerCard card = Given_PurchasedCard(BoonOfVigor.Name);
			Assert_CardIsReady(card);

			// When: targetting other spirit
			action = card.Bind(spirit, gameState);
			When_TargettingSpirit( otherSpirit );

			Assert.True(action.IsResolved);
			action.Apply();

			// Then: received 1 energy
			Assert.Equal(expectedEnergyBonus, otherSpirit.Energy);

		}

		void When_TargettingSpirit(Spirit otherSpirit) {
			Assert.False(action.IsResolved);
			Assert.Equal(gameState.Spirits.Select(x => x.Text).OrderBy(x => x).Join(",")
				, action.GetOptions().Select(x => x.Text).OrderBy(x => x).Join(",")
			);
			action.Select(otherSpirit);
		}

		[Fact]
		public void BoonOfVigor_Stats() {
			AssertCardStatus( PowerCard.For<BoonOfVigor>(), 0, Speed.Fast, "SWP" );
		}

		#endregion BoonOfVigor

		#region FlashFloods

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
			gameState.AddCity(targetSpace);
			gameState.AddTown(targetSpace);
			gameState.AddExplorer(targetSpace);
			Assert.Equal("1C@3,1T@2,1E@1",gameState.GetInvaderSummary(targetSpace).ToString());

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
			Assert.Equal("1C@3,1T@2",gameState.GetInvaderSummary(targetSpace).ToString());
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
			gameState.AddCity(targetSpace);
			gameState.AddTown(targetSpace);
			gameState.AddExplorer(targetSpace);
			Assert.Equal("1C@3,1T@2,1E@1",gameState.GetInvaderSummary(targetSpace).ToString());

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
			Assert.Equal("1C@1,1T@2,1E@1",gameState.GetInvaderSummary(targetSpace).ToString());
		}

		[Fact]
		public void FlashFloods_Stats() {
			AssertCardStatus( flashFloodsCard, 2, Speed.Fast, "SW" );
		}

		#endregion FlashFloods

		#region Wash Away

		[Fact]
		public void WashAway_Nothing() {
			PowerCard card = Given_RiverPlayingWashAway();

			// no explorers

			//  When: activating card
			var action = card.Bind(spirit, gameState);

			//  Then: card has 0 options
			Assert.Empty(action.GetOptions());

			Assert.True(action.IsResolved);

			action.Apply();
			// !!! test that nothing changes
		}

		[Theory]
		[InlineData(1,0,0,"","1E@1")]
		[InlineData(2,0,0,"1E@1","1E@1")]
		[InlineData(0,1,0,"","1T@2")]
		[InlineData(0,2,0,"1T@2","1T@2")]
		[InlineData(1,0,1,"1C@3","1E@1")]
		public void WashAway_1Target1PushableType(int explorerCount, int townCount, int cityCount, string expectedTargetResult, string expectedDestinationResult) {
			PowerCard card = Given_RiverPlayingWashAway();

			// 1 explorer on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			while(0<explorerCount--) gameState.AddExplorer(targetSpace);
			while(0<townCount--) gameState.AddTown(targetSpace);
			while(0<cityCount--) gameState.AddCity(targetSpace);
//			Assert.Equal("1E@1", gameState.GetInvaderSummary(targetSpace).ToString());

			//  When: activating card
			var action = card.Bind(spirit, gameState);

			//  Then: card has options of where to push 1 explorer
			Assert.Equal(
				targetSpace.SpacesExactly(1).Select(s=>s.Label).OrderBy(x=>x).Join(",")
				,action.GetOptions().Select(s=>s.Text).OrderBy(x=>x).Join(",")
			);
			var invaderDestination = board[2];
			action.Select(action.GetOptions().Single(x => x.Text == invaderDestination.Label));
			Assert.True(action.IsResolved);

			// And: apply doesn't throw an exception
			action.Apply();

			// !!! check that explore was moved
			Assert.Equal(expectedTargetResult, gameState.GetInvaderSummary(targetSpace).ToString());
			Assert.Equal(expectedDestinationResult, gameState.GetInvaderSummary(invaderDestination).ToString());
		}

		PowerCard Given_RiverPlayingWashAway() {
			Given_GameWithSpirits(new RiverSurges());

			//   And: a game on Board-A
			Board board = Board.BuildBoardA();
			gameState.Island = new Island(board);

			//   And: Presence on A5 (city/costal)
			var presenceSpace = board[5]; // The 'Y' land in the middle
			spirit.Presence.Add(presenceSpace);

			//   And: Purchased WashAway
			var card = spirit.AvailableCards.Single(c => c.Name == WashAway.Name);
			spirit.Energy = card.Cost;
			spirit.BuyAvailableCards(card);

			// Jump to slow
			spirit.UnresolvedActions.Clear();
			spirit.UnresolvedActions.AddRange(spirit.ActiveCards.Where(x => x.Speed == Speed.Slow));
			Assert_CardIsReady(card);

			return card;
		}

		#endregion


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

		void Assert_CardIsReady( PowerCard card ) {
			Assert.Contains(card, spirit.UnresolvedActions.OfType<PowerCard>().ToList());
		}

		PowerCard Given_PurchasedCard(string cardName) {
			var card = spirit.AvailableCards.Single(c => c.Name == cardName);
			spirit.BuyAvailableCards(card);
			return card;
		}

		void Given_GameWithSpirits(params Spirit[] spirits) {
			spirit = spirits[0];
			gameState = new GameState(spirits);
		}

		static void Given_PurchasedFakePowercards(Spirit otherSpirit, int expectedEnergyBonus) {
			for (int i = 0; i < expectedEnergyBonus; ++i) {
				var otherCard = new PowerCard("Fake-" + i, 0, Speed.Slow);
				otherSpirit.ActiveCards.Add(otherCard);
				otherSpirit.UnresolvedActions.Add(otherCard);
			}
		}

	}

}



