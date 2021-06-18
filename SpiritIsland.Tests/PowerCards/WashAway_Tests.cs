using SpiritIsland.PowerCards;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {
	public class WashAway_Tests : SpiritCards_Tests {

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
			while(0<explorerCount--) gameState.Adjust(Invader.Explorer,targetSpace,1);
			while(0<townCount--) gameState.Adjust(Invader.Town,targetSpace,1);
			while(0<cityCount--) gameState.Adjust(Invader.City,targetSpace,1);

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

			// check that explore was moved
			Assert.Equal(expectedTargetResult, gameState.GetInvaderGroup(targetSpace).ToString());
			Assert.Equal(expectedDestinationResult, gameState.GetInvaderGroup(invaderDestination).ToString());
		}

		// WashAway: Multiple target lands

		// WashAway: into ocean?
		[Fact]
		public void WashAway_DoesntPushIntoOcean(){
			PowerCard card = Given_RiverPlayingWashAway("A4");

			// 1 explorer on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[2];
			gameState.Adjust(Invader.Explorer,targetSpace,1);

			//  When: activating card
			var action = card.Bind(spirit, gameState);

			//  Then: card has options of where to push 1 explorer
			Assert.Equal(
				targetSpace.SpacesExactly(1).Where(x=>x.IsLand).Select(s=>s.Label).OrderBy(x=>x).Join(",")
				,action.GetOptions().Select(s=>s.Text).OrderBy(x=>x).Join(",")
			);

		}

		// WashAway: multiple invader types
		[Fact]
		public void WashAway_1Target2PushableTypes() {
			PowerCard card = Given_RiverPlayingWashAway();

			// 1 explorer on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Adjust(Invader.Explorer,targetSpace,1);
			gameState.Adjust(Invader.Town,targetSpace,1);

			//  When: activating card
			var action = card.Bind(spirit, gameState);

			//  Then: card has options of Explorer or Town
			Assert.Equal("E@1,T@2",action.GetOptions().Select(s=>s.Text).OrderBy(x=>x).Join(",") );
			action.Select(Invader.Explorer);

			var invaderDestination = board[2];
			action.Select(action.GetOptions().Single(x => x.Text == invaderDestination.Label));
			Assert.True(action.IsResolved);

			// And: apply doesn't throw an exception
			action.Apply();

			// check that explore was moved
			Assert.Equal("1T@2", gameState.GetInvaderGroup(targetSpace).ToString());
			Assert.Equal("1E@1", gameState.GetInvaderGroup(invaderDestination).ToString());
		}

		// washAway: damaged towns
		[Fact]
		public void WashAway_DamagedTown(){
			PowerCard card = Given_RiverPlayingWashAway();

			// 1 damaged town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Adjust(Invader.Town,targetSpace,1);
			gameState.ApplyDamage(targetSpace,new DamagePlan(1,Invader.Town));

			//  When: activating card
			var action = card.Bind(spirit, gameState);
			Assert.False(action.IsResolved);

			//  Then: card has options of Explorer or Town
			var invaderDestination = board[2];
			action.Select(action.GetOptions().Single(x => x.Text == invaderDestination.Label));
			Assert.True(action.IsResolved);

			// And: apply doesn't throw an exception
			action.Apply();

			// check that explore was moved
			Assert.Equal("", gameState.GetInvaderGroup(targetSpace).ToString());
			Assert.Equal("1T@1", gameState.GetInvaderGroup(invaderDestination).ToString());
		}

		// WashAway: push 2 different invaders to 2 different lands

		// WashAway: push 3 different invaders to 3 different lands

		PowerCard Given_RiverPlayingWashAway(string startingPresence="A5") {
			// A5 is the 'Y' land in the middle
			Given_GameWithSpirits(new RiverSurges());

			//   And: a game on Board-A
			Board board = Board.BuildBoardA();
			gameState.Island = new Island(board);

			//   And: Presence on A5 (city/costal)
			var presenceSpace = board.Spaces.Single(s=>s.Label==startingPresence);
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

	}

}



