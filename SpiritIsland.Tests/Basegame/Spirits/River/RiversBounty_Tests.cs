using SpiritIsland.Basegame;
using SpiritIsland;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class RiversBounty_Tests : SpiritCards_Tests {

		readonly Board board;

		public RiversBounty_Tests() {
			// A5 is the 'Y' land in the middle
			Given_GameWithSpirits( new RiverSurges() );

			//   And: a game on Board-A
			board = Board.BuildBoardA();
			gameState.Island = new Island( board );

			//   And: Presence on A4
			spirit.Presence.PlaceOn( board[4] );

			//   And: Purchased WashAway
			card = spirit.Hand.Single( c => c.Name == RiversBounty.Name );
			spirit.Energy = card.Cost;
			spirit.PurchaseAvailableCards( card );

			// Jump to slow
			Assert_CardIsReady( card, Speed.Slow );

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
		[InlineData(0,0,0,0)]
		[InlineData(1,0,1,0)]
		[InlineData(1,1,3,1)]
		[InlineData(1,2,4,1)]
		[InlineData(2,1,4,1)]
		public void DahanComingSameLand(
			int startingCount, 
			int dahanToGather, 
			int endingCount,
			int endingEnergy
		) {
			// Given: spirit has 1 presence
			Space target = spirit.Presence.Placed.Single();

			//   And: presence space has dahan
			Given_AddDahan( startingCount, target );

			//   And: neighbors have some dahan
			Space neighbor = target.Adjacent.First();
			Given_AddDahan( dahanToGather, neighbor );

			When_PlayingCard();

			// Select source 1
			if(dahanToGather>0){
				Assert.False( action.IsResolved );
				action.Choose(neighbor);
			}
			// Select source 2
			if(dahanToGather>1){
				Assert.False( action.IsResolved );
				action.Choose(neighbor);
			}

			Assert.Equal( endingCount, gameState.DahanCount( target ) ); // same as original
			Assert.Equal( endingEnergy, spirit.Energy );
		}

		[Fact]
		public void DahanComingDifferentLands() {
			// Given: spirit has 1 presence
			Space target = spirit.Presence.Placed.Single();

			//   And: neighbors have 1 dahan each 
			const int dahanToGather = 2;
			var neighbors = target.Adjacent.ToArray();
			for(int i=0;i<dahanToGather;++i)
				Given_AddDahan( 1, neighbors[i] );

			When_PlayingCard();

			// Select 1st land
			Assert.False( action.IsResolved );
			Assert_Options( neighbors.Take(dahanToGather), TextOption.Done );
			action.Choose(neighbors[0]);
			// Select 2nd land
			Assert.False( action.IsResolved );
			action.Choose( action.GetCurrent().Options[0] );

			Assert.True( action.IsResolved );

			Assert.Equal( 3, gameState.DahanCount( target ) ); // same as original
		}

		[Fact]
		public void TwoPresenceSpaces(){
			// Given: spirit has presence on A4 && A8
			spirit.Presence.PlaceOn(board[8]);
			var targetOptions = spirit.Presence.Spaces.ToArray();
			Assert.Equal(2,targetOptions.Length);

			//   And: 2 dahan in A5 (touches both)
			Given_AddDahan(2,board[5]);

			When_PlayingCard();

			// Select Target / Destination land
			Assert.False( action.IsResolved );
			Assert_Options( targetOptions );
			var target = targetOptions[0];
			action.Choose( target );
			// Select 1st source land
			Assert.False( action.IsResolved );
			action.Choose( board[5] );
			// Select 2nd source land
			Assert.False( action.IsResolved );
			action.Choose( board[5] );

			Assert.True( action.IsResolved );

			Assert.Equal( 3, gameState.DahanCount( target ) ); // same as original
		}

		[Fact]
		public void TwoDahanOnPresenceSpace(){
			// Given: spirit has presence on A4
			var targetOptions = spirit.Presence.Spaces.ToArray();
			Assert.Single( targetOptions);

			//   And: 2 dahan in A5 (touches both)
			Given_AddDahan(2,board[4]);

			When_PlayingCard();

			// Select 1st land
//			Assert.False( action.IsResolved );
//			Assert_Options( targetOptions );
			var target = targetOptions[0];
//			action.Select( target );

			Assert.True( action.IsResolved );

			Assert.Equal( 3, gameState.DahanCount( target ) ); // same as original

		}


		void Given_AddDahan( int startingCount, Space target ) {
			gameState.AdjustDahan( target, startingCount );
			Assert.Equal( startingCount, gameState.DahanCount( target ) );
		}

	}

}
