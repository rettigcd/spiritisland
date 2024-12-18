namespace SpiritIsland.Tests;

/// <summary> Test board with simplified connectivity </summary>
class LineBoard {

	static public Board MakeBoard(){

		var board = new Board("T"
			, BoardOrientation.Home
			,new SSS(Terrain.Ocean,   "T0")
			,new SSS(Terrain.Mountain,"T1")
			,new SSS(Terrain.Wetland, "T2")
			,new SSS(Terrain.Jungle,  "T3")
			,new SSS(Terrain.Sands,   "T4")
			,new SSS(Terrain.Wetland, "T5")
			,new SSS(Terrain.Mountain,"T6")
			,new SSS(Terrain.Sands,   "T7")
			,new SSS(Terrain.Jungle,  "T8")
			,new SSS(Terrain.Jungle,  "T9")
		);
		int count = 10; // board.Spaces.Count();
		for(int i=0;i< count - 1;++i)
			board.SetNeighbors(i, i+1);
		return board;

	}

}
