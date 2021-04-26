using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Tests {

	// The collection of 1+ boards assembled, shall be called the 'Island'

	public class Board {

		#region factories

		static public Board GetBoardA() {
			var tile = new Board(
				new Space(Terrain.Ocean,"A0")
				,new Space(Terrain.Mountain,"A1") // 
				,new Space(Terrain.Wetland,"A2")  // city, dahan
				,new Space(Terrain.Jungle,"A3")   // 2 dahan
				,new Space(Terrain.Sand,"A4")     // blight
				,new Space(Terrain.Wetland,"A5")
				,new Space(Terrain.Mountain,"A6") // 1 dahan
				,new Space(Terrain.Sand,"A7")     // 2 dahan
				,new Space(Terrain.Jungle,"A8")   // town
			);

			tile.SetNeighbors(0, 1,2,3);
			tile.SetNeighbors(1, 2,4,5,6);
			tile.SetNeighbors(2, 3,4);
			tile.SetNeighbors(3, 4);
			tile.SetNeighbors(4, 5);
			tile.SetNeighbors(5, 6,7,8);
			tile.SetNeighbors(6, 8);
			tile.SetNeighbors(7, 8);

			// sides of the board are # starting at the ocean and moving around the board clockwise
			tile.DefineSide(1,6,8).BreakAt(3,7);	// Side 0
			tile.DefineSide(8,7).BreakAt(5);		// Side 1
			tile.DefineSide(7,5,4,3).BreakAt(1,3,7);// Side 2

			return tile;
		}

		static public Board GetBoardB() {
			var tile = new Board(
				new Space(Terrain.Ocean,"B0")
				,new Space(Terrain.Wetland,"B1")  // 1 dahan
				,new Space(Terrain.Mountain,"B2") // city
				,new Space(Terrain.Sand,"B3")     // 2 dahan
				,new Space(Terrain.Jungle,"B4")   // blight
				,new Space(Terrain.Sand,"B5")
				,new Space(Terrain.Wetland,"B6")  // 1 town
				,new Space(Terrain.Mountain,"B7") // 1 dahan
				,new Space(Terrain.Jungle,"B8")   // 2 dahan
			);

			tile.SetNeighbors(0, 1,2,3);
			tile.SetNeighbors(1, 2,4,5,6);
			tile.SetNeighbors(2, 3,4);
			tile.SetNeighbors(3, 4);
			tile.SetNeighbors(4, 5,7);
			tile.SetNeighbors(5, 6,7);
			tile.SetNeighbors(6, 7,8);
			tile.SetNeighbors(7, 8);

			// sides of the board are # starting at the ocean and moving around the board clockwise
			tile.DefineSide(1,6,8).BreakAt(1,7);	// Side 0
			tile.DefineSide(8,7).BreakAt(5);		// Side 1
			tile.DefineSide(7,4,3).BreakAt(1,5);	// Side 2

			return tile;
		}

		static public Board GetBoardC() {
			var tile = new Board(
				new Space(Terrain.Ocean,"C0")
				,new Space(Terrain.Jungle,"C1")   // 1 dahan
				,new Space(Terrain.Sand,"C2")     // city
				,new Space(Terrain.Mountain,"C3") // 2 dahan
				,new Space(Terrain.Jungle,"C4")   
				,new Space(Terrain.Wetland,"C5")  // 2 dahan, blight
				,new Space(Terrain.Sand,"C6")     // 1 dahan
				,new Space(Terrain.Mountain,"C7") // 1 town
				,new Space(Terrain.Wetland,"C8")
			);

			tile.SetNeighbors(0, 1,2,3);
			tile.SetNeighbors(1, 2,5,6);
			tile.SetNeighbors(2, 3,4,5);
			tile.SetNeighbors(3, 4);
			tile.SetNeighbors(4, 5,7);
			tile.SetNeighbors(5, 6,7);
			tile.SetNeighbors(6, 7,8);
			tile.SetNeighbors(7, 8);

			// sides of the board are # starting at the ocean and moving around the board clockwise
			tile.DefineSide(1,6,8).BreakAt(3,7);	// Side 0
			tile.DefineSide(8,7,4).BreakAt(7,11);	// Side 1
			tile.DefineSide(4,3).BreakAt(7);		// Side 2

			return tile;
		}

		static public Board GetBoardD() {
			var tile = new Board(
				new Space(Terrain.Ocean,"D0")
				,new Space(Terrain.Wetland,"D1")   // 2 dahan
				,new Space(Terrain.Jungle,"D2")    // city, 1 dahan
				,new Space(Terrain.Wetland,"D3")   
				,new Space(Terrain.Sand,"D4")   
				,new Space(Terrain.Mountain,"D5")  // 1 dahan, blight
				,new Space(Terrain.Jungle,"D6")    
				,new Space(Terrain.Sand,"D7")      // 1 town, 2 dahan
				,new Space(Terrain.Mountain,"D8")
			);

			tile.SetNeighbors(0, 1,2,3);
			tile.SetNeighbors(1, 2,5,7,8);
			tile.SetNeighbors(2, 3,4,5);
			tile.SetNeighbors(3, 4);
			tile.SetNeighbors(4, 5,6);
			tile.SetNeighbors(5, 6,7);
			tile.SetNeighbors(6, 7);
			tile.SetNeighbors(7, 8);

			// sides of the board are # starting at the ocean and moving around the board clockwise
			tile.DefineSide(1,8).BreakAt(11);		// Side 0
			tile.DefineSide(8,7,6).BreakAt(5,11);	// Side 1
			tile.DefineSide(6,4,3).BreakAt(3,9);	// Side 2

			return tile;
		}

		#endregion

		public Space[] spaces;

		public ITileSide[] Sides => this.sides.ToArray();

		#region constructor
		public Board(params Space[] spaces){
			this.spaces = spaces;
		}

		#endregion

		TileSide DefineSide(params int[] spaceIndexes){
			var side = new TileSide(spaceIndexes.Select(i=>this.spaces[i]).ToArray());
			this.sides.Add(side);
			return side;
		}

		void SetNeighbors(int srcIndex, params int[] neighborIndex){
			spaces[srcIndex].SetAdjacentTo(neighborIndex.Select(i=>spaces[i]).ToArray());
		}

		readonly List<ITileSide> sides = new List<ITileSide>();

		#region TileSide

		public interface ITileSide {
			public void AlignTo(ITileSide other);
		}

		class TileSide : ITileSide {
			public TileSide(params Space[] spaces){
				this.spaces = spaces;
			}

			public void BreakAt(params int[] breakPoints){
				if(breakPoints.Length != spaces.Length-1)
					throw new System.InvalidOperationException("there must be 1 fewer break point than spaces");
				this.breakPoints = breakPoints.ToList();
				this.breakPoints.Insert(0,0);
				this.breakPoints.Add(13);
			}
			
			public void AlignTo(ITileSide otherSide){
				TileSide other = (TileSide)otherSide;
				
				// reverse other
				var otherSpaces = other.spaces.Reverse().ToArray();
				var otherBreakPoints = other.breakPoints.Select(i=>13-i).Reverse().ToList();

				var thisSpaces = this.spaces;
				var thisBreakPoints = this.breakPoints.ToList();

				int thisIndex = 0;
				int otherIndex = 0;
				do{
					// current territories are adjacent
					thisSpaces[thisIndex].SetAdjacentTo(otherSpaces[otherIndex]);
					// advance whichever board is shorter 
					if(thisBreakPoints[thisIndex+1] < otherBreakPoints[otherIndex+1])
						thisIndex++;
					else
						otherIndex++;
				} while(thisIndex<thisBreakPoints.Count-2 || otherIndex<otherBreakPoints.Count-2 );
				thisSpaces[thisIndex].SetAdjacentTo(otherSpaces[otherIndex]);
			}

			readonly Space[] spaces;
			List<int> breakPoints;
		}

		#endregion

	}

}