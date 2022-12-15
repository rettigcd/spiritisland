using System.Drawing;

namespace SpiritIsland;

static public class Polygons {

	// Performs the even-odd-rule Algorithm to find out whether a point is in a given polygon.
	// This runs in O(n) where n is the number of edges of the polygon.
	// return whether the point is in the polygon (not on the edge, just turn < into <= and > into >= for that)
	static public bool IsInside( PointF[] polygon, PointF p ) => IsInside( polygon, p.X, p.Y );
	static public bool IsInside( PointF[] polygon, float pX, float pY ) {

		// A point is in a polygon if a line from the point to infinity crosses the polygon an odd number of times
		bool odd = false;

		// For each edge (In this case for each point of the polygon and the previous one)
		// Starting with the edge from the last to the first node
		for(int i = 0, j = polygon.Length - 1; i < polygon.Length; i++) {
			PointF pi = polygon[i], pj = polygon[j];

			// If a line from the point into infinity crosses this edge
			if( // One point needs to be ABOVE, && one BELOW-OR-ON our y coordinate
				((pY < pi.Y) != (pY < pj.Y))
				// and the edge doesn't cross our Y corrdinate before our x coordinate
				// (but between our x coordinate and infinity)
				&& pX < (pj.X - pi.X) * (pY - pi.Y) / (pj.Y - pi.Y) + pi.X
			) odd = !odd;

			j = i; // next
		}

		return odd; //If the number of crossings was odd, the point is in the polygon
	}

	static public float DistanceFromPolygon( PointF[] polygon, PointF p ) {
		float min_2 = float.MaxValue; // min distanced squared (^2)

		// For each edge (In this case for each point of the polygon and the previous one)
		// Starting with the edge from the last to the first node
		for(int i = 0, j = polygon.Length - 1; i < polygon.Length; i++) {
			PointF closestPoint = GetClosestPointOnSegment(p,polygon[i],polygon[j]);
			float dx = closestPoint.X - p.X;
			float dy = closestPoint.Y - p.Y;
			min_2 = Math.Min(min_2, dx*dx+dy*dy );
			j = i; // next
		}

		return (float)Math.Sqrt(min_2);
	}

	private static PointF GetClosestPointOnSegment( PointF p, PointF end1, PointF end2 ) {
		float A = p.X - end1.X;
		float B = p.Y - end1.Y;
		float C = end2.X - end1.X;
		float D = end2.Y - end1.Y;

		float dot = A * C + B * D;
		float len_sq = C * C + D * D;
		float param = len_sq != 0
			? dot / len_sq
			: -1; // in case of 0 length line

		// Find point on line to measure to.
		PointF closest
			= (param < 0) ? end1 // is beyond end-1, use end-1
			: (param > 1) ? end2 // is beyond end-2, use end-2
			: new PointF( end1.X + param * C, end1.Y + param * D );// is between end 1 & 2, find point on segment to use
		return closest;
	}

	static public T[] JoinAdjacentPolgons<T>( T[] region0, T[] region1 ) {

		// find a corner that is not common
		int i = 0;
		while(i < region0.Length && region1.Contains( region0[i] )) ++i;
		if(i == region0.Length) return Array.Empty<T>(); // all corners are in common 1 space suround the other

		// advance to corner in common, aka find start0 and start1
		int endOfCommonPointsOnPoly2;
		while((endOfCommonPointsOnPoly2 = Array.IndexOf( region1, region0[i] )) == -1)
			i = (i + 1) % region0.Length;

		// find the last point in common
		int endOfCommonPointsOnPoly1 = 0;
		int countOfPointsInCommon = 0;
		while(Array.IndexOf( region1, region0[i] ) != -1) {
			endOfCommonPointsOnPoly1 = i;
			++countOfPointsInCommon;
			i = (i + 1) % region0.Length;
		}

		var mergedArray = new T[region0.Length + region1.Length - countOfPointsInCommon * 2 + 2];

		int k = 0, numFrom0 = region0.Length - countOfPointsInCommon + 1;
		for(i = 0; i < numFrom0; ++i)
			mergedArray[k++] = region0[(endOfCommonPointsOnPoly1 + i) % region0.Length];

		int numFrom1 = region1.Length - countOfPointsInCommon + 1;
		for(i = 0; i < numFrom1; ++i)
			mergedArray[k++] = region1[(endOfCommonPointsOnPoly2 + i) % region1.Length];

		return mergedArray;
	}

}

