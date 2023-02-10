namespace SpiritIsland;

public interface HasNeighbors<T> {
	IEnumerable<T> Adjacent_All { get; }
}

public static class HasNeighborsExtensions {

	// Calculates distances using spaces that are not "In Play", but Does NOT use spaces that are in Stasis

	public static Dictionary<T, int> CalcDistances<T>( this T starter, int maxDistanceToFind ) where T : HasNeighbors<T> {

		Queue<T> spacesLessThanLimit = new Queue<T>();
		// collects distances that are <= distance
		var shortestDistances = new Dictionary<T, int> { { starter, 0 } };

		if(0 < maxDistanceToFind)
			spacesLessThanLimit.Enqueue( starter );

		while(spacesLessThanLimit.Any()) {
			var cur = spacesLessThanLimit.Dequeue();
			int neighborDist = shortestDistances[cur] + 1;
			bool neighborIsLessThanLimit = neighborDist < maxDistanceToFind;
			foreach(var a in cur.Adjacent_All) {
				if(shortestDistances.ContainsKey( a ) && shortestDistances[a] <= neighborDist)
					continue;
				shortestDistances[a] = neighborDist;
				if(neighborIsLessThanLimit)
					spacesLessThanLimit.Enqueue( a );
			}

		}

		return shortestDistances;
	}

}
