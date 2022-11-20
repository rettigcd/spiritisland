using System.Data;
using System.Drawing;

namespace SpiritIsland;

/// <summary>
/// The normalized 2D layout of 1 board
/// </summary>
public class BoardLayout {

	public PointF[] perimeter; // for connecting boards
	public SpaceLayout[] Spaces;

	public PointF[] boardCorners = new PointF[] {
		topLeftCorner,
		topRightCorner, // Side-0 origin (after 180 rotation)
		bottomRightCorner, // Side-1 origin after -60 rotation
		origin, // Side-2 origin (no rotation)
	};

	public double SideRotationDegrees(int i) {
		var from = boardCorners[i];
		var to = boardCorners[i+1];
		return Math.Atan2(to.Y-from.Y,to.X-from.X) * 180 / Math.PI;
	}

	public void ReMap( PointMapper mapper ) {

		// perimeter
		for(int i=0;i<perimeter.Length;++i)
			perimeter[i] = mapper.Map( perimeter[i] );

		foreach(var space in this.Spaces)
			space.ReMap(mapper);

		// origin
		for(int i = 0; i< boardCorners.Length; ++i) {
			var src = boardCorners[i];
			var dst = mapper.Map( src );
			boardCorners[i] = dst;
		}
	}

	public RectangleF CalcExtents() {
		float minX = float.MaxValue;
		float minY = float.MaxValue;
		float maxX = float.MinValue;
		float maxY = float.MinValue;
		foreach(var p in perimeter) {
			if(p.X < minX) minX = p.X;
			if(p.Y < minY) minY = p.Y;
			if(p.X > maxX) maxX = p.X;
			if(p.Y > maxY) maxY = p.Y;
		}
		return new RectangleF( minX, minY, maxX - minX, maxY - minY );
	}


	#region private static

	static BoardLayout() {

		static PointF inflect( PointF orig ) => new PointF( 1.0f - orig.X, -orig.Y );
		static PointF Translate( PointF orig, float deltaX, float deltaY )
			=> new PointF( orig.X + deltaX, orig.Y + deltaY );
		static PointF RotateAboutOrigin( PointF orig, double thetaDegrees ) {
			double theta = thetaDegrees * Math.PI / 180;
			double sinTheta = Math.Sin( theta );
			double cosTheta = Math.Cos( theta );
			return new PointF(
				(float)(orig.X * cosTheta - orig.Y * sinTheta),
				(float)(orig.X * sinTheta + orig.Y * cosTheta)
			);
		}

		boardHeight = (float)(0.5 * Math.Sqrt( 3 ));
		float xDelta = .06f;
		float yOffset = xDelta; // could be anything..
		float xOff = 0.09f;

		origin = new PointF( 0f, 0f );
		topLeftCorner = new PointF( 0.5f, boardHeight );
		bottomRightCorner = new PointF( 1f, 0f );
		topRightCorner = new PointF( 1.5f, boardHeight );

		// bottom
		bot = new PointF[12];
		bot[0] = new PointF( xOff + xDelta, 0 );
		bot[1] = new PointF( xOff + xDelta * 2, -yOffset );
		bot[2] = new PointF( xOff + xDelta * 3, -yOffset );
		bot[3] = new PointF( xOff + xDelta * 4, -yOffset );
		bot[4] = new PointF( xOff + xDelta * 5, 0 );
		bot[5] = new PointF( xOff + xDelta * 6, 0 );
		bot[6] = inflect( bot[5] );
		bot[7] = inflect( bot[4] );
		bot[8] = inflect( bot[3] );
		bot[9] = inflect( bot[2] );
		bot[10] = inflect( bot[1] );
		bot[11] = inflect( bot[0] );

		// top
		top = bot
			.Select( orig => Translate( orig, 0.5f, boardHeight ) )
			.ToArray();

		// right
		rig = bot
			.Select( orig => Translate( RotateAboutOrigin( orig, 240.0 ), 1.5f, boardHeight ) )
			.ToArray();

	}

	// Corners
	static public readonly float boardHeight;
	static readonly PointF origin;
	static readonly PointF topLeftCorner;
	static readonly PointF bottomRightCorner;
	static readonly PointF topRightCorner;
	static readonly PointF[] bot;
	static readonly PointF[] top;
	static readonly PointF[] rig;

