using SpiritIsland;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public abstract class Space : IOption {

		readonly List<Space> adjacents = new List<Space>();
		Board board;

		protected Space(string label) {
			this.Label = label;
		}

		public Board Board {
			get { return board; }
			set { if(board != null) throw new InvalidOperationException( "cannot set board twice" ); board = value; }
		}

		public string Label { get; }
		public bool IsOcean => Is( Terrain.Ocean );
		public bool IsSand => Is( Terrain.Sand );
		public bool IsMountain => Is( Terrain.Mountain );
		public bool IsJungle => Is( Terrain.Jungle );
		public bool IsWetland => Is( Terrain.Wetland );

		public bool IsCoastal { get; set; }

		public IEnumerable<Space> Adjacent => adjacents;

		public string Text => Label;

		public void Disconnect() {
			// Remove us from neighbors adjacent list
			foreach(var a in adjacents)
				a.adjacents.Remove( this );
			// Remove neighbors from our list.
			adjacents.Clear();
		}

		public abstract bool Is( Terrain terrain );
		public abstract bool IsOneOf( params Terrain[] options );

		public IEnumerable<Space> Range( int distance ) {
			Dictionary<Space, int> shortestDistances = CalcDistances( distance );
			return shortestDistances.Keys;
		}


		/// <summary> If adjacent to ocean, sets is-costal </summary>
		public void SetAdjacentToSpaces( params Space[] spaces ) {
			foreach(var land in spaces) {
				Connect( land );
				land.Connect( this );
			}
		}

		public IEnumerable<Space> SpacesExactly( int distance ) {
			return distance switch {
				0 => new Space[] { this },
				1 => adjacents,
				_ => CalcDistances( distance ).Where( p => p.Value == distance ).Select( p => p.Key ),
			};
		}

		public override string ToString() => Label;

		Dictionary<Space, int> CalcDistances( int distance ) {

			Queue<Space> spacesLessThanLimit = new Queue<Space>();
			// collects distances that are <= distance
			var shortestDistances = new Dictionary<Space, int> { { this, 0 } };

			if(distance > 0)
				spacesLessThanLimit.Enqueue( this );

			while(spacesLessThanLimit.Count > 0) {
				var cur = spacesLessThanLimit.Dequeue();
				int neighborDist = shortestDistances[cur] + 1;
				bool neighborIsLessThanLimit = neighborDist < distance;
				foreach(var a in cur.adjacents) {
					if(shortestDistances.ContainsKey( a ) && shortestDistances[a] <= neighborDist)
						continue;
					shortestDistances[a] = neighborDist;
					if(neighborIsLessThanLimit)
						spacesLessThanLimit.Enqueue( a );
				}

			}

			return shortestDistances;
		}

		void Connect( Space adjacent ) {
			adjacents.Add( adjacent );
			if(adjacent.IsOcean)
				this.IsCoastal = true;
		}
	}
}