﻿using Shouldly;
using SpiritIsland.Basegame;
using SpiritIsland.BranchAndClaw;
using SpiritIsland.BranchAndClaw.Minor;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SpiritIsland.Tests {

	public class WrapInWingsOfSunlight_Tests {

		[Trait("Feature","Move")]
		[Fact]
		public void Move5() {
			// Setup
			var spirit = new TestSpirit(PowerCard.For<WrapInWingsOfSunlight>());
			var user = new VirtualUser( spirit );
			var board = Board.BuildBoardA();
			var gameState = new GameState( spirit, board );
			gameState.DisableInvaderDeck();
			gameState.Initialize();

			// Given: A3 has 10 Dahans
			var src = board[3];
			gameState.Tokens[src][TokenType.Dahan.Default] = 10;

			//  And: spirit has presence on A3
			spirit.Presence.PlaceOn(src,gameState);

			//  And: Destination has no Dahan on it
			var dst = board[1];
			gameState.Tokens[dst][TokenType.Dahan.Default] = 0;

			// When: playing Card
			static async Task PlayCard(TargetSpaceCtx ctx) { try { await WrapInWingsOfSunlight.ActAsync( ctx ); } catch(Exception ex) { 
				_ = ex.ToString(); 
			} } 
			_ = PlayCard( new TargetSpaceCtx(spirit,gameState,src,Cause.Power) );

			//  And: Can bring 2 of each
			user.AssertDecision( "Push D@2 to", "A1,A2,A3,A4,A5,A6,A7,A8,Done", dst.Label );
			user.AssertDecisionX( "# of dahan to move", "(5),4,3,2,1" );

			// Then: target 2 of each
			var dstTokens = gameState.Tokens[dst];
			dstTokens[TokenType.Dahan.Default].ShouldBe(5);
		}

		[Trait("Feature","Push")]
		[Fact]
		public void TerrifyingChase_PushDahan_NoBeast() {
			// Setup
			var spirit = new TestSpirit(PowerCard.For<TerrifyingChase>());
			var user = new VirtualUser( spirit );
			var board = Board.BuildBoardA();
			var gameState = new GameState( spirit, board );
			gameState.DisableInvaderDeck();
			gameState.Initialize();

			// Given: A5 has 3 Towns, Dahans, and Explorers
			var src = board[5];
			var tokens = gameState.Tokens[src];
			tokens[TokenType.Dahan.Default] = 3;
			tokens[Invader.Explorer.Default] = 3;
			tokens[Invader.Town.Default] = 3;

			//  And: spirit has presence on A5
			spirit.Presence.PlaceOn(src,gameState);

			//  And: dst has nothing on it
			var dst = board[8];

			// When: playing Card
			static async Task PlayCard(TargetSpaceCtx ctx) { try { await TerrifyingChase.ActAsync( ctx ); } catch(Exception ex) { 
				_ = ex.ToString(); 
			} } 
			_ = PlayCard( new TargetSpaceCtx(spirit,gameState,src,Cause.Power) );

			//  And: Can bring 2 of each
			user.AssertDecisionX( "Push (2)", "(D@2),E@1,T@2" );
			user.AssertDecision( "Push D@2 to", "A1,A4,A6,A7,A8", dst.Label );
			user.AssertDecisionX( "Push (1)", "(D@2),E@1,T@2" );
			user.AssertDecision( "Push D@2 to", "A1,A4,A6,A7,A8", dst.Label );

			// Then: target 2 of each
			var dstTokens = gameState.Tokens[dst];
			dstTokens[TokenType.Dahan.Default].ShouldBe(2);
		}


	}


	public class GrowthThroughSacrifice_Tests {

		[Fact]
		public void RevealedTokens_GainElements() {

			var fxt = new GameFixture()
				.WithSpirit(new TestSpirit(PowerCard.For<GrowthThroughSacrifice>()))
				.Start();

			fxt.spirit.AddCardToHand(PowerCard.For<GnawingRootbiters>());

			// Given: Spirits first energy track has 1-fire
			fxt.spirit.Presence.Energy.slots[1] = Track.FireEnergy;
			// Given: Spirits can play 2 cards
			fxt.spirit.Presence.CardPlays.slots[0] = Track.Card2;
			// Given: Spirit has 2 presence on A5
			fxt.spirit.Presence.PlaceOn(fxt.gameState.Island.Boards[0][5],fxt.gameState);

			// When: user grows
			fxt.user.Growth_SelectsOption(0);
			//  And: plays card SUT
			fxt.user.PlaysCard(GrowthThroughSacrifice.Name);
			//  And: plays slow card (to keep round from ending)
			fxt.user.PlaysCard(GnawingRootbiters.Name);

			fxt.spirit.Elements[Element.Fire].ShouldBe(1); // from Growth Through Sacrifice card

			//  And: Resolves SUT
			fxt.user.SelectsFastAction(GrowthThroughSacrifice.Name);
			fxt.user.AssertDecisionX("Select presence to destroy","A5");

			fxt.user.AssertDecisionX("Select Power Option","Remove 1 blight from one of your lands,(Add 1 presence to one of your lands)");
			fxt.user.AssertDecisionX("Select Presence to place.","(fire energy),2 cardplay,Take Presence from Board");
			fxt.user.AssertDecisionX("Where would you like to place your presence?","A5");

			// Should be waiting on slow action
			fxt.spirit.Elements[Element.Fire].ShouldBe(2); // 1 from Growth Through Sacrifice card, 1 from Energy track
		}

	}

}
