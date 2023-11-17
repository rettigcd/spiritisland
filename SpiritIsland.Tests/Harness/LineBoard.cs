namespace SpiritIsland.Tests;

/// <summary> Test board with simplified connectivity </summary>
class LineBoard {

	static public Board MakeBoard(){

		var board = new Board("T"
			, BoardOrientation.Home
			,new Space1(Terrain.Ocean,  "T0")
			,new Space1(Terrain.Mountain,"T1")
			,new Space1(Terrain.Wetland, "T2")
			,new Space1(Terrain.Jungle,  "T3")
			,new Space1(Terrain.Sands,    "T4")
			,new Space1(Terrain.Wetland, "T5")
			,new Space1(Terrain.Mountain,"T6")
			,new Space1(Terrain.Sands,    "T7")
			,new Space1(Terrain.Jungle,  "T8")
			,new Space1(Terrain.Jungle,  "T9")
		);
		int count = 10; // board.Spaces.Count();
		for(int i=0;i< count - 1;++i)
			board.SetNeighbors(i, i+1);
		return board;

	}

}
