﻿using SpiritIsland.PowerCards;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class RiversBounty_Tests : SpiritCards_Tests {

		readonly Board board;

		public RiversBounty_Tests(){
			// A5 is the 'Y' land in the middle
			Given_GameWithSpirits(new RiverSurges());

			//   And: a game on Board-A
			board = Board.BuildBoardA();
			gameState.Island = new Island(board);

			//   And: Presence on A4
			spirit.Presence.Add(board[4]);

			//   And: Purchased WashAway
			card = spirit.AvailableCards.Single(c => c.Name == RiversBounty.Name);
			spirit.Energy = card.Cost;
			spirit.BuyAvailableCards(card);

			// Jump to slow
			spirit.UnresolvedActions.Clear();
			spirit.UnresolvedActions.AddRange(spirit.ActiveCards.Where(x => x.Speed == Speed.Slow));
			Assert_CardIsReady(card);

		}

		[Fact]
		public void Stats() {
			var card = PowerCard.For<RiversBounty>();
			Assert_CardStatus( card, 0, Speed.Slow, "SWB" );
		}


		// 1 target, 0 dahan, 1 to gather       => resolved, dahan gathered, no child
		// 1 target, 1 dahan, 1 to gather        => resolved, dahan gathered, child!
		// 1 target, 2 dahan, nothing to gather  => resolved, child!
		[Theory]
		[InlineData(0,0,0)]
		[InlineData(1,0,1)]
		[InlineData(1,1,3)]
		[InlineData(1,2,4)]
		[InlineData(2,1,4)]
		public void DahanComingSameLand(int startingCount, int dahanToGather, int endingCount) {
			// Given: spirit has 1 presence
			Space target = spirit.Presence.Single();

			//   And: presence space has dahan
			Given_AddDahan( startingCount, target );

			//   And: neighbors have some dahan
			Space neighbor = target.SpacesExactly(1).First();
			Given_AddDahan( dahanToGather, neighbor );

			When_PlayingCard();

			Assert.True( action.IsResolved );
			action.Apply();

			Assert.Equal( endingCount, gameState.GetDahanOnSpace( target ) ); // same as original
		}

		[Fact]
		public void DahanComingDifferentLands() {
			// Given: spirit has 1 presence
			Space target = spirit.Presence.Single();

			//   And: neighbors have 1 dahan each 
			const int dahanToGather = 2;
			var neighbors = target.SpacesExactly(1).ToArray();
			for(int i=0;i<dahanToGather;++i)
				Given_AddDahan( 1, neighbors[i] );

			When_PlayingCard();

			// Select 1st land
			Assert.False( action.IsResolved );
			Assert_Options( neighbors.Take(dahanToGather) );
			action.Select(neighbors[0]);

			Assert.True( action.IsResolved );
			action.Apply();

			Assert.Equal( 3, gameState.GetDahanOnSpace( target ) ); // same as original
		}

		[Fact]
		public void TwoPresenceSpaces(){
			// Given: spirit has presence on A4 && A8
			spirit.Presence.Add(board[8]);
			var targetOptions = spirit.Presence.Distinct().ToArray();
			Assert.Equal(2,targetOptions.Length);

			//   And: 2 dahan in A5 (touches both)
			Given_AddDahan(2,board[5]);

			When_PlayingCard();

			// Select 1st land
			Assert.False( action.IsResolved );
			Assert_Options( targetOptions );
			var target = targetOptions[0];
			action.Select( target );

			Assert.True( action.IsResolved );
			action.Apply();

			Assert.Equal( 3, gameState.GetDahanOnSpace( target ) ); // same as original

		}

		void Given_AddDahan( int startingCount, Space target ) {
			gameState.AddDahan( target, startingCount );
			Assert.Equal( startingCount, gameState.GetDahanOnSpace( target ) );
		}

	}

}
