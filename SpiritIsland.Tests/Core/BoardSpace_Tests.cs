namespace SpiritIsland.Tests;

[Trait("Space","Connectivity")]
public class BoardSpace_Tests {

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	public void Space_IsWithinXDistanceFromSelf( int distance ) {
		var space = MakeSpace("T1");
		var spaces = space.Range( distance );
		Assert.Contains( space, spaces );
	}

	static SingleSpaceSpec MakeSpace(string name) => new(Terrain.None,name,null);

	[Fact]
	public void Adjacentcy_IsTransitive() {
		// Given: land-1 is adjacent to land-2
		var land1 = MakeSpace("T1");
		var land2 = MakeSpace("t2");
		land1.SetAdjacentToSpaces( land2 );
		// Then: land2 is adjacent to land 1
		Assert.Contains( land1, land2.Adjacent_Existing );
		Assert.Contains( land2, land1.Adjacent_Existing );
	}

	[Fact]
	public void MultipleNeighbors() {
		// Given space has 2 neighbors
		SingleSpaceSpec main = MakeSpace("N0");
		SingleSpaceSpec neighbor1 = MakeSpace("N1");
		SingleSpaceSpec neighbor2 = MakeSpace("N2");
		main.SetAdjacentToSpaces( neighbor1 );
		main.SetAdjacentToSpaces( neighbor2 );

		// Then: it is adjacent to both
		var neighbors = main.Range( 1 );
		Assert.Contains( neighbor1, neighbors );
		Assert.Contains( neighbor2, neighbors );

		//  And: Neighbors are 2 away from each other
		Assert.Contains( neighbor2, neighbor1.Range( 2 ) );
		SpacesExactly( neighbor2, 2 ).ToArray().ShouldContain( neighbor1 );
		//Assert.Contains( neighbor1,  );
	}

	static IEnumerable<SpaceSpec> SpacesExactly( SpaceSpec space, int distance )
		=> distance switch {
			0 => new SpaceSpec[] { space },
			1 => space.Adjacent_Existing,
			_ => space.CalcDistances( distance ).Where( p => p.Value == distance ).Select( p => p.Key ),
		};

	#region Internal Board Connectivity

	[Fact]
	public void BoardA_Connectivity() {
		var boardA = Boards.A;

		Assert_CanReachSpaceWithNHops( boardA[0], 0, boardA[0] );
		Assert_CanReachSpaceWithNHops( boardA[0], 1, boardA[1], boardA[2], boardA[3] );
		Assert_CanReachSpaceWithNHops( boardA[0], 2, boardA[4], boardA[5], boardA[6] );
		Assert_CanReachSpaceWithNHops( boardA[0], 3, boardA[7], boardA[8] );

		Assert.True( boardA[1].IsCoastal );
		Assert.True( boardA[2].IsCoastal );
		Assert.True( boardA[3].IsCoastal );

	}

	#endregion

	[Fact]
	public void PlaceTiles() {

		var tileD = Boards.D;
		var tileB = BoardFactory.BuildB( new BoardOrientation(new BoardCoord(0,1),0) );
		_ = new Island(tileB,tileD);

		Assert_BoardSpacesTouch( tileB[3], tileD[1] );
		Assert_BoardSpacesTouch( tileB[4], tileD[1] ); 
		Assert_BoardSpacesTouch( tileB[4], tileD[8] ); 
		Assert_BoardSpacesTouch( tileB[7], tileD[8] ); 
	}

	[Fact]
	public void Island_1Board(){
		var _ = new Island(Boards.C);
		// no connectivity to test
	}

	[Fact]
	public void Island_2Boards() {
		var layout = GameBuilder.TwoBoardLayout;
		Board boardC = BoardFactory.Build( "C", layout[0] );
		Board boardD = BoardFactory.Build( "D", layout[1] );

		_ = new Island( boardC, boardD );

		// The are properly connected as a 2-board island
		Assert_BoardSpacesTouch( boardC[3], boardD[6] ); 
		Assert_BoardSpacesTouch( boardC[3], boardD[4] ); 
		Assert_BoardSpacesTouch( boardC[4], boardD[4] ); 
		Assert_BoardSpacesTouch( boardC[4], boardD[3] ); 

	}

