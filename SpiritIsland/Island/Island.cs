namespace SpiritIsland;

public sealed class Island : IHaveMemento {

	#region constructor
	public Island(params Board[] boards) {
		Boards = boards; // assigning before validation to simplify validation code

		ValidateNoOverlap();
		ValidateAccessibleOceans();

		ConnectSides();
	}

	void ValidateAccessibleOceans() {
		if(HidesAnOcean(Boards.Select(b=>b.Orientation)))
			throw new ArgumentException( $"Invalid layout. Ocean is hidden." );
	}

	static bool HidesAnOcean(IEnumerable<BoardOrientation> boards) {
		HashSet<SideCoords> sides = [];
		foreach(var board in boards)
			for(int i = 0; i < 3; ++i)
				sides.Add( board.SideCoord( i ) );
		return boards.Any( b => sides.Contains( b.OceanSideReversed ) );
	}

	void ValidateNoOverlap() {
		if(HasOverlap(Boards.Select(b=>b.Orientation)))
			throw new Exception( "Boards overlap" );
	}

	static bool HasOverlap(IEnumerable<BoardOrientation> boards) {
		var boardCount = boards.Count();
		return boards.Select( b => b.OddCorner ).Distinct().Count() != boardCount
			|| boards.Select( b => b.EvenCorner ).Distinct().Count() != boardCount;
	}

	void ConnectSides() {
		for(int b1 = 0; b1 < Boards.Length - 1; ++b1) {
			Board focusBoard = Boards[b1];
			var remainingBoards = Boards.Skip(b1);
			for(int s1 = 0; s1 < 3; s1++) {
				SideCoords focusSide = focusBoard.Orientation.SideCoord(s1);
				SideCoords side1Reversed = new SideCoords(focusSide.To,focusSide.From);
				foreach(Board board2 in remainingBoards) {
					for(int s2 = 0; s2<3; ++s2) {
						SideCoords side2 = board2.Orientation.SideCoord( s2 );
						if(side1Reversed == side2)
							board2.Sides[s2].ConnectTo( focusBoard.Sides[s1] );
					}
				}
			}
		}
	}

	#endregion

	public BoardOrientation[] AvailableConnections() {
		var openSides = Boards.SelectMany(board=>board.Orientation.Sides)
			.GroupBy(s=>s)
			.Where(grp=>grp.Count() == 1)
			.Select(grp=>grp.Key)
			.ToArray();

		var boardList = Boards.Select(b=>b.Orientation).ToList();
		boardList.Insert(0, null);

		return openSides
			.SelectMany(side => boardSideIndex.Select(i=>BoardOrientation.ToMatchSide(i,side) ))
			.Where(newOrient=> {
				boardList[0] = newOrient;
				return !HasOverlap(boardList) && !HidesAnOcean(boardList);
			} )
			.ToArray();
	}

	public Board[] Boards { get; private set; }

	public void RemoveBoard( Board b ) {
		Boards = Boards.Where( x => x != b ).ToArray();
	}

	public void AddBoard( BoardSide newBoardSide, BoardSide existing ) {
		newBoardSide.ConnectTo( existing );
		var boardList = Boards.ToList();
		boardList.Add( newBoardSide.Board );
		Boards = [..boardList];
		ValidateNoOverlap();
		ValidateAccessibleOceans();
	}

	#region Memento

	object IHaveMemento.Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	static readonly int[] boardSideIndex = [0,1,2];

	class MyMemento {
		public MyMemento( Island src ) {
			boards = src.Boards.Select( b => new BoardInfo( b ) ).ToArray();
			nativeTerrain = src.Boards.SelectMany(b=>b.Spaces_Unfiltered).OfType<Space1>()
				.ToDictionary(s=>s.Text,s=>s.NativeTerrain);
		}
		public void Restore( Island src ) {
			src.Boards = boards.Select( b => b.Restore() ).ToArray();
			foreach(Space1 space in src.Boards.SelectMany(b=>b.Spaces_Unfiltered).OfType<Space1>())
				space.NativeTerrain = nativeTerrain[space.Text];
		}
		readonly BoardInfo[] boards;
		readonly Dictionary<string,Terrain> nativeTerrain;
	}

	class BoardInfo {
		public BoardInfo( Board b ) { _name = b.Name; _orientation = b.Orientation; _invaderActionCount=b.InvaderActionCount; }
		readonly string _name;
		readonly BoardOrientation _orientation;
		readonly int _invaderActionCount;
		public Board Restore() {
			var board = Board.BuildBoard( _name, _orientation ); // supplies spaces
			board.InvaderActionCount = _invaderActionCount;
			return board;
		}
	}

	#endregion

}