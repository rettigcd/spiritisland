namespace SpiritIsland.Tests;

/// <summary> Test board with simplified connectivity </summary>
class LineBoard {

	static public Board MakeBoard(){

		SpaceLayout layout = null;

		var board = new Board("T"
			,new Space1(Terrain.Ocean,   "T0", layout )
			,new Space1(Terrain.Mountain,"T1", layout )
			,new Space1(Terrain.Wetland, "T2", layout )
			,new Space1(Terrain.Jungle,  "T3", layout )
			,new Space1(Terrain.Sand,    "T4", layout )
			,new Space1(Terrain.Wetland, "T5", layout )
			,new Space1(Terrain.Mountain,"T6", layout )
			,new Space1(Terrain.Sand,    "T7", layout )
			,new Space1(Terrain.Jungle,  "T8", layout )
			,new Space1(Terrain.Jungle,  "T9", layout )
		);
		int count = 10; // board.Spaces.Count();
		for(int i=0;i< count - 1;++i)
			board.SetNeighbors(i, i+1);
		return board;

	}

}
