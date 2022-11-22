namespace SpiritIsland;

public partial class Board {

	public BoardLayout Layout { get; set; }

	#region factories

	// These cannot be reused because when they get connected to other boards, 
	// there neighbor state changes.
	static readonly public string[] AvailableBoards = { "A", "B", "C", "D" };
	static public Board BuildBoard(string boardName) {
		return boardName switch {
			"A" => SpiritIsland.Board.BuildBoardA(),
			"B" => SpiritIsland.Board.BuildBoardB(),
			"C" => SpiritIsland.Board.BuildBoardC(),
			"D" => SpiritIsland.Board.BuildBoardD(),
			_ => null,
		};
	}

	static public Board BuildBoardA() {
		var board = new Board("A"
			,new Space1(Terrain.Ocean,"A0")
			,new Space1(Terrain.Mountain,"A1") // 
			,new Space1(Terrain.Wetland,"A2","CD")  // city, dahan
			,new Space1(Terrain.Jungle,"A3","DD")   // 2 dahan
			,new Space1(Terrain.Sand,"A4","B")     // blight
			,new Space1(Terrain.Wetland,"A5")
			,new Space1(Terrain.Mountain,"A6","D") // 1 dahan
			,new Space1(Terrain.Sand,"A7","DD")     // 2 dahan
			,new Space1(Terrain.Jungle,"A8","T")   // town
		);

		board.SetNeighbors(0, 1,2,3);
		board.SetNeighbors(1, 2,4,5,6);
		board.SetNeighbors(2, 3,4);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5);
		board.SetNeighbors(5, 6,7,8);
		board.SetNeighbors(6, 8);
		board.SetNeighbors(7, 8);

		// sides of the board are # starting at the ocean and moving around the board clockwise
		board.DefineSide(1,6,8).BreakAt(3,7);	// Side 0
		board.DefineSide(8,7).BreakAt(5);		// Side 1
		board.DefineSide(7,5,4,3).BreakAt(1,3,7);// Side 2

		board.Layout = BoardLayout.BoardA();