	#endregion

	static PointF FindCenterOfSpacePoints( PointF[] spacePoints ) {
		float maxX = -1000f, maxY = -1000f, minX = 1000f, minY = 1000f;
		foreach(var p in spacePoints) {
			if(p.X < minX) minX = p.X;
			if(p.Y < minY) minY = p.Y;
			if(p.X > maxX) maxX = p.X;
			if(p.Y > maxY) maxY = p.Y;
		}
		var center = new PointF( (minX + maxX) * .5f, (minY + maxY) * .5f );
		return center;
	}

	static public BoardLayout BoardA() {

		// Edge points with ocean
		var _01 = new PointF( .4f, .846f );
		var _03 = new PointF( 0.01f, .15f );
		// Internal Points
		var _012 = new PointF( .48f, .756f );
		var _023 = new PointF( .18f, .22f );
		var _124 = new PointF( .55f, .4f );
		var _145 = new PointF( .69f, .45f );
		var _156 = new PointF( .79f, .57f );
		var _234 = new PointF( .45f, .2f );
		var _568 = new PointF( 1f, .67f );
		var _578 = new PointF( 1.17f, .62f );
		// style points
		var _0 = new PointF( .14f, .67f );
		var _02 = new PointF( .34f, .67f );
		var _45 = new PointF( .81f, .35f );
		var _56 = new PointF( .94f, .54f );

		var a0Points = new PointF[] { _01, _012, _02, _023, _03, _0 };
		var a1Points = new PointF[] { _01, topLeftCorner, top[0], top[1], top[2], _156, _145, _124, _012 };
		var a2Points = new PointF[] { _012, _124, _234, _023, _02 };
		var a3Points = new PointF[] { _03, _023, _234, bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin };
		var a4Points = new PointF[] { bot[9], bot[8], bot[7], bot[6], bot[5], _234, _124, _145, _45 };
		var a5Points = new PointF[] { bot[11], bot[10], bot[9], _45, _145, _156, _56, _568, _578, };
		var a6Points = new PointF[] { top[2], top[3], top[4], top[5], top[6], _568, _56, _156, };
		var a7Points = new PointF[] { rig[4], rig[5], rig[6], rig[7], rig[8], rig[9], rig[10], rig[11], bottomRightCorner, bot[11], _578, };
		var a8Points = new PointF[] { top[6], top[7], top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], _578, _568, };

