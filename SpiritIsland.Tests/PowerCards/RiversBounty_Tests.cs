using SpiritIsland.PowerCards;
using System.Linq;
using Xunit;

namespace SpiritIsland.Tests {

	public class RiversBounty_Tests : SpiritCards_Tests {

		Board board;

		public RiversBounty_Tests(){
			// A5 is the 'Y' land in the middle
			Given_GameWithSpirits(new RiverSurges());

			//   And: a game on Board-A
			board = Board.BuildBoardA();
			gameState.Island = new Island(board);

			//   And: Presence on A5 (city/costal)
			var presenceSpace = board.Spaces.Single(s=>s.Label=="A4");
			spirit.Presence.Add(presenceSpace);

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

			if(dahanToGather>1){
				Assert_Options(action,"1","2");
				Assert.False(action.IsResolved);
				action.Select("2"); // do both
			}

			Assert.True( action.IsResolved );
			action.Apply();

			Assert.Equal( endingCount, gameState.GetDahanOnSpace( target ) ); // same as original
		}

		[Fact]
		public void DahanComingDifferentLands() {
			// Given: spirit has 1 presence
			Space target = spirit.Presence.Single();

			//   And: neighbors have 
			const int dahanToGather = 2;
			var neighbors = target.SpacesExactly(1).ToArray();
			for(int i=0;i<dahanToGather;++i)
			Given_AddDahan( 1, neighbors[i] );

			When_PlayingCard();

			Assert.False(action.IsResolved);
			action.Select("2");

			// Select 1st land
			Assert.False( action.IsResolved );
			Assert_Options(action,neighbors.Take(dahanToGather));
			action.Select(neighbors[0]);

			Assert.True( action.IsResolved );
			action.Apply();

			Assert.Equal( 3, gameState.GetDahanOnSpace( target ) ); // same as original
		}

		[Fact]
		public void TwoPresenceSpaces(){
			// Given: spirit has 2 presence on non-touching spaces
			spirit.Presence.Add(board[8]);
			var targetOptions = spirit.Presence.Distinct().ToArray();
			Assert.Equal(2,targetOptions.Length);

			//   And: both presence have 2 dahan already in them
			foreach(var space in targetOptions)
				Given_AddDahan(2,space);

			When_PlayingCard();

			// Select 1st land
			Assert.False( action.IsResolved );
			Assert_Options(action,targetOptions);
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
