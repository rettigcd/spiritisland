﻿using SpiritIsland.PowerCards;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {
	public class WashAway_Tests : SpiritCards_Tests {

		[Fact]
		public void Nothing() {
			Given_RiverPlayingWashAway();

			// no explorers

			//  When: activating card
			var action = card.Bind(spirit, gameState);

			//  Then: card has 0 options
			Assert.Empty(action.Options);

			Assert.True(action.IsResolved);

			action.Apply();
			// !!! test that nothing changes
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
			gameState.Adjust(Invader.Explorer,targetSpace,explorerCount);
			gameState.Adjust(Invader.Town,targetSpace,townCount);
			gameState.Adjust(Invader.City,targetSpace,cityCount);

			//  When: activating card
			var action = card.Bind(spirit, gameState);

			//  Then: card has options of where to push 1 explorer
			Assert.Equal(
				targetSpace.SpacesExactly(1).Select(s=>s.Label).OrderBy(x=>x).Join(",")
				,action.Options.Select(s=>s.Text).OrderBy(x=>x).Join(",")
			);
			var invaderDestination = board[2];
			action.Select(action.Options.Single(x => x.Text == invaderDestination.Label));
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
		public void DoesntPushIntoOcean(){
			Given_RiverPlayingWashAway("A4");

			// 1 explorer on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[2];
			gameState.Adjust(Invader.Explorer,targetSpace,1);

			//  When: activating card
			var action = card.Bind(spirit, gameState);

			//  Then: card has options of where to push 1 explorer
			Assert_Options(action,targetSpace.SpacesExactly(1).Where(x=>x.IsLand));

		}

		[Fact]
		public void One1Target2PushableTypes() {
			Given_RiverPlayingWashAway();

			// 1 explorer + 1 Town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Adjust(Invader.Explorer,targetSpace,1);
			gameState.Adjust(Invader.Town,targetSpace,1);

			//  When: activating card
			var action = card.Bind(spirit, gameState);

			//  Then: Select Explorer
			Assert.False(action.IsResolved);
			Assert_Options(action,"E@1,T@2");
			action.Select(Invader.Explorer);

			//  Then: Select destination for Explorer
			var explorerDestination = board[2];
			Assert.False(action.IsResolved);
			action.Select( explorerDestination );

			//  Then: Select destination for Town
			var townDestination = board[3];
			Assert.False(action.IsResolved);
			action.Select( townDestination );

			// And: apply doesn't throw an exception
			Assert.True(action.IsResolved);
			action.Apply();

			// check that explore was moved
			Assert.Equal("", gameState.GetInvaderGroup(targetSpace).ToString());
			Assert.Equal("1E@1", gameState.GetInvaderGroup(explorerDestination).ToString());
			Assert.Equal("1T@2", gameState.GetInvaderGroup(townDestination).ToString());
		}


		[Fact]
		public void DamagedTown(){
			Given_RiverPlayingWashAway();

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
			action.Select(action.Options.Single(x => x.Text == invaderDestination.Label));
			Assert.True(action.IsResolved);

			// And: apply doesn't throw an exception
			action.Apply();

			// check that explore was moved
			Assert.Equal("", gameState.GetInvaderGroup(targetSpace).ToString());
			Assert.Equal("1T@1", gameState.GetInvaderGroup(invaderDestination).ToString());
		}

		// WashAway: push 3 different invaders to 3 different lands
		// WashAway: multiple invader types
		[Fact]
		public void Push3InvadersToDifferentLands() {
			Given_RiverPlayingWashAway();

			// 1 explorer + 1 Town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Adjust( Invader.Explorer, targetSpace, 3 );

			//  When: activating card
			When_PlayingCard();

			//  Then: Select destination for Explorer 1
			var dstn1 = board[2];
			action.Select( dstn1 );
			Assert.False( action.IsResolved );

			//  Then: Select destination for Explorer 2
			var dstn2 = board[3];
			action.Select( dstn2 );
			Assert.False( action.IsResolved );

			//  Then: Select destination for Explorer 3
			var dstn3 = board[5];
			action.Select( dstn3 );
			Assert.True( action.IsResolved );

			// And: apply doesn't throw an exception
			action.Apply();

			// check that explore was moved
			Assert.Equal( "", gameState.GetInvaderGroup( targetSpace ).ToString() );
			Assert.Equal( "1E@1", gameState.GetInvaderGroup( dstn1 ).ToString() );
			Assert.Equal( "1E@1", gameState.GetInvaderGroup( dstn2 ).ToString() );
			Assert.Equal( "1E@1", gameState.GetInvaderGroup( dstn3 ).ToString() );
		}

		void Given_RiverPlayingWashAway(string startingPresence="A5") {
			// A5 is the 'Y' land in the middle
			Given_GameWithSpirits(new RiverSurges());

			//   And: a game on Board-A
			Board board = Board.BuildBoardA();
			gameState.Island = new Island(board);

			//   And: Presence on A5 (city/costal)
			var presenceSpace = board.Spaces.Single(s=>s.Label==startingPresence);
			spirit.Presence.Add(presenceSpace);

			//   And: Purchased WashAway
			card = spirit.AvailableCards.Single(c => c.Name == WashAway.Name);
			spirit.Energy = card.Cost;
			spirit.BuyAvailableCards(card);

			// Jump to slow
			spirit.UnresolvedActions.Clear();
			spirit.UnresolvedActions.AddRange(spirit.ActiveCards.Where(x => x.Speed == Speed.Slow));
			Assert_CardIsReady(card);

		}

	}

}



