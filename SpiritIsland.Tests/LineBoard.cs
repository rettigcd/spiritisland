using System;
using System.Collections.Generic;
using System.Text;

namespace SpiritIsland.Tests {
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
			for(int i=0;i<board.SpaceCount-1;++i)
				board.SetNeighbors(i, i+1);
			return board;

		}

	}
}
