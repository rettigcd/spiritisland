using System.Xml.Linq;

namespace SpiritIsland;

public partial class Board {

	#region factories

	// These cannot be reused because when they get connected to other boards, 
	// there neighbor state changes.
	static readonly public string[] AvailableBoards = { "A", "B", "C", "D", "E", "F" };
	static public Board BuildBoard(string boardName, BoardOrientation orientation) {
		return boardName switch {
			"A" => BuildBoardA( orientation ),
			"B" => BuildBoardB( orientation ),
			"C" => BuildBoardC( orientation ),
			"D" => BuildBoardD( orientation ),
			"E" => BuildBoardE( orientation ),
			"F" => BuildBoardF( orientation ),
			_ => null,
		};
	}

	static public Board BuildBoardA(BoardOrientation orientation=null) {

		Board board = new Board("A"
			, orientation ?? BoardOrientation.Home
			, new Space1(Terrain.Ocean,   "A0")
			, new Space1(Terrain.Mountain,"A1" ) // 
			, new Space1(Terrain.Wetland, "A2", "CD")  // city, dahan
			, new Space1(Terrain.Jungle,  "A3", "DD")   // 2 dahan
			, new Space1(Terrain.Sand,    "A4", "B")     // blight
			, new Space1(Terrain.Wetland, "A5" )
			, new Space1(Terrain.Mountain,"A6", "D") // 1 dahan
			, new Space1(Terrain.Sand,    "A7", "DD")     // 2 dahan
			, new Space1(Terrain.Jungle,  "A8", "T")   // town
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

	static public Board BuildBoardB( BoardOrientation orientation = null ) {

		Board board = new Board("B"
			, orientation ?? BoardOrientation.Home
			,new Space1(Terrain.Ocean,   "B0")
			,new Space1(Terrain.Wetland, "B1","D")  // 1 dahan
			,new Space1(Terrain.Mountain,"B2","C") // city
			,new Space1(Terrain.Sand,    "B3","DD")     // 2 dahan
			,new Space1(Terrain.Jungle,  "B4","B")   // blight
			,new Space1(Terrain.Sand,    "B5")
			,new Space1(Terrain.Wetland, "B6","T")  // 1 town
			,new Space1(Terrain.Mountain,"B7","D") // 1 dahan
			,new Space1(Terrain.Jungle,  "B8","DD")   // 2 dahan
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

	static public Board BuildBoardC( BoardOrientation orientation = null ) {

		var board = new Board("C"
			, orientation ?? BoardOrientation.Home
			,new Space1(Terrain.Ocean,   "C0")
			,new Space1(Terrain.Jungle,  "C1","D")   // 1 dahan
			,new Space1(Terrain.Sand,    "C2","C")     // city
			,new Space1(Terrain.Mountain,"C3","DD") // 2 dahan
			,new Space1(Terrain.Jungle,  "C4")   
			,new Space1(Terrain.Wetland, "C5","DDB")  // 2 dahan, blight
			,new Space1(Terrain.Sand,    "C6","D")     // 1 dahan
			,new Space1(Terrain.Mountain,"C7","T") // 1 town
			,new Space1(Terrain.Wetland, "C8")
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

	static public Board BuildBoardD( BoardOrientation orientation = null ) {
		var board = new Board("D"
			, orientation ?? BoardOrientation.Home
			,new Space1(Terrain.Ocean,   "D0")
			,new Space1(Terrain.Wetland, "D1","DD")   // 2 dahan
			,new Space1(Terrain.Jungle,  "D2","CD")    // city, 1 dahan
			,new Space1(Terrain.Wetland, "D3")   
			,new Space1(Terrain.Sand,    "D4")   
			,new Space1(Terrain.Mountain,"D5","DB")  // 1 dahan, blight
			,new Space1(Terrain.Jungle,  "D6")    
			,new Space1(Terrain.Sand,    "D7","TDD")      // 1 town, 2 dahan
			,new Space1(Terrain.Mountain,"D8")
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

	static public Board BuildBoardE( BoardOrientation orientation = null ) {
		var layout = BoardLayout.BoardE();
		var board = new Board( "E"
			, orientation ?? BoardOrientation.Home
			, new Space1( Terrain.Ocean,    "E0")
			, new Space1( Terrain.Sand,     "E1", "D" )   // 1 dahan
			, new Space1( Terrain.Mountain, "E2", "C" )    // city
			, new Space1( Terrain.Jungle,   "E3", "DD" )  // 2 dahan
			, new Space1( Terrain.Wetland,  "E4", "B" )      // 1 blight
			, new Space1( Terrain.Mountain, "E5", "D" )  // 1 dahan
			, new Space1( Terrain.Sand,     "E6" )
			, new Space1( Terrain.Jungle,   "E7", "T" )      // 1 town
			, new Space1( Terrain.Wetland,  "E8", "DD" ) // 2 dahan
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

	static public Board BuildBoardF( BoardOrientation orientation = null ) {

		var board = new Board( "F"
			, orientation ?? BoardOrientation.Home
			, new Space1( Terrain.Ocean,    "F0" )
			, new Space1( Terrain.Sand,     "F1","DD" )
			, new Space1( Terrain.Jungle,   "F2","C" )
			, new Space1( Terrain.Wetland,  "F3","D" )
			, new Space1( Terrain.Mountain, "F4","B" )
			, new Space1( Terrain.Jungle,   "F5","D" )
			, new Space1( Terrain.Mountain, "F6","DD" )
			, new Space1( Terrain.Wetland,  "F7","" )
			, new Space1( Terrain.Sand,     "F8","T" )
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

	/// <summary>InPlay (and existing) spaces on the board.</summary>
	public IEnumerable<Space> Spaces => Spaces_Existing.IsInPlay();

	public IEnumerable<Space> Spaces_Existing => _spaces.Where( Space.Exists );

	/// <summary>All spaces, including the ones that are not in play and are in Stasis.</summary>
	public IEnumerable<Space> Spaces_Unfiltered => _spaces;

	#region Add / Remove spaces from board
	public void AddSpace( Space space ) {
		_spaces = _spaces.Union( new[] {space}).ToArray();
	} // Weave togehter

	/// <returns>Old adjacents</returns>
	public Space[] Remove( Space space ) {
		var oldAdj = space.Adjacent_Existing.ToArray();
		space.Disconnect();
		_spaces = _spaces.Where(s => s != space ).ToArray();
		return oldAdj;
	} // Weave together
	#endregion

	// !!! Boards need saved to Memento,
	// Spaces Active/Inactive can be changed
	// Weave Together and cast down can change board spaces to and all the linkage.
	// To do this, we probably need to pull the Layout coordinates out of The Board/Space classes
	// and move to the UI.
	public int InvaderActionCount { get; set; } = 1;

	public Space Ocean => Spaces_Existing.Single( s => s.IsOcean );

	public Space this[int index]{ get => _spaces[index]; }

	#region constructor

	public Board(
		string name, 
		BoardOrientation orientation,
		params Space1[] spaces
	){
		Name = name;
		Orientation = orientation;

		// attach spaces
		_spaces = spaces;
		for(int i = 0; i < spaces.Length; ++i) {
			Space1 space = spaces[i];
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

	public BoardSide[] Sides => _sides.ToArray();

	public BoardOrientation Orientation { get; }

	BoardSide DefineSide( params int[] spaceIndexes ) {
		var side = new BoardSide( this, spaceIndexes.Select( i => _spaces[i] ).ToArray() );
		_sides.Add( side );
		return side;
	}

	readonly List<BoardSide> _sides = new List<BoardSide>();
	Space[] _spaces;



}