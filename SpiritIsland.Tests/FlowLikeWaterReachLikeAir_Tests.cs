﻿using Shouldly;
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
			tokens[TokenType.Dahan.Default] = 3;
			tokens[Invader.Explorer.Default] = 3;
			tokens[Invader.Town.Default] = 3;

			//  And: spirit has presence on A5
			spirit.Presence.PlaceOn(a5);

			//  And: A1 has nothing on it
			var a1 = board[1];

			// When: playing Card
			async Task PlayCard(TargetSpiritCtx ctx) { try { await FlowLikeWaterReachLikeAir.ActAsync( ctx ); } catch(Exception ex) { 
				int i = 0; 
			} } 
			_ = PlayCard( new TargetSpiritCtx(spirit,gameState,spirit,Cause.Power) );

			//  And: Can bring 2 of each
			user.AssertDecisionX( "Push Presence (bringing up to 2 explorers, 2 towns, 2 dahan)", "A5" );
			user.AssertDecisionX( "Move Presence + up to 2 explorers,towns,dahan to", "(A1),A4,A6,A7,A8" );
			user.AssertDecisionX( "Push up to (6)", "(D@2),E@1,T@2,Done" );
			user.AssertDecisionX( "Push up to (5)", "(D@2),E@1,T@2,Done" );
			user.AssertDecisionX( "Push up to (4)", "(E@1),T@2,Done" );
			user.AssertDecisionX( "Push up to (3)", "(E@1),T@2,Done" );
			user.AssertDecisionX( "Push up to (2)", "(T@2),Done" );
			user.AssertDecisionX( "Push up to (1)", "(T@2),Done" );

			// Then: target 2 of each
			var dst = gameState.Tokens[a1];
			dst[TokenType.Dahan.Default].ShouldBe(2);
			dst[Invader.Explorer.Default].ShouldBe(2);
			dst[Invader.Town.Default].ShouldBe(2);
		}
		
	}


}
