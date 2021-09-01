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
			card.ActivateAsync( spirit, gameState );
			var action = spirit.Action;

			//  Then: card is resolved
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
			var grp = gameState.Invaders.Counts[targetSpace];
			grp.Add( Invader.Explorer,explorerCount );
			grp.Add( Invader.Town,townCount );
			grp.Add( Invader.City,cityCount );

			//  When: activating card
			card.ActivateAsync( spirit, gameState );
			action = spirit.Action;

			var invader = action.GetCurrent().Options[0] as InvaderSpecific;
			Then_SelectInvaderToPush(invader,3, invader.Summary,"Done");

			//  Then: card has options of where to push 1 explorer
			Assert_Options(	targetSpace.Adjacent, new TextOption("Done") );

			var invaderDestination = board[2];
			action.Choose( invaderDestination.Label );

			// And: apply doesn't throw an exception
			Assert.True(action.IsResolved);

			// check that explore was moved
			gameState.Assert_Invaders(targetSpace, expectedTargetResult );
			gameState.Assert_Invaders( invaderDestination, expectedDestinationResult );
		}

		// WashAway: Multiple target lands

		// WashAway: into ocean?
		[Fact]
		public void DoesntPushIntoOcean(){
			Given_RiverPlayingWashAway("A4");

			// 1 explorer on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[2];
			gameState.Invaders.Counts[targetSpace].Add(Invader.Explorer);

			//  When: activating card
//			var engine = spirit.Bind( gameState );
			card.ActivateAsync( spirit, gameState );
			action = spirit.Action;

			Then_SelectInvaderToPush( Invader.Explorer[1], 3,"E@1", "Done" );
			
			//  Then: card has options of where to push 1 explorer
			Assert_Options( targetSpace.Adjacent.Where(x=>x.Terrain != Terrain.Ocean ), new TextOption("Done") );

		}

		[Fact]
		public void One1Target2PushableTypes() {
			Given_RiverPlayingWashAway();

			// 1 explorer + 1 Town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			var grp =gameState.Invaders.Counts[targetSpace];
			grp.Add(Invader.Explorer);
			grp.Add(Invader.Town);

			var explorerDestination = board[2];
			var townDestination = board[3];

			//  When: activating card
			card.ActivateAsync( spirit, gameState );
			action = spirit.Action;

			//  Then: Select Explorer
			Assert.False(action.IsResolved);
			Assert_Options("E@1,T@2","Done");
			action.Choose("E@1");

			//  Then: Select destination for Explorer
			Assert.False(action.IsResolved);
			action.Choose( explorerDestination );

			//  Then: Select Town
			Assert.False(action.IsResolved);
			Assert_Options("T@2","Done");
			action.Choose( "T@2" );

			//  Then: Select destination for Town
			Assert.False(action.IsResolved);
			action.Choose( townDestination );

			// And: apply doesn't throw an exception
			Assert.True(action.IsResolved);

			// check that explore was moved
			gameState.Assert_Invaders(targetSpace,"");
			gameState.Assert_Invaders(explorerDestination, "1E@1" );
			gameState.Assert_Invaders(townDestination, "1T@2" );
		}

		[Fact]
		public void DamagedTown() {
			Given_RiverPlayingWashAway();

			// 1 damaged town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Invaders.Counts[targetSpace].Adjust( Invader.Town[1], 1 );

			var invaderDestination = board[2];

			//  When: activating card
			card.ActivateAsync( spirit, gameState );
			action = spirit.Action;

			//  Auto-Selects: target space
			//			action.Select( targetSpace );

			Then_SelectInvaderToPush( Invader.Town[1], 3, "T@1", "Done" );
			Then_PushInvader( "T@1", invaderDestination, "A1","A2","A3","A5","Done" );

			Assert.True( action.IsResolved );

			// check that explore was moved
			gameState.Assert_Invaders( targetSpace, "" );
			gameState.Assert_Invaders( invaderDestination, "1T@1" );
		}

		void Then_PushInvader( 
			string invaderText, 
			Space invaderDestination, 
			params string[] options
		) {
			Assert.False( action.IsResolved );
			Assert.Equal( "Push "+invaderText+" to", action.GetCurrent().Prompt );
			Assert_Options( options );
			action.Choose( invaderDestination );
		}

		void Then_SelectInvaderToPush( InvaderSpecific invader, int remaining, params string[] options ) {
			Assert.False( action.IsResolved );
			Assert.Equal( $"Select invader to push ({remaining} remaining)", action.GetCurrent().Prompt );
			Assert_Options( options );
			action.Choose( action.GetCurrent().Options.Single( x => x.Text == invader.Summary ) );
		}

		// WashAway: push 3 different invaders to 3 different lands
		// WashAway: multiple invader types
		[Fact]
		public void Push3InvadersToDifferentLands() {
			Given_RiverPlayingWashAway();

			// 1 explorer + 1 Town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Invaders.Counts[ targetSpace ].Add( Invader.Explorer, 3 );

			//  When: activating card
			When_PlayingCard();

			Then_SelectInvaderToPush(Invader.Explorer[1],3, "E@1","Done");

			//  Then: Select destination for Explorer 1
			var dstn1 = board[2];
			action.Choose( dstn1 );
			Assert.False( action.IsResolved );

			Then_SelectInvaderToPush(Invader.Explorer[1],2,"E@1","Done");

			//  Then: Select destination for Explorer 2
			var dstn2 = board[3];
			action.Choose( dstn2 );
			Assert.False( action.IsResolved );

			Then_SelectInvaderToPush(Invader.Explorer[1],1,"E@1","Done");
			//  Then: Select destination for Explorer 3
			var dstn3 = board[5];
			action.Choose( dstn3 );
			Assert.True( action.IsResolved );

			// And: apply doesn't throw an exception

			// check that explore was moved
			gameState.Assert_Invaders(targetSpace,"");
			gameState.Assert_Invaders(dstn1, "1E@1" );
			gameState.Assert_Invaders( dstn2, "1E@1" );
			gameState.Assert_Invaders( dstn3, "1E@1" );
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



