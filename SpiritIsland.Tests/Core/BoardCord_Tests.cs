namespace SpiritIsland.Tests;

[Trait("Space","BoardCoordinates")]
public class BoardCord_Tests {

	[Theory]
	[InlineData( 0,  0, 0, "[0,0][1,0][1,1][0,1]" )]
	[InlineData( 1,  0, 0, "[0,0][0,1][-1,2][-1,1]" )]
	[InlineData( 2,  0, 0, "[0,0][-1,1][-2,1][-1,0]" )]
	[InlineData( 3,  0, 0, "[0,0][-1,0][-1,-1][0,-1]" )]
	[InlineData( 4,  0, 0, "[0,0][0,-1][1,-2][1,-1]" )]
	[InlineData( 5,  0, 0, "[0,0][1,-1][2,-1][1,0]" )]
	[InlineData( 12, 0, 0, "[0,0][1,0][1,1][0,1]" )] // wrap back to 0
	[InlineData( 0, 3, 3, "[3,3][4,3][4,4][3,4]" )]
	public void BoardRotationAndOffset(int rotationTick, int d0, int d60, string expected) {
		var board = Board.BuildBoardA(
			new BoardOrientation( new BoardCoord( d0, d60 ), rotationTick )
		);
		Assert_CornersShouldBe(board.Orientation, expected );
	}

	static void Assert_CornersShouldBe( BoardOrientation board, string expectedCorners ) {
		string.Join("",board.Corners.Select(s=>s.ToString())).ShouldBe( expectedCorners );
	}

	static XY[] GetCornerPoints(BoardOrientation orientation )
		=> BoardLayout.Get("B").ReMap( orientation.GetTransformMatrix() ).BoardCorners;

	// Layout - when we align boards, the new boards move and have the correct corners.
	[Fact]
	public void JoiningSides2ToHome2() {
		var orien = BoardOrientation.ToMatchSide( 2, BoardOrientation.Home.SideCoord( 2 ) );
		Assert_CornersShouldBe( orien, "[1,2][0,2][0,1][1,1]" );

		XY[] bCorners = GetCornerPoints(orien);
		// Then
		bCorners[3].ShouldBe( 1.5f, BoardLayout.boardHeight );
		bCorners[2].ShouldBe( 0.5f, BoardLayout.boardHeight );
		bCorners[1].ShouldBe( 1f, 2 * BoardLayout.boardHeight );
		bCorners[0].ShouldBe( 2f, 2 * BoardLayout.boardHeight );

	}

	[Fact]
	public void JoiningSides2ToHome1() {
		var orien = BoardOrientation.ToMatchSide( 2, BoardOrientation.Home.SideCoord( 1 ) );
		Assert_CornersShouldBe( orien, "[2,-1][2,0][1,1][1,0]" );

		XY[] bCorners = GetCornerPoints( orien );
		// Then
		bCorners[3].ShouldBe( 1, 0 );
		bCorners[2].ShouldBe( 1.5f, BoardLayout.boardHeight );
		bCorners[1].ShouldBe( 2, 0 );
		bCorners[0].ShouldBe( 1.5f, -BoardLayout.boardHeight );
	}

	[Fact]
	public void JoiningSides1ToHome2() {
		var orien = BoardOrientation.ToMatchSide( 1, BoardOrientation.Home.SideCoord( 2 ) );
		Assert_CornersShouldBe( orien, "[-1,2][0,1][1,1][0,2]" );

		XY[] bCorners = GetCornerPoints( orien );
		// Then
		bCorners[3].ShouldBe( 1f, 2 * BoardLayout.boardHeight );
		bCorners[2].ShouldBe( 1.5f, BoardLayout.boardHeight );
		bCorners[1].ShouldBe( 0.5f, BoardLayout.boardHeight );
		bCorners[0].ShouldBe( 0, 2 * BoardLayout.boardHeight );
	}

	[Fact]
	public void JoiningSides1ToHome1() {
		var orien = BoardOrientation.ToMatchSide( 1, BoardOrientation.Home.SideCoord( 1 ) );
		Assert_CornersShouldBe( orien, "[2,1][1,1][1,0][2,0]" );

		XY[] bCorners = GetCornerPoints( orien );
		// Then
		bCorners[3].ShouldBe( 2, 0 );
		bCorners[2].ShouldBe( 1, 0 );
		bCorners[1].ShouldBe( 1.5f, BoardLayout.boardHeight );
		bCorners[0].ShouldBe( 2.5f, BoardLayout.boardHeight );
	}

