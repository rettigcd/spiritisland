using System.Collections.Generic;

namespace SpiritIsland {

	public class MinDistanceCalculator {
		readonly Queue<Space> spacesLessThanLimit = new();
		readonly Dictionary<Space, int> shortestDistances = new();
			
		public MinDistanceCalculator SetTargets(IEnumerable<Space> targets ) {
			foreach(var target in targets) {
				shortestDistances.Add( target, 0 );
				spacesLessThanLimit.Enqueue( target );
			}
			return this;
		}

		public MinDistanceCalculator Calculate() {
			while(spacesLessThanLimit.Count > 0) {
				// get next
				var cur = spacesLessThanLimit.Dequeue();
				int neighborDist = shortestDistances[cur] + 1;
				// add neighbors to dictionary and evaluated its neighbors
				foreach(var a in cur.Adjacent) {
					if(!shortestDistances.ContainsKey( a ) || neighborDist < shortestDistances[a]) {
						shortestDistances[a] = neighborDist;
						spacesLessThanLimit.Enqueue( a );
					}
				}
			}
			return this;
		}

		public int this[Space space] => shortestDistances[space];

	}


}
