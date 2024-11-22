namespace SpiritIsland;

#nullable enable

public class Board {

	#region factories

	// These cannot be reused because when they get connected to other boards, 
	// there neighbor state changes.
	static readonly public string[] AvailableBoards = [ "A", "B", "C", "D", "E", "F" ];
	static public Board BuildBoard(string boardName, BoardOrientation orientation) {
		return boardName switch {
			"A" => BuildBoardA( orientation ),
			"B" => BuildBoardB( orientation ),
			"C" => BuildBoardC( orientation ),
			"D" => BuildBoardD( orientation ),
			"E" => BuildBoardE( orientation ),
			"F" => BuildBoardF( orientation ),
			_ => throw new ArgumentException($"Unknown board '{boardName}'", nameof(boardName))
		};
	}

	static public Board BuildBoardA(BoardOrientation? orientation=null) {

		Board board = new Board("A"
			, orientation ?? BoardOrientation.Home
			, new SingleSpaceSpec(Terrain.Ocean,   "A0")
			, new SingleSpaceSpec(Terrain.Mountain,"A1" ) // 
			, new SingleSpaceSpec(Terrain.Wetland, "A2", "CD")  // city, dahan
			, new SingleSpaceSpec(Terrain.Jungle,  "A3", "DD")   // 2 dahan
			, new SingleSpaceSpec(Terrain.Sands,    "A4", "B")     // blight
			, new SingleSpaceSpec(Terrain.Wetland, "A5" )
			, new SingleSpaceSpec(Terrain.Mountain,"A6", "D") // 1 dahan
			, new SingleSpaceSpec(Terrain.Sands,    "A7", "DD")     // 2 dahan
			, new SingleSpaceSpec(Terrain.Jungle,  "A8", "T")   // town
		);

		board.SetNeighbors(0, 1,2,3);
		board.SetNeighbors(1, 2,4,5,6);
		board.SetNeighbors(2, 3,4);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5);
		board.SetNeighbors(5, 6,7,8);
		board.SetNeighbors(6, 8);
		board.SetNeighbors(7, 8);

		board.DefineSide( 7, 5, 4, 3 ).BreakAt( 1, 3, 7 );// Side 0
		board.DefineSide( 8, 7 ).BreakAt( 5 );            // Side 1
		board.DefineSide(1,6,8).BreakAt(3,7);	          // Side 2