	[Fact]
	public void JoiningSides1ToHome0() {
		var orien = BoardOrientation.ToMatchSide( 1, BoardOrientation.Home.SideCoord( 0 ) );
		Assert_CornersShouldBe( orien, "[2,-1][1,0][0,0][1,-1]" );

		XY[] bCorners = GetCornerPoints( orien );
		// Then
		bCorners[3].ShouldBe( 0.5f, -BoardLayout.boardHeight );
		bCorners[2].ShouldBe( 0, 0 );
		bCorners[1].ShouldBe( 1, 0 );
		bCorners[0].ShouldBe( 1.5f, -BoardLayout.boardHeight );
	}

	[Fact]
	public void JoiningSides0ToHome1() {
		var orien = BoardOrientation.ToMatchSide( 0, BoardOrientation.Home.SideCoord( 1 ) );
		Assert_CornersShouldBe( orien, "[1,1][1,0][2,-1][2,0]" );

		XY[] bCorners = GetCornerPoints( orien );
		// Then
		bCorners[3].ShouldBe( 2, 0 );
		bCorners[2].ShouldBe( 1.5f, -BoardLayout.boardHeight );
		bCorners[1].ShouldBe( 1, 0 );
		bCorners[0].ShouldBe( 1.5f, BoardLayout.boardHeight );
	}


	[Fact]
	public void JoiningSides0ToHome2() {
		var orien = BoardOrientation.ToMatchSide( 0, BoardOrientation.Home.SideCoord( 2 ) );
		Assert_CornersShouldBe( orien, "[0,1][1,1][1,2][0,2]" );

		XY[] bCorners = GetCornerPoints( orien );
		// Then
		bCorners[3].ShouldBe( 1f, 2 * BoardLayout.boardHeight );
		bCorners[2].ShouldBe( 2f, 2 * BoardLayout.boardHeight );
		bCorners[1].ShouldBe( 1.5f, BoardLayout.boardHeight );
		bCorners[0].ShouldBe( 0.5f, BoardLayout.boardHeight );
	}

	[Fact]
	public void JoiningSides2ToHome0() {
		var orien = BoardOrientation.ToMatchSide( 2, BoardOrientation.Home.SideCoord( 0 ) );
		Assert_CornersShouldBe( orien, "[0,-1][1,-1][1,0][0,0]" );

		XY[] bCorners = GetCornerPoints( orien );
		// Then
		bCorners[3].ShouldBe( 0, 0 );
		bCorners[2].ShouldBe( 1, 0 );
		bCorners[1].ShouldBe( 0.5f, -BoardLayout.boardHeight );
		bCorners[0].ShouldBe( -0.5f, -BoardLayout.boardHeight );

	}


	[Fact]
	public void JoiningSides0ToHome0() {
		var orien = BoardOrientation.ToMatchSide( 0, BoardOrientation.Home.SideCoord( 0 ) );
		Assert_CornersShouldBe( orien, "[1,0][0,0][0,-1][1,-1]" );

		XY[] bCorners = GetCornerPoints( orien );
		// Then
		bCorners[3].ShouldBe( 0.5f, -BoardLayout.boardHeight );
		bCorners[2].ShouldBe( -0.5f, -BoardLayout.boardHeight );
		bCorners[1].ShouldBe( 0, 0 );
		bCorners[0].ShouldBe( 1, 0 );
	}


	[Fact]
	public void TwoBoards_CannotOverlap() {
		Board a = Board.BuildBoardA();
		Board b = Board.BuildBoardB();
		Should.Throw<Exception>(()=>new Island(a,b));
	}

	[Fact]
	public void TwoBoards_CannotHideOcean() {

		// Given: board 1 in home positin
		Board a = Board.BuildBoardA();

		//   And: board 2 rotated up 1 slot to hide board1's ocean
		BoardOrientation orien = new BoardOrientation(BoardCoord.Origin,1);
		Board b = Board.BuildBoardB(orien);

		// When build the island
		Island action() => new Island( a, b );

		// Then: get an exception
		Should.Throw<ArgumentException>( action );
	}


}