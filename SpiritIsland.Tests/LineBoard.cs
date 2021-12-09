
using System.Linq;

namespace SpiritIsland.Tests {

	/// <summary> Test board with simplified connectivity </summary>
	class LineBoard {

		static public Board MakeBoard(){

			var board = new Board(
				new Space(Terrain.Ocean,"T0")
				,new Space(Terrain.Mountain,"T1")
				,new Space(Terrain.Wetland,"T2")
				,new Space(Terrain.Jungle,"T3")
				,new Space(Terrain.Sand,"T4")
				,new Space(Terrain.Wetland,"T5")
				,new Space(Terrain.Mountain,"T6")
				,new Space(Terrain.Sand,"T7")
				,new Space(Terrain.Jungle,"T8")
				,new Space(Terrain.Jungle,"T9")
			);
			int count = 10; // board.Spaces.Count();
			for(int i=0;i< count - 1;++i)
				board.SetNeighbors(i, i+1);
			return board;

		}

	}
}