	//static void Assert_BoardSpacesTouch( Board boardC, int x, Board boardD, int y ) {
	//	Assert_BoardSpacesTouch( boardC[x], boardD[y] );
	//}

	static void Assert_BoardSpacesTouch( SpaceSpec startingSpace, SpaceSpec neighbor ) {
		Assert.Contains( neighbor, startingSpace.Adjacent_Existing );// , $"{a.Label} should touch {b.Label}" );
	}

	[Fact]
	public void Island_3Boards(){
		var layout = GameBuilder.ThreeBoardLayout;
		Board b = BoardFactory.Build( "B", layout[0] );
		Board c = BoardFactory.Build( "C", layout[2] );
		Board d = BoardFactory.Build( "D", layout[1] );
		var _ = new Island(b,c,d);

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
		var layout = GameBuilder.FourBoardLayout;
		Board b = BoardFactory.Build( "B", layout[0] );
		Board a = BoardFactory.Build( "A", layout[1] );
		Board c = BoardFactory.Build( "C", layout[3] );
		Board d = BoardFactory.Build( "D", layout[2] );

		_ = new Island(a, b, c, d);

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

	[Theory]
	[InlineData("A1","")]
	[InlineData("A2","CD")]
	[InlineData("A3","DD")]
	[InlineData("A4","B")]
	[InlineData("A5","")]
	[InlineData("A6","D")]
	[InlineData("A7","DD")]
	[InlineData("A8","T")]
	[InlineData("B1","D")]
	[InlineData("B2","C")]
	[InlineData("B3","DD")]
	[InlineData("B4","B")]
	[InlineData("B5","")]
	[InlineData("B6","T")]
	[InlineData("B7","D")]
	[InlineData("B8","DD")]
	[InlineData("C1","D")]
	[InlineData("C2","C")]
	[InlineData("C3","DD")]
	[InlineData("C4","")]
	[InlineData("C5","DDB")]
	[InlineData("C6","D")]
	[InlineData("C7","T")]
	[InlineData("C8","")]
	[InlineData("D1","DD")]
	[InlineData("D2","CD")]
	[InlineData("D3","")]
	[InlineData("D4","")]
	[InlineData("D5","DB")]
	[InlineData("D6","")]
	[InlineData("D7","TDD")]
	[InlineData("D8","")]
	public void StartingItems(string spaceLabel,string items){
		var board = BoardFactory.Build( spaceLabel[..1] );
		var gameState = new SoloGameState( board );
		gameState.DisableInvaderDeck();
		// When:
		gameState.Initialize();
	// Then:
	var space = board.Spaces.Single(x=>x.Label==spaceLabel);
	var tokens = gameState.Tokens[space];

	int ee = items.Count(c=>c=='E');
	int aa = tokens[StdTokens.Explorer];
	Assert.True(ee==aa,tokens.InvaderSummary()+" ex:"+ee+" act:"+aa);

	Assert.Equal(items.Count(c=>c=='C'), tokens[StdTokens.City]);
	Assert.Equal(items.Count(c=>c=='T'), tokens[StdTokens.Town] );
	Assert.Equal(items.Count(c=>c=='E'), tokens[StdTokens.Explorer] );
	Assert.Equal(items.Count(c=>c=='D'), tokens.Dahan.CountAll);
	Assert.Equal(items.Count(c=>c=='B'), tokens.Blight.Any?1:0);
	}

	#region private

	static void Assert_CanReachSpaceWithNHops( SpaceSpec source, int distance, params SpaceSpec[] needles ) {
		Assert.Equal( needles, SpacesExactly( source, distance ) );
	}

	#endregion

}
