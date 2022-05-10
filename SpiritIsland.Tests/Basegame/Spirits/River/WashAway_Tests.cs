﻿using SpiritIsland.Basegame;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class WashAway_Tests : SpiritCards_Tests {

		public WashAway_Tests():base(new RiverSurges() ) { }

		[Fact]
		public void Nothing() {
			Given_RiverPlayingWashAway();
			gameState.Phase = Phase.Slow;

			// no explorers

			When_PlayingCard();

			User.Assert_Done();

		}

		[Trait("Feature","Push")]
		[Theory]
		[InlineData(1,0,0,"","1E@1")]
		[InlineData(0,1,0,"","1T@2")]
		[InlineData(1,0,1,"1C@3","1E@1")]
		public void OneTarget1PushableType(int explorerCount, int townCount, int cityCount, string expectedTargetResult, string expectedDestinationResult) {
			Given_RiverPlayingWashAway();
			gameState.Phase = Phase.Slow;

			// 1 explorer on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			var grp = gameState.Tokens[targetSpace];
			grp.AdjustDefault( Invader.Explorer, explorerCount );
			grp.AdjustDefault( Invader.Town, townCount );
			grp.AdjustDefault( Invader.City, cityCount );

			When_PlayingCard();

			User.TargetsLand( WashAway.Name, targetSpace.Label );

			var invader = spirit.Action.GetCurrent().Options[0] as Token;

			User.OptionallyPushesInvaderTo(invader.ToString(),"A1,(A2),A3,A5");

			// check that explore was moved
			gameState.Assert_Invaders(targetSpace, expectedTargetResult );
			gameState.Assert_Invaders( board[2], expectedDestinationResult );
		}

		// WashAway: Multiple target lands

		// WashAway: into ocean?
		[Trait("Feature","Push")]
		[Fact]
		public void DoesntPushIntoOcean(){
			Given_RiverPlayingWashAway("A4");
			gameState.Phase = Phase.Slow;

			// 1 explorer on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[2];
			gameState.Tokens[targetSpace].AdjustDefault(Invader.Explorer, 1);

			When_PlayingCard();

			User.TargetsLand( WashAway.Name, targetSpace.Label );
			User.OptionallyPushesInvaderTo("E@1","A1,(A3),A4");
		}

		[Trait("Feature","Push")]
		[Fact]
		public void One1Target2PushableTypes() {
			Given_RiverPlayingWashAway();
			gameState.Phase = Phase.Slow;

			// 1 explorer + 1 Town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			var grp =gameState.Tokens[targetSpace];
			grp.AdjustDefault(Invader.Explorer, 1);
			grp.AdjustDefault(Invader.Town, 1);

			var explorerDestination = board[2];
			var townDestination = board[3];

			When_PlayingCard();

			User.TargetsLand( WashAway.Name, targetSpace.Label );
			User.OptionallyPushesInvaderTo("(E@1),T@2","A1,(A2),A3,A5", 2);
			User.OptionallyPushesInvaderTo("T@2","A1,A2,(A3),A5");

			User.Assert_Done();

			// check that explore was moved
			gameState.Assert_Invaders(targetSpace,"");
			gameState.Assert_Invaders(explorerDestination, "1E@1" );
			gameState.Assert_Invaders(townDestination, "1T@2" );
		}

		[Trait("Feature","Push")]
		[Fact]
		public void DamagedTown() {
			Given_RiverPlayingWashAway();
			gameState.Phase = Phase.Slow;

			// 1 damaged town on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Tokens[targetSpace].Adjust( Tokens.Town1, 1 );

			var invaderDestination = board[2];

			When_PlayingCard();
			System.Threading.Thread.Sleep(50); // !!! is this necessary?

			User.TargetsLand( WashAway.Name, targetSpace.Label );
			User.OptionallyPushesInvaderTo("T@1","A1,(A2),A3,A5");

			User.Assert_Done();

			// check that explore was moved
			gameState.Assert_Invaders( targetSpace, "" );
			gameState.Assert_Invaders( invaderDestination, "1T@1" );
		}

		// WashAway: push 3 different invaders to 3 different lands
		// WashAway: multiple invader types
		[Trait("Feature","Push")]
		[Fact]
		public void Push3InvadersToDifferentLands() {
			Given_RiverPlayingWashAway();
			gameState.Phase = Phase.Slow;

			// 31 explorers on A4
			var board = gameState.Island.Boards[0];
			Space targetSpace = board[4];
			gameState.Tokens[ targetSpace ].AdjustDefault( Invader.Explorer, 3 );

			//  When: activating card
			When_PlayingCard();

			User.TargetsLand( WashAway.Name, targetSpace.Label );
			User.OptionallyPushesInvaderTo("E@1","A1,(A2),A3,A5", 3);
			User.OptionallyPushesInvaderTo("E@1","A1,A2,(A3),A5", 2);
			User.OptionallyPushesInvaderTo("E@1","A1,A2,A3,(A5)");

			// check that explore was moved
			gameState.Assert_Invaders( targetSpace,"" );
			gameState.Assert_Invaders( board[2], "1E@1" );
			gameState.Assert_Invaders( board[3], "1E@1" );
			gameState.Assert_Invaders( board[5], "1E@1" );
		}

		void Given_RiverPlayingWashAway(string startingPresence="A5") {
			// A5 is the 'Y' land in the middle
			Given_GameWithSpirits(spirit);

			//   And: a game on Board-A
			Board board = Board.BuildBoardA();
			gameState.Island = new Island(board);

			//   And: Presence on A5 (city/costal)
			var presenceSpace = board.Spaces.Single(s=>s.Label==startingPresence);
			spirit.Presence.PlaceOn(presenceSpace, gameState);

			//   And: Purchased WashAway
			card = spirit.Hand.Single(c => c.Name == WashAway.Name);
			spirit.Energy = card.Cost;
			PlayCard();

			// Jump to slow
			Assert_CardIsReady(card,Phase.Slow);

		}

	}

}



