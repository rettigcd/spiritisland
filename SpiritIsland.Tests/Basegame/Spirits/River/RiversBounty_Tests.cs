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
			gameState.Phase = Phase.Slow;

			//   And: Presence on A4
			spirit.Presence.PlaceOn( board[4], gameState );

			//   And: Purchased WashAway
			card = spirit.Hand.Single( c => c.Name == RiversBounty.Name );
			spirit.Energy = card.Cost;
			spirit.PurchaseAvailableCards_Test( card );

			// Jump to slow
			Assert_CardIsReady( card, Phase.Slow );

		}

		[Fact]
		public void Stats() {
			var card = PowerCard.For<RiversBounty>();
			Assert_CardStatus( card, 0, Phase.Slow, "SWB" );
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
			User.TargetsLand( RiversBounty.Name, "A4" );

			string token = "D@2 on " + neighbor.Label;

			// Select source 1
			if(dahanToGather > 0)
				User.GathersOptionalToken( token );

			// Select source 2
			if(dahanToGather > 1)
				User.GathersOptionalToken( token );

			Assert_DahanCount( target, endingCount );
			spirit.Energy.ShouldBe( endingEnergy );
		}

		void Assert_DahanCount( Space target, int endingCount ) {
			gameState.Tokens[target].Dahan.Count.ShouldBe( endingCount ); // same as original
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

			User.TargetsLand( RiversBounty.Name, "A4" );
			User.GathersOptionalToken("(D@2 on A1),D@2 on A2");
			User.GathersOptionalToken("D@2 on A2");

			User.Assert_Done();

			Assert_DahanCount( target, 3 );
		}

		[Fact]
		public void DamagedDahanComingDifferentLands() {
			// Given: spirit has 1 presence
			Space target = spirit.Presence.Placed.Single();
			var ctx = new TargetSpaceCtx(spirit,gameState,target,Cause.Power);

			//   And: neighbors have 1 damaged dahan each 
			const int dahanToGather = 2;
			var neighbors = ctx.Adjacent.ToArray();
			for(int i = 0; i<dahanToGather;++i)
				ctx.Target(neighbors[i]).Dahan.AddDamaged(1);

			When_PlayingCard();

			User.TargetsLand( RiversBounty.Name, "A4" );
			User.GathersOptionalToken("(D@1 on A1),D@1 on A2");
			User.GathersOptionalToken("D@1 on A2");

			User.Assert_Done();

			Assert_DahanCount( target, 3 );
		}


		[Fact]
		public void TwoPresenceSpaces(){
			// Given: spirit has presence on A4 && A8
			spirit.Presence.PlaceOn(board[8], gameState);
			var targetOptions = spirit.Presence.Spaces.ToArray();
			Assert.Equal(2,targetOptions.Length);

			//   And: 2 dahan in A5 (touches both)
			Given_AddDahan(2,board[5]);

			When_PlayingCard();

			User.TargetsLand( RiversBounty.Name,"(A4),A8");
			User.GathersOptionalToken( "D@2 on A5" );
			User.GathersOptionalToken( "D@2 on A5" );

			User.Assert_Done();

			Assert_DahanCount( board[4], 3 );

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
			User.TargetsLand( RiversBounty.Name,"A4" );

			User.Assert_Done();

		}

		[Fact]
		public void DahanCountIncludesDamaged() {
			// This is a nice test, but it is too close to the implementation.  Refactoring might not use ctx.DahanCount
			var space = gameState.Island.Boards[0][4];
			var ctx = new TargetSpaceCtx(spirit,gameState,space, Cause.None);
			var dahan = ctx.Tokens.Dahan;
			dahan.AddDamaged(5);
			dahan.Add(7); //$$$
			ctx.Dahan.Count.ShouldBe(12);
		}

		void Given_AddDahan( int startingCount, Space target ) {
			gameState.DahanOn( target ).Add( startingCount );
			Assert_DahanCount( target, startingCount );
		}

	}

}
