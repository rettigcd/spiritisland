using SpiritIsland.Basegame;
using SpiritIsland;
using System.Linq;
using Xunit;
using Shouldly;

namespace SpiritIsland.Tests.Basegame.Spirits.River {

	public class RiversBounty_Tests : SpiritCards_Tests {

		readonly Board board;

		public RiversBounty_Tests():base(new RiverSurges() ) {

			// A5 is the 'Y' land in the middle
			Given_GameWithSpirits( spirit );

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

			//  Select: A4
			User.TargetsLand("A4");

			string token = "D@2 on " + neighbor.Label;

			// Select source 1
			if(dahanToGather>0)
				User.GathersOptionalToken( token );
			
			// Select source 2
			if(dahanToGather>1)
				User.GathersOptionalToken( token );
			

			Assert.Equal( endingCount, gameState.DahanGetCount( target ) ); // same as original
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

			User.TargetsLand( "A4" );
			User.GathersOptionalToken("(D@2 on A1),D@2 on A2");
			User.GathersOptionalToken("D@2 on A2");

			User.Assert_Done();

			Assert.Equal( 3, gameState.DahanGetCount( target ) ); // same as original
		}

		[Fact]
		public void DamagedDahanComingDifferentLands() {
			// Given: spirit has 1 presence
			Space target = spirit.Presence.Placed.Single();
			var ctx = new TargetSpaceCtx(spirit,gameState,target,Cause.Power);

			//   And: neighbors have 1 damaged dahan each 
			const int dahanToGather = 2;
			var neighbors = ctx.Adjacents.ToArray();
			for(int i = 0; i<dahanToGather;++i)
			foreach(var n in neighbors)
				ctx.TargetSpace(neighbors[i]).Tokens[TokenType.Dahan[1]] = 1;

			When_PlayingCard();

			User.TargetsLand( "A4" );
			User.GathersOptionalToken("(D@1 on A1),D@1 on A2");
			User.GathersOptionalToken("D@1 on A2");

			User.Assert_Done();

			Assert.Equal( 3, gameState.DahanGetCount( target ) ); // same as original
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

			User.TargetsLand("(A4),A8");
			User.GathersOptionalToken( "D@2 on A5" );
			User.GathersOptionalToken( "D@2 on A5" );

			User.Assert_Done();

			Assert.Equal( 3, gameState.DahanGetCount( board[4] ) ); // same as original
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
			User.TargetsLand( "A4" );

			User.Assert_Done();

		}



		[Fact]
		public void DahanCountIncludesDamaged() {
			// This is a nice test, but it is too close to the implementation.  Refactoring might not use ctx.DahanCount
			var space = gameState.Island.Boards[0][4];
			var ctx = new TargetSpaceCtx(spirit,gameState,space, Cause.None);
			ctx.Tokens[TokenType.Dahan[1]] = 5;
			ctx.Tokens[TokenType.Dahan[2]] = 7;
			ctx.DahanCount.ShouldBe(12);
		}

		void Given_AddDahan( int startingCount, Space target ) {
			gameState.DahanAdjust( target, startingCount );
			Assert.Equal( startingCount, gameState.DahanGetCount( target ) );
		}

	}

}