		var layout = new BoardLayout {
			perimeter = new PointF[] {
				origin,
				_03,_0,_01,
				topLeftCorner,
				top[0], top[1], top[2], top[3], top[4], top[5], top[6], top[7], top[8], top[9], top[10], top[11],
				topRightCorner,
				rig[0],rig[1],rig[2],rig[3],rig[4],rig[5],rig[6],rig[7],rig[8],rig[9],rig[10],rig[11],
				bottomRightCorner,
				bot[11], bot[10], bot[ 9 ], bot[8], bot[7], bot[6], bot[5], bot[4], bot[3], bot[2], bot[1], bot[0],
			},
			Spaces = new SpaceLayout[] { 
				new SpaceLayout(a0Points), 
				new SpaceLayout(a1Points), 
				new SpaceLayout(a2Points), 
				new SpaceLayout(a3Points), 
				new SpaceLayout(a4Points), 
				new SpaceLayout(a5Points), 
				new SpaceLayout(a6Points), 
				new SpaceLayout(a7Points), 
				new SpaceLayout(a8Points)
			}
		};
		return layout;
	}

	static public BoardLayout BoardB() {

		// Edge points with ocean
		var _01 = new PointF( .4f, .846f );
		var _03 = new PointF( 0.01f, .15f );
		// Internal Points
		var _012 = new PointF( .36f, .6f );
		var _023 = new PointF( .15f, .20f );
		var _156 = new PointF( .7f, .7f );
		var _145 = new PointF( .67f, .58f );
		var _124 = new PointF( .53f, .57f );
		var _234 = new PointF( .57f, .17f );
		var _457 = new PointF( .91f, .27f );
		var _567 = new PointF( 1.05f, .5f );
		var _678 = new PointF( 1.15f, .6f );
		// style points
		var _0 = new PointF( .14f, .67f );

		var b0Points = new PointF[] { _01, _012, _023, _03, _0 };
		var b1Points = new PointF[] { _01, topLeftCorner, top[0], _156, _145, _124, _012 };
		var b2Points = new PointF[] { _012, _124, _234, _023 };
		var b3Points = new PointF[] { _03, _023, _234, bot[7], bot[6], bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin };
		var b4Points = new PointF[] { bot[11], bot[10], bot[9], bot[8], bot[7], _234, _124, _145, _457 };
		var b5Points = new PointF[] { _145, _156, _567, _457 };
		var b6Points = new PointF[] { top[0], top[1], top[2], top[3], top[4], top[5], top[6], _678, _567, _156, };
		var b7Points = new PointF[] { rig[4], rig[5], rig[6], rig[7], rig[8], rig[9], rig[10], rig[11], bottomRightCorner, bot[11], _457, _567, _678 };
		var b8Points = new PointF[] { top[6], top[7], top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], _678 };

		var layout = new BoardLayout {
			perimeter = new PointF[] {
				origin,
				_03,_0,_01,
				topLeftCorner,
				top[0], top[1], top[2], top[3], top[4], top[5], top[6], top[7], top[8], top[9], top[10], top[11],
				topRightCorner,
				rig[0],rig[1],rig[2],rig[3],rig[4],rig[5],rig[6],rig[7],rig[8],rig[9],rig[10],rig[11],
				bottomRightCorner,
				bot[11], bot[10], bot[ 9 ], bot[8], bot[7], bot[6], bot[5], bot[4], bot[3], bot[2], bot[1], bot[0],
			},
			Spaces = new SpaceLayout[] {
				new SpaceLayout(b0Points),
				new SpaceLayout(b1Points),
				new SpaceLayout(b2Points),
				new SpaceLayout(b3Points),
				new SpaceLayout(b4Points),
				new SpaceLayout(b5Points),
				new SpaceLayout(b6Points),
				new SpaceLayout(b7Points),
				new SpaceLayout(b8Points)
			}
		};
		return layout;
	}

	static public BoardLayout BoardC() {

		// Edge points with ocean
		var _01 = new PointF( .4f, .846f );
		var _03 = new PointF( 0.01f, .08f );
		// Internal Points
		var _012 = new PointF( .29f, .6f );
		var _023 = new PointF( .19f, .20f );
		var _125 = new PointF( .56f, .5f );
		var _156 = new PointF( .69f, .53f );
		var _145 = new PointF( .67f, .58f );
		var _234 = new PointF( .53f, .08f );
		var _245 = new PointF( .6f, .2f );
		var _457 = new PointF( 1.05f, .25f );
		var _567 = new PointF( .93f, .55f );
		var _678 = new PointF( 1.15f, .62f );
		// style points
		var _0 = new PointF( .14f, .67f );

		var c0Points = new PointF[] { _01, _012, _023, _03, _0 };
		var c1Points = new PointF[] { _01, topLeftCorner, top[0], top[1], top[2], _156, _125, _012 };
		var c2Points = new PointF[] { _012, _125, _245, _234, _023 };
		var c3Points = new PointF[] { _03, _023, _234, bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin };
		var c4Points = new PointF[] { bottomRightCorner, bot[11], bot[10], bot[9], bot[8], bot[7], bot[6], bot[5], _234, _245, _457, rig[10], rig[11] };
		var c5Points = new PointF[] { _156, _125, _245, _457, _567 };
		var c6Points = new PointF[] { top[2], top[3], top[4], top[5], top[6], _678, _567, _156, };
		var c7Points = new PointF[] { rig[6], rig[7], rig[8], rig[9], rig[10], _457, _567, _678 };
		var c8Points = new PointF[] { top[6], top[7], top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], rig[5], rig[6], _678 };

		var layout = new BoardLayout {
			perimeter = new PointF[] {
				origin,
				_03,_0,_01,
				topLeftCorner,
				top[0], top[1], top[2], top[3], top[4], top[5], top[6], top[7], top[8], top[9], top[10], top[11],
				topRightCorner,
				rig[0],rig[1],rig[2],rig[3],rig[4],rig[5],rig[6],rig[7],rig[8],rig[9],rig[10],rig[11],
				bottomRightCorner,
				bot[11], bot[10], bot[ 9 ], bot[8], bot[7], bot[6], bot[5], bot[4], bot[3], bot[2], bot[1], bot[0],
			},
			Spaces = new SpaceLayout[] {
				new SpaceLayout(c0Points),
				new SpaceLayout(c1Points),
				new SpaceLayout(c2Points),
				new SpaceLayout(c3Points),
				new SpaceLayout(c4Points),
				new SpaceLayout(c5Points),
				new SpaceLayout(c6Points),
				new SpaceLayout(c7Points),
				new SpaceLayout(c8Points)
			}
		};
		return layout;
	}

	static public BoardLayout BoardD() {

		// Edge points with ocean
		var _01 = new PointF( .4f, .846f );
		var _03 = new PointF( 0.0f, .15f );
		// Internal Points
		var _012 = new PointF( .35f, .67f );
		var _023 = new PointF( .15f, .23f );
		var _125 = new PointF( .59f, .66f );
		var _157 = new PointF( .87f, .68f );
		var _178 = new PointF( 1.06f, .73f );
		var _234 = new PointF( .42f, .17f );
		var _245 = new PointF( .56f, .27f );
		var _456 = new PointF( .74f, .31f );
		var _567 = new PointF( .93f, .42f );
		// style points
		var _0 = new PointF( .14f, .67f );

		var d0Points = new PointF[] { _01, _012, _023, _03, _0 };
		var d1Points = new PointF[] { _01, topLeftCorner, top[0], top[1], top[2], top[3], top[4], top[5], top[6], top[7], top[8], top[9], top[10], _178, _157, _125, _012 };
		var d2Points = new PointF[] { _012, _125, _245, _234, _023 };
		var d3Points = new PointF[] { _03, _023, _234, bot[3], bot[2], bot[1], bot[0], origin };
		var d4Points = new PointF[] { bot[9], bot[8], bot[7], bot[6], bot[5], bot[4], bot[3], _234, _245, _456 };
		var d5Points = new PointF[] { _157, _567, _456, _245, _125 };
		var d6Points = new PointF[] { _456, _567, rig[10], rig[11], bottomRightCorner, bot[11], bot[10], bot[9] };
		var d7Points = new PointF[] { rig[4], rig[5], rig[6], rig[7], rig[8], rig[9], rig[10], _567, _157, _178 };
		var d8Points = new PointF[] { top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], _178 };

		var layout = new BoardLayout {
			perimeter = new PointF[] {
				origin,
				_03,_0,_01,
				topLeftCorner,
				top[0], top[1], top[2], top[3], top[4], top[5], top[6], top[7], top[8], top[9], top[10], top[11],
				topRightCorner,
				rig[0],rig[1],rig[2],rig[3],rig[4],rig[5],rig[6],rig[7],rig[8],rig[9],rig[10],rig[11],
				bottomRightCorner,
				bot[11], bot[10], bot[ 9 ], bot[8], bot[7], bot[6], bot[5], bot[4], bot[3], bot[2], bot[1], bot[0],
			},
			Spaces = new SpaceLayout[] {
				new SpaceLayout(d0Points),
				new SpaceLayout(d1Points),
				new SpaceLayout(d2Points),
				new SpaceLayout(d3Points),
				new SpaceLayout(d4Points),
				new SpaceLayout(d5Points),
				new SpaceLayout(d6Points),
				new SpaceLayout(d7Points),
				new SpaceLayout(d8Points)
			}
		};
		layout.Spaces[1].AdjustCenter(0f,-.05f);
		return layout;
	}

	static public T[] JoinAdjacentPolgons<T>( T[] region0, T[] region1 ) {

		// find a corner that is not common
		int i = 0;
		while(i < region0.Length && region1.Contains( region0[i] )) ++i;
		if(i == region0.Length) return new T[0]; // all corners are in common 1 space suround the other

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
