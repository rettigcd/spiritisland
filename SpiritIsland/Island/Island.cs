namespace SpiritIsland;
public class Island {

	public Board[] Boards { get; private set; }

	public void RemoveBoard( Board b ) {
		Boards = Boards.Where(x=>x!=b).ToArray();
	}

	public void AddBoard( BoardSide newBoardSide, BoardSide existing ) {
		newBoardSide.ConnectTo( existing, true );
		var boardList = Boards.ToList();
		boardList.Add( newBoardSide.Board );
		Boards = boardList.ToArray();
	}

	public Island(params Board[] boards){
		this.Boards = boards;
		switch(boards.Length){
			case 1: break;
			case 2:
				// Make adjacent
				boards[1].Sides[2].ConnectTo( boards[0].Sides[2], true );
				break;
			case 3:
				// aligns the board CCW going around island permiter
				// when veiwing from ocean, board-0 is on left of board-1
				boards[2].Sides[2].ConnectTo( boards[0].Sides[1], true );
				boards[1].Sides[1].ConnectTo( boards[0].Sides[2], true );
				boards[1].Sides[2].ConnectTo( boards[2].Sides[1], false );
				break;
			case 4:
				// aligns the board CCW going around island permiter
				// when veiwing from ocean, board-0 is on left of board-1
				boards[3].Sides[1].ConnectTo( boards[0].Sides[1], true );
				boards[1].Sides[0].ConnectTo( boards[0].Sides[2], true );
				boards[2].Sides[1].ConnectTo( boards[1].Sides[1], true );
				boards[2].Sides[2].ConnectTo( boards[3].Sides[0], false );
				break;

			default: throw new ArgumentOutOfRangeException(nameof(boards.Length),"wrong # of boards");
		}
	}

	// !!! Review the use cases for each TerrainMapper - Determine if all are necessary or could be renamed for their use.
	public readonly TerrainMapper Terrain           = new TerrainMapper(); // Default
	public readonly TerrainMapper Terrain_ForFear   = new TerrainMapper(); // Default
	public          TerrainMapper Terrain_ForPower  = new TerrainMapper();
	public          TerrainMapper Terrain_ForBlight = new TerrainMapper();

}