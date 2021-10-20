using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class Space : IOption{

		#region SetUp

		/// <summary> If adjacent to ocean, sets is-costal </summary>
		public void SetAdjacentToSpaces( params Space[] spaces ) {
			foreach(var land in spaces) {
				AddAdjacent( land );
				land.AddAdjacent( this );
			}
		}

		void AddAdjacent( Space adjacent ) {
			adjacents.Add(adjacent);
			if(adjacent.Terrain == Terrain.Ocean)
				this.IsCoastal = true;
		}

		public void Destroy() {
			// Disconnect us from neighbors
			foreach(var a in adjacents)
				a.adjacents.Remove(this);
			// Disconnect neighbors from us
			adjacents.Clear();
		}

		readonly List<Space> adjacents = new List<Space>();
		#endregion

		#region constructor

		public Space(Terrain terrain, string label,string startingItems=""){
			this.Terrain = terrain;
			this.Label = label;
			this.StartUpCounts = new StartUpCounts(startingItems);
		}

		#endregion

		public Board Board {
			get{ return board; }
			set{ if(board != null) throw new InvalidOperationException("cannot set board twice"); board = value; }
		}
		Board board;

		public string Label { get; }

		public Terrain Terrain { get; }

		public bool IsCoastal { get; set; }

		string IOption.Text => Label;

		public StartUpCounts StartUpCounts { get; }

		public IEnumerable<Space> Range( int distance ) {
			Dictionary<Space, int> shortestDistances = CalcDistances( distance );
			return shortestDistances.Keys;
		}

		public IEnumerable<Space> SpacesExactly( int distance ) {
			return distance switch {
				0 => new Space[] {this},
				1 => adjacents,
				_ => CalcDistances( distance ).Where( p => p.Value == distance ).Select( p => p.Key ),
			};
		}

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

		public IEnumerable<Space> Adjacent => adjacents;

		public override string ToString() =>Label;

		public void InitTokens( TokenCountDictionary counts ) {
			StartUpCounts initialCounts = StartUpCounts;
			counts[ Invader.City[3] ]         = initialCounts.Cities;
			counts[ Invader.Town[2] ]         = initialCounts.Towns;
			counts[ Invader.Explorer[1] ]     = initialCounts.Explorers;
			counts.Dahan[2]                   = initialCounts.Dahan;
			counts.Blight.Count               = initialCounts.Blight; // don't use AddBlight because that pulls it from the card and triggers blighted island
		}

	}

}