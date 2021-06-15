using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public enum Terrain {None, Jungle, Mountain, Ocean, Sand, Wetland };

	public class Space : IOption{

		#region Game-Static
		public string Label {get;}
		public Terrain Terrain {get;}
		public bool IsCostal { get; set; }

		string IOption.Text => Label;

		readonly Dictionary<Space,int> _distanceTo = new Dictionary<Space, int>();

		#endregion

		#region SetUp
		int _highestDistanceCalculated = 0;
		void CalculateDistancesUpTo( int distance ) {
			while(_highestDistanceCalculated < distance) {
				int nextLevel = _highestDistanceCalculated + 1;
				var newSpaces = _distanceTo
					.Where( p => p.Value == _highestDistanceCalculated )
					.SelectMany( p => p.Key._distanceTo.Where(n=>n.Value==1).Select(q=>q.Key) ) // must get this directly
					.Distinct()
					.Where( space => !_distanceTo.ContainsKey( space )  // new space
						|| _distanceTo[space] > nextLevel ) // has a longer path than this path
					.ToList(); // force iteration so we can make changes to _distanceTo
				foreach(var newSpace in newSpaces)
					_distanceTo[newSpace] = nextLevel;
				_highestDistanceCalculated = nextLevel;
			}
		}
		/// <summary> If adjacent to ocean, sets is-costal </summary>
		public void SetAdjacentTo( params Space[] spaces ) {
			foreach(var land in spaces) {
				SetNeighbor( land );
				land.SetNeighbor( this );
			}

			_highestDistanceCalculated = 1;
		}

		void SetNeighbor( Space land ) {
			_distanceTo.Add( land, 1 ); // @@@ 
			if(land.Terrain == Terrain.Ocean)
				this.IsCostal = true;
		}
		#endregion

		public Space(Terrain terrain=Terrain.None, string label=null){
			this.Terrain = terrain;
			this.Label = label;
			_distanceTo.Add(this,0);
		}

		public IEnumerable<Space> SpacesWithin( int distance ) {
			CalculateDistancesUpTo( distance );
			return _distanceTo.Where( p => p.Value <= distance ).Select( p => p.Key );
		}

		public IEnumerable<Space> SpacesExactly( int distance ) {
			CalculateDistancesUpTo( distance );
			return _distanceTo.Where( p => p.Value == distance ).Select( p => p.Key );
		}

		public override string ToString() {
			return Label.ToString();
		}

	}

}