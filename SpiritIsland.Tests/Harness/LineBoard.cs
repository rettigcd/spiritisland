namespace SpiritIsland.Tests;

/// <summary> Test board with simplified connectivity </summary>
class LineBoard {

	static public Board MakeBoard(){

		var board = new Board("T"
			, BoardOrientation.Home
			,new SingleSpaceSpec(Terrain.Ocean,  "T0")
			,new SingleSpaceSpec(Terrain.Mountain,"T1")
			,new SingleSpaceSpec(Terrain.Wetland, "T2")
			,new SingleSpaceSpec(Terrain.Jungle,  "T3")
			,new SingleSpaceSpec(Terrain.Sands,    "T4")
			,new SingleSpaceSpec(Terrain.Wetland, "T5")
			,new SingleSpaceSpec(Terrain.Mountain,"T6")
			,new SingleSpaceSpec(Terrain.Sands,    "T7")
			,new SingleSpaceSpec(Terrain.Jungle,  "T8")
			,new SingleSpaceSpec(Terrain.Jungle,  "T9")
		);
		int count = 10; // board.Spaces.Count();
		for(int i=0;i< count - 1;++i)
			board.SetNeighbors(i, i+1);
		return board;

	}

}
