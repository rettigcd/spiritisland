using SpiritIsland.Basegame;
using SpiritIsland;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class WashAway_Tests : SpiritCards_Tests {

		[Fact]
		public void Nothing() {
			Given_RiverPlayingWashAway();

			// no explorers

			//  When: activating card
			card.Activate( spirit, gameState );
			var action = spirit.Action;

			//  Then: card has 0 options
			Assert.Empty(action.Options);

			Assert.True(action.IsResolved);

		}

		[Theory]
		[InlineData(1,0,0,"","1E@1")]
		[InlineData(0,1,0,"","1T@2")]
		[InlineData(1,0,1,"1C@3","1E@1")]
		public void OneTarget1PushableType(int explorerCount, int townCount, int cityCount, string expectedTargetResult, string expectedDestinationResult) {
			Given_RiverPlayingWashAway();

			// 1 explorer on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Adjust(targetSpace,InvaderSpecific.Explorer,explorerCount);
			gameState.Adjust(targetSpace,InvaderSpecific.Town,townCount);
			gameState.Adjust(targetSpace,InvaderSpecific.City,cityCount);

			//  When: activating card
			card.Activate( spirit, gameState );
			action = spirit.Action;

			var invader = action.Options[0] as InvaderSpecific;
			Then_SelectInvaderToPush(invader,invader.Summary,"Done");

			//  Then: card has options of where to push 1 explorer
			Assert_Options(	targetSpace.Adjacent );

			var invaderDestination = board[2];
			action.Select( invaderDestination.Label );

			// And: apply doesn't throw an exception
			Assert.True(action.IsResolved);

			// check that explore was moved
			Assert.Equal(expectedTargetResult, gameState.InvadersOn(targetSpace).ToString());
			Assert.Equal(expectedDestinationResult, gameState.InvadersOn(invaderDestination).ToString());
		}

		// WashAway: Multiple target lands

		// WashAway: into ocean?
		[Fact]
		public void DoesntPushIntoOcean(){
			Given_RiverPlayingWashAway("A4");

			// 1 explorer on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[2];
			gameState.Adjust(targetSpace,InvaderSpecific.Explorer,1);

			//  When: activating card
//			var engine = spirit.Bind( gameState );
			card.Activate( spirit, gameState );
			action = spirit.Action;

			Then_SelectInvaderToPush( InvaderSpecific.Explorer, "E@1", "Done" );
			
			//  Then: card has options of where to push 1 explorer
			Assert_Options( targetSpace.Adjacent.Where(x=>x.IsLand) );

		}

		[Fact]
		public void One1Target2PushableTypes() {
			Given_RiverPlayingWashAway();

			// 1 explorer + 1 Town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Adjust(targetSpace,InvaderSpecific.Explorer,1);
			gameState.Adjust(targetSpace,InvaderSpecific.Town,1);

			var explorerDestination = board[2];
			var townDestination = board[3];

			//  When: activating card
			card.Activate( spirit, gameState );
			action = spirit.Action;

			//  Then: Select Explorer
			Assert.False(action.IsResolved);
			Assert_Options("E@1,T@2","Done");
			action.Select(InvaderSpecific.Explorer);

			//  Then: Select destination for Explorer
			Assert.False(action.IsResolved);
			action.Select( explorerDestination );

			//  Then: Select Town
			Assert.False(action.IsResolved);
			Assert_Options("T@2","Done");
			action.Select(InvaderSpecific.Town);

			//  Then: Select destination for Town
			Assert.False(action.IsResolved);
			action.Select( townDestination );

			// And: apply doesn't throw an exception
			Assert.True(action.IsResolved);

			// check that explore was moved
			Assert.Equal("", gameState.InvadersOn(targetSpace).ToString());
			Assert.Equal("1E@1", gameState.InvadersOn(explorerDestination).ToString());
			Assert.Equal("1T@2", gameState.InvadersOn(townDestination).ToString());
		}

		[Fact]
		public void DamagedTown() {
			Given_RiverPlayingWashAway();

			// 1 damaged town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Adjust( targetSpace, InvaderSpecific.Town1, 1 );

			var invaderDestination = board[2];

			//  When: activating card
			card.Activate( spirit, gameState );
			action = spirit.Action;

			//  Auto-Selects: target space
			//			action.Select( targetSpace );

			Then_SelectInvaderToPush( InvaderSpecific.Town1, "T@1", "Done" );
			Then_PushInvader( "T@1", invaderDestination, "A1","A2","A3","A5" );

			Assert.True( action.IsResolved );

			// check that explore was moved
			Assert.Equal( "", gameState.InvadersOn( targetSpace ).ToString() );
			Assert.Equal( "1T@1", gameState.InvadersOn( invaderDestination ).ToString() );
		}

		void Then_PushInvader( 
			string invaderText, 
			Space invaderDestination, 
			params string[] options
		) {
			Assert.False( action.IsResolved );
			Assert.Equal( "Push "+invaderText+" to", action.Prompt );
			Assert_Options( options );
			action.Select( invaderDestination );
		}

		void Then_SelectInvaderToPush( InvaderSpecific invader, params string[] options ) {
			Assert.False( action.IsResolved );
			Assert.Equal( "Select invader to push", action.Prompt );
			Assert_Options( options );
			action.Select( action.Options.Single( x => x.Text == invader.Summary ) );
		}

		// WashAway: push 3 different invaders to 3 different lands
		// WashAway: multiple invader types
		[Fact]
		public void Push3InvadersToDifferentLands() {
			Given_RiverPlayingWashAway();

			// 1 explorer + 1 Town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Adjust( targetSpace, InvaderSpecific.Explorer, 3 );

			//  When: activating card
			When_PlayingCard();

			Then_SelectInvaderToPush(InvaderSpecific.Explorer,"E@1","Done");

			//  Then: Select destination for Explorer 1
			var dstn1 = board[2];
			action.Select( dstn1 );
			Assert.False( action.IsResolved );

			Then_SelectInvaderToPush(InvaderSpecific.Explorer,"E@1","Done");

			//  Then: Select destination for Explorer 2
			var dstn2 = board[3];
			action.Select( dstn2 );
			Assert.False( action.IsResolved );

			Then_SelectInvaderToPush(InvaderSpecific.Explorer,"E@1","Done");
			//  Then: Select destination for Explorer 3
			var dstn3 = board[5];
			action.Select( dstn3 );
			Assert.True( action.IsResolved );

			// And: apply doesn't throw an exception

			// check that explore was moved
			Assert.Equal( "", gameState.InvadersOn( targetSpace ).ToString() );
			Assert.Equal( "1E@1", gameState.InvadersOn( dstn1 ).ToString() );
			Assert.Equal( "1E@1", gameState.InvadersOn( dstn2 ).ToString() );
			Assert.Equal( "1E@1", gameState.InvadersOn( dstn3 ).ToString() );
		}

		void Given_RiverPlayingWashAway(string startingPresence="A5") {
			// A5 is the 'Y' land in the middle
			Given_GameWithSpirits(new RiverSurges());

			//   And: a game on Board-A
			Board board = Board.BuildBoardA();
			gameState.Island = new Island(board);

			//   And: Presence on A5 (city/costal)
			var presenceSpace = board.Spaces.Single(s=>s.Label==startingPresence);
			spirit.Presence.PlaceOn(presenceSpace);

			//   And: Purchased WashAway
			card = spirit.Hand.Single(c => c.Name == WashAway.Name);
			spirit.Energy = card.Cost;
			spirit.PurchaseAvailableCards(card);

			// Jump to slow
			Assert_CardIsReady(card,Speed.Slow);

		}

	}

}



