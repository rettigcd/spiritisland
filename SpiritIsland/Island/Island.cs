using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
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
					boards[0].Sides[2].AlignTo( boards[1].Sides[2] ); 
					break;
				case 3:
					// aligns the board CCW going around island permiter
					// when veiwing from ocean, board-0 is on left of board-1
					boards[0].Sides[2].AlignTo( boards[1].Sides[1] ); 
					boards[1].Sides[2].AlignTo( boards[2].Sides[1] ); 
					boards[2].Sides[2].AlignTo( boards[0].Sides[1] );
					break;
				case 4:
					// aligns the board CCW going around island permiter
					// when veiwing from ocean, board-0 is on left of board-1
					boards[0].Sides[2].AlignTo( boards[1].Sides[0] ); 
					boards[1].Sides[1].AlignTo( boards[2].Sides[1] ); 
					boards[2].Sides[2].AlignTo( boards[3].Sides[0] );
					boards[3].Sides[1].AlignTo( boards[0].Sides[1] ); 
					break;
				default: throw new ArgumentOutOfRangeException(nameof(boards.Length),"wrong # of boards");
			}
		}
		public IEnumerable<Space> AllSpaces => Boards.SelectMany(b=>b.Spaces); // could be extension method

		public TerrainMapper TerrainMapFor(Cause cause) => cause switch {
			Cause.Power  => Terrain_ForPowerAndBlight,
			Cause.Blight => Terrain_ForPowerAndBlight,
			_            => Terrain
		};

		readonly TerrainMapper Terrain = new TerrainMapper();
		public TerrainMapper Terrain_ForPowerAndBlight = new TerrainMapper();

	}

}