		return board;
	}

	static public Board BuildBoardB() {
		var board = new Board("B"
			,new Space1(Terrain.Ocean,"B0")
			,new Space1(Terrain.Wetland,"B1","D")  // 1 dahan
			,new Space1(Terrain.Mountain,"B2", "C") // city
			,new Space1(Terrain.Sand,"B3","DD")     // 2 dahan
			,new Space1(Terrain.Jungle,"B4","B")   // blight
			,new Space1(Terrain.Sand,"B5")
			,new Space1(Terrain.Wetland,"B6", "T")  // 1 town
			,new Space1(Terrain.Mountain,"B7","D") // 1 dahan
			,new Space1(Terrain.Jungle,"B8","DD")   // 2 dahan
		);

		board.SetNeighbors(0, 1,2,3);
		board.SetNeighbors(1, 2,4,5,6);
		board.SetNeighbors(2, 3,4);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5,7);
		board.SetNeighbors(5, 6,7);
		board.SetNeighbors(6, 7,8);
		board.SetNeighbors(7, 8);

		// sides of the board are # starting at the ocean and moving around the board clockwise
		board.DefineSide(1,6,8).BreakAt(1,7);	// Side 0
		board.DefineSide(8,7).BreakAt(5);		// Side 1
		board.DefineSide(7,4,3).BreakAt(1,5);    // Side 2

		board.Layout = BoardLayout.BoardB();

		return board;
	}

	static public Board BuildBoardC() {
		var board = new Board("C"
			,new Space1(Terrain.Ocean,"C0")
			,new Space1(Terrain.Jungle,"C1","D")   // 1 dahan
			,new Space1(Terrain.Sand,"C2","C")     // city
			,new Space1(Terrain.Mountain,"C3","DD") // 2 dahan
			,new Space1(Terrain.Jungle,"C4")   
			,new Space1(Terrain.Wetland,"C5","DDB")  // 2 dahan, blight
			,new Space1(Terrain.Sand,"C6","D")     // 1 dahan
			,new Space1(Terrain.Mountain,"C7","T") // 1 town
			,new Space1(Terrain.Wetland,"C8")
		);

		board.SetNeighbors(0, 1,2,3);
		board.SetNeighbors(1, 2,5,6);
		board.SetNeighbors(2, 3,4,5);
		board.SetNeighbors(3, 4);
		board.SetNeighbors(4, 5,7);
		board.SetNeighbors(5, 6,7);
		board.SetNeighbors(6, 7,8);
		board.SetNeighbors(7, 8);

		// sides of the board are # starting at the ocean and moving around the board clockwise
		board.DefineSide(1,6,8).BreakAt(3,7);	// Side 0
		board.DefineSide(8,7,4).BreakAt(7,11);	// Side 1
		board.DefineSide(4,3).BreakAt(7);       // Side 2

		board.Layout = BoardLayout.BoardC();

		return board;
	}

	static public Board BuildBoardD() {
		var board = new Board("D"
			,new Space1(Terrain.Ocean,"D0")
			,new Space1(Terrain.Wetland,"D1","DD")   // 2 dahan
			,new Space1(Terrain.Jungle,"D2","CD")    // city, 1 dahan
			,new Space1(Terrain.Wetland,"D3")   
			,new Space1(Terrain.Sand,"D4")   
			,new Space1(Terrain.Mountain,"D5","DB")  // 1 dahan, blight
			,new Space1(Terrain.Jungle,"D6")    
			,new Space1(Terrain.Sand,"D7","TDD")      // 1 town, 2 dahan
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

		// sides of the board are # starting at the ocean and moving around the board clockwise
		board.DefineSide(1,8).BreakAt(11);		// Side 0
		board.DefineSide(8,7,6).BreakAt(5,11);	// Side 1
		board.DefineSide(6,4,3).BreakAt(3,9);   // Side 2

		board.Layout = BoardLayout.BoardD();

		return board;
	}

	#endregion

	/// <summary>
	/// These Spaces start out in numeric order at beginning of game but are not guaranteed to stay in numeric order. (Absolute Statis removes spaces from board and restores them.)
	/// </summary>
	public IEnumerable<Space> Spaces => spaces;

	#region Add / Remove spaces from board
	public void Add( Space space, Space[] adjacents ) {
		spaces = spaces.Union( new[] {space}).ToArray();
		space.SetAdjacentToSpaces(adjacents);
	} // Absolute Stasis / Weave togehter

	/// <returns>Old adjacents</returns>
	public Space[] Remove( Space space ) {
		var oldAdj = space.Adjacent.ToArray();
		space.Disconnect();
		spaces = spaces.Where(s => s != space ).ToArray();
		return oldAdj;
	} // Absolute Stasis / Weave together
	#endregion

	public Space Ocean => Spaces.Single( space => space.IsOcean );

	public Space this[int index]{ get => spaces[index]; }

	#region constructor

	public Board(string name, params Space[] spaces){
		this.Name = name;
		this.spaces = spaces;
		foreach(var space in spaces) space.Board = this;
	}

	#endregion

	public string Name { get; }

	/// <summary>
	/// public so we can build a test board.
	/// </summary>
	public void SetNeighbors(int srcIndex, params int[] neighborIndex){
		spaces[srcIndex].SetAdjacentToSpaces( neighborIndex.Select(i=>spaces[i] ).ToArray());
	}

	public BoardSide[] Sides => this.sides.ToArray();

	BoardSide DefineSide( params int[] spaceIndexes ) {
		var side = new BoardSide( this, spaceIndexes.Select( i => spaces[i] ).ToArray() );
		this.sides.Add( side );
		return side;
	}

	Space[] spaces;

	readonly List<BoardSide> sides = new List<BoardSide>();

}