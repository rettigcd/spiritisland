using System.Collections.Generic;
using Xunit;

namespace SpiritIsland.Tests {

	public class BoardSpace_Tests {

		[Theory]
		[InlineData( 0 )]
		[InlineData( 1 )]
		[InlineData( 2 )]
		public void Space_Is0DistanceFromSelf( int distance ) {
			var space = new Space();
			var spaces = space.SpacesWithin( distance );
			Assert.Contains( space, spaces );
		}

		[Fact]
		public void Adjacentcy_IsTransitive() {
			// Given: land-1 is adjacent to land-2
			var land1 = new Space();
			var land2 = new Space();
			land1.SetAdjacentTo( land2 );
			// Then: land2 is adjacent to land 1
			Assert.Contains( land1, land2.SpacesExactly( 1 ) );
			Assert.Contains( land2, land1.SpacesExactly( 1 ) );
		}

		[Fact]
		public void MultipleNeighbors() {
			// Given space has 2 neighbors
			var main = new Space();
			var neighbor1 = new Space();
			var neighbor2 = new Space();
			main.SetAdjacentTo( neighbor1 );
			main.SetAdjacentTo( neighbor2 );
			// Then: it is adjacent to both
			var neighbors = main.SpacesWithin( 1 );
			Assert.Contains( neighbor1, neighbors );
			Assert.Contains( neighbor2, neighbors );
			//  And: Neighbors are 2 away from each other
			Assert.Contains( neighbor2, neighbor1.SpacesWithin( 2 ) );
			Assert.Contains( neighbor1, neighbor2.SpacesExactly( 2 ) );
		}

		#region Internal Board Connectivity

		[Fact]
		public void BoardA_Connectivity() {
			var boardA = Board.BuildBoardA();

			Assert_CanReachSpaceWithNHops( boardA[0], 0, boardA[0] );
			Assert_CanReachSpaceWithNHops( boardA[0], 1, boardA[1], boardA[2], boardA[3] );
			Assert_CanReachSpaceWithNHops( boardA[0], 2, boardA[4], boardA[5], boardA[6] );
			Assert_CanReachSpaceWithNHops( boardA[0], 3, boardA[7], boardA[8] );

			Assert.True( boardA[1].IsCostal );
			Assert.True( boardA[2].IsCostal );
			Assert.True( boardA[3].IsCostal );

		}

		#endregion

		Board BoardA => Board.BuildBoardA();
		Board BoardB => Board.BuildBoardB();
		Board BoardC => Board.BuildBoardC();
		Board BoardD => Board.BuildBoardD();

		[Fact]
		public void PlaceTiles() {

			var tileB = BoardB;
			var tileD = BoardD;

			tileB.Sides[2].AlignTo( tileD.Sides[0] );

			Assert_BoardSpacesTouch( tileB[3], tileD[1] );
			Assert_BoardSpacesTouch( tileB[4], tileD[1] ); 
			Assert_BoardSpacesTouch( tileB[4], tileD[8] ); 
			Assert_BoardSpacesTouch( tileB[7], tileD[8] ); 
		}

		[Fact]
		public void Island_1Board(){
			new Island(BoardC);
			// no connectivity to test
		}

		[Fact]
		public void Island_2Boards() {
			var boardC = BoardC;
			var boardD = BoardD;

			new Island( boardC, boardD );

			// The are properly connected as a 2-board island
			Assert_BoardSpacesTouch( boardC[3], boardD[6] ); 
			Assert_BoardSpacesTouch( boardC[3], boardD[4] ); 
			Assert_BoardSpacesTouch( boardC[4], boardD[4] ); 
			Assert_BoardSpacesTouch( boardC[4], boardD[3] ); 

		}

		//static void Assert_BoardSpacesTouch( Board boardC, int x, Board boardD, int y ) {
		//	Assert_BoardSpacesTouch( boardC[x], boardD[y] );
		//}

		static void Assert_BoardSpacesTouch( Space startingSpace, Space neighbor ) {
			Assert.Contains( neighbor, startingSpace.SpacesExactly( 1 ) );// , $"{a.Label} should touch {b.Label}" );
		}

		[Fact]
		public void Island_3Boards(){
			var b = Board.BuildBoardB();
			var c = Board.BuildBoardC();
			var d = Board.BuildBoardD();

			new Island(b,c,d);

			Assert_BoardSpacesTouch( b[3], c[8] );
			Assert_BoardSpacesTouch( b[3], c[7] );
			Assert_BoardSpacesTouch( b[4], c[7] );
			Assert_BoardSpacesTouch( b[4], c[4] );
			Assert_BoardSpacesTouch( b[7], c[4] );

			Assert_BoardSpacesTouch( c[3], d[8] );
			Assert_BoardSpacesTouch( c[3], d[7] );
			Assert_BoardSpacesTouch( c[4], d[7] );
			Assert_BoardSpacesTouch( c[4], d[6] );

			Assert_BoardSpacesTouch( d[3], b[8] );
			Assert_BoardSpacesTouch( d[4], b[8] );
			Assert_BoardSpacesTouch( d[4], b[7] );
			Assert_BoardSpacesTouch( d[6], b[7] );

		}

		[Fact]
		public void Island_4Boards(){
			var a = BoardA;
			var b = BoardB;
			var c = BoardC;
			var d = BoardD;

			new Island(a,b,c,d);

			Assert_BoardSpacesTouch( a[3], b[1] );
			Assert_BoardSpacesTouch( a[3], b[6] );
			Assert_BoardSpacesTouch( a[4], b[6] );
			Assert_BoardSpacesTouch( a[4], b[8] );
			Assert_BoardSpacesTouch( a[5], b[8] );
			Assert_BoardSpacesTouch( a[7], b[8] );

			Assert_BoardSpacesTouch( b[7], c[8] );
			Assert_BoardSpacesTouch( b[7], c[7] );
			Assert_BoardSpacesTouch( b[8], c[7] );
			Assert_BoardSpacesTouch( b[8], c[4] );

			Assert_BoardSpacesTouch( c[3], d[1] );
			Assert_BoardSpacesTouch( c[4], d[1] );
			Assert_BoardSpacesTouch( c[4], d[8] );

			Assert_BoardSpacesTouch( d[6], a[8] );
			Assert_BoardSpacesTouch( d[7], a[8] );
			Assert_BoardSpacesTouch( d[7], a[7] );
			Assert_BoardSpacesTouch( d[8], a[7] );

		}


		#region private

		void Assert_CanReachSpaceWithNHops( Space source, int distance, params Space[] needles ) {
			Assert.Equal( needles, source.SpacesExactly( distance ) );
		}

		#endregion

	}

}
