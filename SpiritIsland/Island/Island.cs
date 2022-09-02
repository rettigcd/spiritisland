namespace SpiritIsland;
public class Island {

	public Board[] Boards { get; private set; }
	public void RemoveBoard( Board b ) {
		Boards = Boards.Where(x=>x!=b).ToArray();
	}

	public Island(params Board[] boards){
		this.Boards = boards;
		switch(boards.Length){
			case 1: break;
			case 2:
				// Make adjacent
				boards[0].Sides[2].IsAdjacentTo( boards[1].Sides[2] );
				// Move Board 1 to match Board 0
				boards[1].Sides[2].MoveLayoutTo( boards[0].Sides[2] );
				break;
			case 3:
				// aligns the board CCW going around island permiter
				// when veiwing from ocean, board-0 is on left of board-1
				boards[0].Sides[2].IsAdjacentTo( boards[1].Sides[1] );
				boards[1].Sides[2].IsAdjacentTo( boards[2].Sides[1] );
				boards[2].Sides[2].IsAdjacentTo( boards[0].Sides[1] );
				// Move Boards 1&2 to match Board 0
				boards[1].Sides[1].MoveLayoutTo( boards[0].Sides[2] );
				boards[2].Sides[2].MoveLayoutTo( boards[0].Sides[1] );
				break;
			case 4:
				// aligns the board CCW going around island permiter
				// when veiwing from ocean, board-0 is on left of board-1
				boards[0].Sides[2].IsAdjacentTo(boards[1].Sides[0] );
				boards[1].Sides[1].IsAdjacentTo(boards[2].Sides[1] );
				boards[2].Sides[2].IsAdjacentTo(boards[3].Sides[0] );
				boards[3].Sides[1].IsAdjacentTo(boards[0].Sides[1] );
				// Move Boards 1-3 to match Board 0
				boards[1].Sides[0].MoveLayoutTo( boards[0].Sides[2] );
				boards[2].Sides[1].MoveLayoutTo( boards[1].Sides[1] );//doesn't touch 0, use 1 as reference instead
				boards[3].Sides[1].MoveLayoutTo( boards[0].Sides[1] );

				break;

			default: throw new ArgumentOutOfRangeException(nameof(boards.Length),"wrong # of boards");
		}
	}
	public IEnumerable<Space> AllSpaces => Boards.SelectMany(b=>b.Spaces); // could be extension method

	// Static-Default Terrain for non-power, non-blight
	public readonly TerrainMapper Terrain = new TerrainMapper(); // Default
	public readonly TerrainMapper Terrain_ForFear = new TerrainMapper(); // Default
	public TerrainMapper Terrain_ForPower = new TerrainMapper();
	public TerrainMapper Terrain_ForBlight = new TerrainMapper();

//	public TerrainMapper Current {
//		get {
//			System.Runtime.Remoting.Messaging.CallContext.SetData( "key", "test value" );
//			return null;
//		}
//	}



}