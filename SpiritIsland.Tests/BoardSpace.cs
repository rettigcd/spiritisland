using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Tests {

	public enum Terrain {None, Jungle, Mountain, Ocean, Sand, Wetland };



	public class BoardSpace{

		static public BoardSpace[] GetBoardA() {
			var spaces = new BoardSpace[]{
				new BoardSpace(Terrain.Ocean,"0")
				,new BoardSpace(Terrain.Mountain,"1") // 
				,new BoardSpace(Terrain.Wetland,"2")  // city, dahan
				,new BoardSpace(Terrain.Jungle,"3")   // 2 dahan
				,new BoardSpace(Terrain.Sand,"4")     // blight
				,new BoardSpace(Terrain.Wetland,"5")
				,new BoardSpace(Terrain.Mountain,"6") // 1 dahan
				,new BoardSpace(Terrain.Sand,"7")     // 2 dahan
				,new BoardSpace(Terrain.Jungle,"8")   // town
			};

			SetNeighbors(spaces,0, 1,2,3);
			SetNeighbors(spaces,1, 2,4,5,6);
			SetNeighbors(spaces,2, 3,4);
			SetNeighbors(spaces,3, 4);
			SetNeighbors(spaces,4, 5);
			SetNeighbors(spaces,5, 6,7,8);
			SetNeighbors(spaces,6, 8);
			SetNeighbors(spaces,7, 8);
			return spaces;
		}

		static void SetNeighbors(BoardSpace[] spaces, int srcIndex, params int[] neighborIndex){
			spaces[srcIndex].SetAdjacentTo(neighborIndex.Select(i=>spaces[i]).ToArray());
		}

		static public BoardSpace[] GetBoardB() {
			var spaces = new BoardSpace[]{
				new BoardSpace(Terrain.Ocean,"0")
				,new BoardSpace(Terrain.Wetland,"1")  // 1 dahan
				,new BoardSpace(Terrain.Mountain,"2") // city
				,new BoardSpace(Terrain.Sand,"3")     // 2 dahan
				,new BoardSpace(Terrain.Jungle,"4")   // blight
				,new BoardSpace(Terrain.Sand,"5")
				,new BoardSpace(Terrain.Wetland,"6")  // 1 town
				,new BoardSpace(Terrain.Mountain,"7") // 1 dahan
				,new BoardSpace(Terrain.Jungle,"8")   // 2 dahan
			};

			SetNeighbors(spaces,0, 1,2,3);
			SetNeighbors(spaces,1, 2,4,5,6);
			SetNeighbors(spaces,2, 3,4);
			SetNeighbors(spaces,3, 4);
			SetNeighbors(spaces,4, 5,7);
			SetNeighbors(spaces,5, 6,7);
			SetNeighbors(spaces,6, 7,8);
			SetNeighbors(spaces,7, 8);

			return spaces;
		}


		public string Label {get;}
		public Terrain Terrain {get;}

		public BoardSpace(Terrain terrain=Terrain.None, string label=null){
			this.Terrain = terrain;
			this.Label = label;
			_distanceTo.Add(this,0);
		}

		public bool IsCostal { get; set; }

		/// <summary>
		/// If adjacent to ocean, sets is-costal
		/// </summary>
		/// <param name="spaces"></param>
		public void SetAdjacentTo( params BoardSpace[] spaces ) {
			foreach(var land in spaces) {
				SetNeighbor( land );
				land.SetNeighbor( this );
			}

			_highestDistanceCalculated = 1;
		}

		void SetNeighbor( BoardSpace land ) {
			_distanceTo.Add( land, 1 );
			if(land.Terrain == Terrain.Ocean)
				this.IsCostal = true;
		}

		public IEnumerable<BoardSpace> SpacesWithin( int distance ) {
			CalculateDistancesUpTo( distance );
			return _distanceTo.Where( p => p.Value <= distance ).Select( p => p.Key );
		}

		public IEnumerable<BoardSpace> SpacesExactly( int distance ) {
			CalculateDistancesUpTo( distance );
			return _distanceTo.Where( p => p.Value == distance ).Select( p => p.Key );
		}

		public override string ToString() {
			return Label.ToString();
		}

		void CalculateDistancesUpTo( int distance ) {
			while(_highestDistanceCalculated < distance) {
				int nextLevel = _highestDistanceCalculated + 1;
				var newSpaces = _distanceTo
					.Where( p => p.Value == _highestDistanceCalculated )
					.SelectMany( p => p.Key.SpacesExactly( 1 ) )
					.Distinct()
					.Where( space => !_distanceTo.ContainsKey( space )  // new space
						|| _distanceTo[space] > nextLevel ) // has a longer path than this path
					.ToList(); // force iteration so we can make changes to _distanceTo
				foreach(var newSpace in newSpaces)
					_distanceTo[newSpace] = nextLevel;
				_highestDistanceCalculated = nextLevel;
			}
		}

		readonly Dictionary<BoardSpace,int> _distanceTo = new Dictionary<BoardSpace, int>();
		int _highestDistanceCalculated = 0;
	}

}