		return board;
	}

	static public Board BuildBoardB( BoardOrientation? orientation = null ) {

		Board board = new Board("B"
			, orientation ?? BoardOrientation.Home
			,new SingleSpaceSpec(Terrain.Ocean,   "B0")
			,new SingleSpaceSpec(Terrain.Wetland, "B1","D")  // 1 dahan
			,new SingleSpaceSpec(Terrain.Mountain,"B2","C") // city
			,new SingleSpaceSpec(Terrain.Sands,    "B3","DD")     // 2 dahan
			,new SingleSpaceSpec(Terrain.Jungle,  "B4","B")   // blight
			,new SingleSpaceSpec(Terrain.Sands,    "B5")
			,new SingleSpaceSpec(Terrain.Wetland, "B6","T")  // 1 town
			,new SingleSpaceSpec(Terrain.Mountain,"B7","D") // 1 dahan
			,new SingleSpaceSpec(Terrain.Jungle,  "B8","DD")   // 2 dahan
		);

		board.SetNeighbors(0, 1,2,3);
		board.SetNeighbors(1, 2,4,5,6);
		board.SetNeighbors(2, 3,4);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5,7);
		board.SetNeighbors(5, 6,7);
		board.SetNeighbors(6, 7,8);
		board.SetNeighbors(7, 8);

		board.DefineSide( 7, 4, 3 ).BreakAt( 1, 5 );    // Side 0
		board.DefineSide( 8, 7 ).BreakAt( 5 );          // Side 1
		board.DefineSide(1,6,8).BreakAt(1,7);	        // Side 2

		return board;
	}

	static public Board BuildBoardC( BoardOrientation? orientation = null ) {

		var board = new Board("C"
			, orientation ?? BoardOrientation.Home
			,new SingleSpaceSpec(Terrain.Ocean,   "C0")
			,new SingleSpaceSpec(Terrain.Jungle,  "C1","D")   // 1 dahan
			,new SingleSpaceSpec(Terrain.Sands,    "C2","C")     // city
			,new SingleSpaceSpec(Terrain.Mountain,"C3","DD") // 2 dahan
			,new SingleSpaceSpec(Terrain.Jungle,  "C4")   
			,new SingleSpaceSpec(Terrain.Wetland, "C5","DDB")  // 2 dahan, blight
			,new SingleSpaceSpec(Terrain.Sands,    "C6","D")     // 1 dahan
			,new SingleSpaceSpec(Terrain.Mountain,"C7","T") // 1 town
			,new SingleSpaceSpec(Terrain.Wetland, "C8")
		);

		board.SetNeighbors(0, 1,2,3);
		board.SetNeighbors(1, 2,5,6);
		board.SetNeighbors(2, 3,4,5);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5,7);
		board.SetNeighbors(5, 6,7);
		board.SetNeighbors(6, 7,8);
		board.SetNeighbors(7, 8);

		board.DefineSide( 4, 3 ).BreakAt( 7 );          // Side 0
		board.DefineSide( 8, 7, 4 ).BreakAt( 7, 11 );   // Side 1
		board.DefineSide(1,6,8).BreakAt(3,7);	        // Side 2

		return board;
	}

	static public Board BuildBoardD( BoardOrientation? orientation = null ) {
		var board = new Board("D"
			, orientation ?? BoardOrientation.Home
			,new SingleSpaceSpec(Terrain.Ocean,   "D0")
			,new SingleSpaceSpec(Terrain.Wetland, "D1","DD")   // 2 dahan
			,new SingleSpaceSpec(Terrain.Jungle,  "D2","CD")    // city, 1 dahan
			,new SingleSpaceSpec(Terrain.Wetland, "D3")   
			,new SingleSpaceSpec(Terrain.Sands,    "D4")   
			,new SingleSpaceSpec(Terrain.Mountain,"D5","DB")  // 1 dahan, blight
			,new SingleSpaceSpec(Terrain.Jungle,  "D6")    
			,new SingleSpaceSpec(Terrain.Sands,    "D7","TDD")      // 1 town, 2 dahan
			,new SingleSpaceSpec(Terrain.Mountain,"D8")
		);

		board.SetNeighbors(0, 1,2,3);
		board.SetNeighbors(1, 2,5,7,8);
		board.SetNeighbors(2, 3,4,5);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5,6);
		board.SetNeighbors(5, 6,7);
		board.SetNeighbors(6, 7);
		board.SetNeighbors(7, 8);

		board.DefineSide( 6, 4, 3 ).BreakAt( 3, 9 );  // Side 0
		board.DefineSide( 8, 7, 6 ).BreakAt( 5, 11 ); // Side 1
		board.DefineSide(1,8).BreakAt(11);		      // Side 2

		return board;
	}

	static public Board BuildBoardE( BoardOrientation? orientation = null ) {

		var board = new Board( "E"
			, orientation ?? BoardOrientation.Home
			, new SingleSpaceSpec( Terrain.Ocean,    "E0")
			, new SingleSpaceSpec( Terrain.Sands,     "E1", "D" )   // 1 dahan
			, new SingleSpaceSpec( Terrain.Mountain, "E2", "C" )    // city
			, new SingleSpaceSpec( Terrain.Jungle,   "E3", "DD" )  // 2 dahan
			, new SingleSpaceSpec( Terrain.Wetland,  "E4", "B" )      // 1 blight
			, new SingleSpaceSpec( Terrain.Mountain, "E5", "D" )  // 1 dahan
			, new SingleSpaceSpec( Terrain.Sands,     "E6" )
			, new SingleSpaceSpec( Terrain.Jungle,   "E7", "T" )      // 1 town
			, new SingleSpaceSpec( Terrain.Wetland,  "E8", "DD" ) // 2 dahan
		);

		board.SetNeighbors( 0, 1, 2, 3 );
		board.SetNeighbors( 1, 2, 5, 7 );
		board.SetNeighbors( 2, 3, 5 );
		board.SetNeighbors( 3, 4, 5 );
		board.SetNeighbors( 4, 5, 6, 7 );
		board.SetNeighbors( 5, 7 );
		board.SetNeighbors( 6, 7, 8 );
		board.SetNeighbors( 7, 8 );

		board.DefineSide( 6, 4, 3 ).BreakAt( 1, 7 ); // Side 0
		board.DefineSide( 8, 6 ).BreakAt( 9 );       // Side 1
		board.DefineSide( 1, 7, 8 ).BreakAt( 5, 9 ); // Side 2

		return board;
	}

	static public Board BuildBoardF( BoardOrientation? orientation = null ) {

		var board = new Board( "F"
			, orientation ?? BoardOrientation.Home
			, new SingleSpaceSpec( Terrain.Ocean,    "F0" )
			, new SingleSpaceSpec( Terrain.Sands,     "F1","DD" )
			, new SingleSpaceSpec( Terrain.Jungle,   "F2","C" )
			, new SingleSpaceSpec( Terrain.Wetland,  "F3","D" )
			, new SingleSpaceSpec( Terrain.Mountain, "F4","B" )
			, new SingleSpaceSpec( Terrain.Jungle,   "F5","D" )
			, new SingleSpaceSpec( Terrain.Mountain, "F6","DD" )
			, new SingleSpaceSpec( Terrain.Wetland,  "F7","" )
			, new SingleSpaceSpec( Terrain.Sands,     "F8","T" )
		);

		board.SetNeighbors( 0, 1, 2, 3 );
		board.SetNeighbors( 1, 2, 5, 6 );
		board.SetNeighbors( 2, 3, 4, 5 );
		board.SetNeighbors( 3, 4 );
		board.SetNeighbors( 4, 5, 7, 8 );
		board.SetNeighbors( 5, 6, 8 );
		board.SetNeighbors( 6, 8 );
		board.SetNeighbors( 7, 8 );

		board.DefineSide( 7, 4, 3 ).BreakAt( 3, 9 );  // Side 0
		board.DefineSide( 8, 7 ).BreakAt( 7 );        // Side 1
		board.DefineSide( 1, 6, 8 ).BreakAt( 5, 11 ); // Side 2

		return board;
	}


	#endregion

	/// <summary>Non-stasis spaces that are 'InPlay' for current Terrain/Power.</summary>
	public IEnumerable<SpaceSpec> Spaces => Spaces_Existing.Where( x=>TerrainMapper.Current.IsInPlay(x.ScopeSpace) );

	/// <summary>Non-stasis spaces </summary>
	public IEnumerable<SpaceSpec> Spaces_Existing => _spaces.Where( SpaceSpec.Exists );

	/// <summary>All spaces, including the ones that are not in play and are in Stasis.</summary>
	public IEnumerable<SpaceSpec> Spaces_Unfiltered => _spaces;

	public int InvaderActionCount { get; set; } = 1;

	public SpaceSpec Ocean => Spaces_Existing.Single( s => s.IsOcean );
	
	public SpaceSpec this[int index]{ get => _spaces[index]; }

	public BoardSide[] Sides => [.. _sides];

	public BoardOrientation Orientation { get; }

	public BoardLayout Layout => _layout ??= BoardLayout.Get(Name);

	#region constructor

	public Board(
		string name, 
		BoardOrientation orientation,
		params SingleSpaceSpec[] spaces
	){
		Name = name;
		Orientation = orientation;
		if(spaces.Length == 0)
			throw new Exception("Each Board should have 9 spaces but this one has 0.");

		// attach spaces
		_spaces = spaces;
		for(int i = 0; i < spaces.Length; ++i) {
			SingleSpaceSpec space = spaces[i];
			space.Board = this;
		}
	}

	#endregion

	public string Name { get; }

	/// <summary>
	/// public so we can build a test board.
	/// </summary>
	public void SetNeighbors(int srcIndex, params int[] neighborIndex){
		_spaces[srcIndex].SetAdjacentToSpaces( neighborIndex.Select(i=>_spaces[i] ).ToArray());
	}

	/// <summary> Used by Weave Together... </summary>
	public void AddSpace(SpaceSpec space) {
		_spaces = _spaces.Union(new[] { space }).ToArray();
	}

	/// <returns> Used by Weave Together... </returns>
	public SpaceSpec[] Remove(SpaceSpec space) {
		var oldAdj = space.Adjacent_Existing.ToArray();
		space.Disconnect();
		_spaces = _spaces.Where(s => s != space).ToArray();
		return oldAdj;
	}

	BoardSide DefineSide( params int[] spaceIndexes ) {
		var side = new BoardSide( this, spaceIndexes.Select( i => _spaces[i] ).ToArray() );
		_sides.Add( side );
		return side;
	}

	BoardLayout? _layout;
	readonly List<BoardSide> _sides = [];
	SpaceSpec[] _spaces;



}

#nullable disable