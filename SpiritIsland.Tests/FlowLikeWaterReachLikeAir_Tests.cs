using Shouldly;
using SpiritIsland.BranchAndClaw;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests {

	public class FlowLikeWaterReachLikeAir_Tests {

		[Trait("Feature","Push")]
		[Fact]
		public void CanBring_2TownDahanExplorer() {
			// Setup
			var spirit = new TestSpirit(PowerCard.For<FlowLikeWaterReachLikeAir>());
			var user = new VirtualUser( spirit );
			var board = Board.BuildBoardA();
			var gameState = new GameState( spirit, board );
			gameState.DisableInvaderDeck();
			gameState.Initialize();

			// Given: A5 has 3 Towns, Dahans, and Explorers
			var a5 = board[5];
			var tokens = gameState.Tokens[a5];
			tokens.InitDefault(TokenType.Dahan , 3);
			tokens.InitDefault(Invader.Explorer, 3);
			tokens.InitDefault(Invader.Town    , 3);

			//  And: spirit has presence on A5
			spirit.Presence.PlaceOn(a5,gameState);

			//  And: A1 has nothing on it
			var a1 = board[1];

			// When: playing Card
			static async Task PlayCard(TargetSpiritCtx ctx) { try { await FlowLikeWaterReachLikeAir.ActAsync( ctx ); } catch(Exception ex) { 
				_ = ex.ToString(); 
			} } 
			var action = gameState.StartAction();
			_ = PlayCard( spirit.BindMyPower( gameState, action ).TargetSpirit(spirit) );

			//  And: Can bring 2 of each
			user.AssertDecisionX( "Select Presence to push.", "(A5),Done" );
			user.AssertDecisionX( "Push Presence to", "(A1),A4,A6,A7,A8" );
			user.AssertDecisionX( "Push up to (6)", "(D@2),E@1,T@2,Done" );
			user.AssertDecisionX( "Push up to (5)", "(D@2),E@1,T@2,Done" );
			user.AssertDecisionX( "Push up to (4)", "(E@1),T@2,Done" );
			user.AssertDecisionX( "Push up to (3)", "(E@1),T@2,Done" );
			user.AssertDecisionX( "Push up to (2)", "(T@2),Done" );
			user.AssertDecisionX( "Push up to (1)", "(T@2),Done" );

			// Then: target 2 of each
			var dst = gameState.Tokens[a1];
			dst[StdTokens.Dahan].ShouldBe(2);
			dst[StdTokens.Explorer].ShouldBe(2);
			dst[StdTokens.Town].ShouldBe(2);
		}
		
	}


}
