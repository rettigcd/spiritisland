using System.Data;
using System.Drawing;

namespace SpiritIsland;

/// <summary>
/// Layout based on a bounding Rectangle of (0,0,1.5,.866)
/// </summary>
public class BoardLayout {

	#region Static Builders

	static public BoardLayout Get( string name )
		=> name switch {
			"A" => BoardA(),
			"B" => BoardB(),
			"C" => BoardC(),
			"D" => BoardD(),
			"E" => BoardE(),
			"F" => BoardF(),
			_ => null,
		};

	static public BoardLayout BoardA() {

		// Edge points with ocean
		PointF _03 = new( 0.01f, .15f );
		PointF _0 = new( .14f, .67f ); // style point
		PointF _01 = new( .4f, .846f );
		// Internal Points
		PointF _012 = new( .48f, .756f );
		PointF _023 = new( .18f, .22f );
		PointF _124 = new( .55f, .4f );
		PointF _145 = new( .69f, .45f );
		PointF _156 = new( .79f, .57f );
		PointF _234 = new( .45f, .2f );
		PointF _568 = new( 1f, .67f );
		PointF _578 = new( 1.17f, .62f );
		// style points
		PointF _02 = new( .34f, .67f );
		PointF _45 = new( .81f, .35f );
		PointF _56 = new( .94f, .54f );

		PointF[] a0Points = [_01, _012, _02, _023, _03, _0];
		PointF[] a1Points = [_01, topLeftCorner, top[0], top[1], top[2], _156, _145, _124, _012];
		PointF[] a2Points = [_012, _124, _234, _023, _02];
		PointF[] a3Points = [_03, _023, _234, bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin];
		PointF[] a4Points = [bot[9], bot[8], bot[7], bot[6], bot[5], _234, _124, _145, _45];
		PointF[] a5Points = [bot[11], bot[10], bot[9], _45, _145, _156, _56, _568, _578,];
		PointF[] a6Points = [top[2], top[3], top[4], top[5], top[6], _568, _56, _156,];
		PointF[] a7Points = [rig[4], rig[5], rig[6], rig[7], rig[8], rig[9], rig[10], rig[11], bottomRightCorner, bot[11], _578,];
		PointF[] a8Points = [top[6], top[7], top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], _578, _568,];

		var layout = new BoardLayout {
			Perimeter = MakePerimeter_CCW( _03, _0, _01 ),
			_spaces = [
				new SpaceLayout( a0Points ),
				new SpaceLayout( a1Points ),
				new SpaceLayout( a2Points ),
				new SpaceLayout( a3Points ),
				new SpaceLayout( a4Points ),
				new SpaceLayout( a5Points ),
				new SpaceLayout( a6Points ),
				new SpaceLayout( a7Points ),
				new SpaceLayout( a8Points )
			]
		};
		return layout;
	}

	static public BoardLayout BoardB() {

		// Edge points with ocean
		var _03 = new PointF( 0.01f, .15f );
		var _0 = new PointF( .14f, .67f ); // style point
		var _01 = new PointF( .4f, .846f );
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

		PointF[] b0Points = [_01, _012, _023, _03, _0];
		PointF[] b1Points = [_01, topLeftCorner, top[0], _156, _145, _124, _012];
		PointF[] b2Points = [_012, _124, _234, _023];
		PointF[] b3Points = [_03, _023, _234, bot[7], bot[6], bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin];
		PointF[] b4Points = [bot[11], bot[10], bot[9], bot[8], bot[7], _234, _124, _145, _457];
		PointF[] b5Points = [_145, _156, _567, _457];
		PointF[] b6Points = [top[0], top[1], top[2], top[3], top[4], top[5], top[6], _678, _567, _156,];
		PointF[] b7Points = [rig[4], rig[5], rig[6], rig[7], rig[8], rig[9], rig[10], rig[11], bottomRightCorner, bot[11], _457, _567, _678];
		PointF[] b8Points = [top[6], top[7], top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], _678];

		var layout = new BoardLayout {
			Perimeter = MakePerimeter_CCW( _03, _0, _01 ),
			_spaces = [
				new SpaceLayout( b0Points ),
				new SpaceLayout( b1Points ),
				new SpaceLayout( b2Points ),
				new SpaceLayout( b3Points ),
				new SpaceLayout( b4Points ),
				new SpaceLayout( b5Points ),
				new SpaceLayout( b6Points ),
				new SpaceLayout( b7Points ),
				new SpaceLayout( b8Points )
			]
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

		PointF[] c0Points = [_01, _012, _023, _03, _0];
		PointF[] c1Points = [_01, topLeftCorner, top[0], top[1], top[2], _156, _125, _012];
		PointF[] c2Points = [_012, _125, _245, _234, _023];
		PointF[] c3Points = [_03, _023, _234, bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin];
		PointF[] c4Points = [bottomRightCorner, bot[11], bot[10], bot[9], bot[8], bot[7], bot[6], bot[5], _234, _245, _457, rig[10], rig[11]];
		PointF[] c5Points = [_567, _457, _245, _125, _156];
		PointF[] c6Points = [top[2], top[3], top[4], top[5], top[6], _678, _567, _156,];
		PointF[] c7Points = [rig[6], rig[7], rig[8], rig[9], rig[10], _457, _567, _678];
		PointF[] c8Points = [top[6], top[7], top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], rig[5], rig[6], _678];

		var layout = new BoardLayout {
			Perimeter = [
				origin,
				_03,
				_0,
				_01,
				topLeftCorner,
				top[0],
				top[1],
				top[2],
				top[3],
				top[4],
				top[5],
				top[6],
				top[7],
				top[8],
				top[9],
				top[10],
				top[11],
				topRightCorner,
				rig[0],
				rig[1],
				rig[2],
				rig[3],
				rig[4],
				rig[5],
				rig[6],
				rig[7],
				rig[8],
				rig[9],
				rig[10],
				rig[11],
				bottomRightCorner,
				bot[11],
				bot[10],
				bot[9],
				bot[8],
				bot[7],
				bot[6],
				bot[5],
				bot[4],
				bot[3],
				bot[2],
				bot[1],
				bot[0],
			],
			_spaces = [
				new SpaceLayout( c0Points ),
				new SpaceLayout( c1Points ),
				new SpaceLayout( c2Points ),
				new SpaceLayout( c3Points ),
				new SpaceLayout( c4Points ),
				new SpaceLayout( c5Points ),
				new SpaceLayout( c6Points ),
				new SpaceLayout( c7Points ),
				new SpaceLayout( c8Points )
			]
		};
		return layout;
	}

	static public BoardLayout BoardD() {

		// Edge points with ocean
		var _03 = new PointF( 0.0f, .15f );
		var _0 = new PointF( .14f, .67f ); // style
		var _01 = new PointF( .4f, .846f );
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

		PointF[] d0Points = [_01, _012, _023, _03, _0];
		PointF[] d1Points = [_01, topLeftCorner, top[0], top[1], top[2], top[3], top[4], top[5], top[6], top[7], top[8], top[9], top[10], _178, _157, _125, _012];
		PointF[] d2Points = [_012, _125, _245, _234, _023];
		PointF[] d3Points = [_03, _023, _234, bot[3], bot[2], bot[1], bot[0], origin];
		PointF[] d4Points = [bot[9], bot[8], bot[7], bot[6], bot[5], bot[4], bot[3], _234, _245, _456];
		PointF[] d5Points = [_157, _567, _456, _245, _125];
		PointF[] d6Points = [_456, _567, rig[10], rig[11], bottomRightCorner, bot[11], bot[10], bot[9]];
		PointF[] d7Points = [rig[4], rig[5], rig[6], rig[7], rig[8], rig[9], rig[10], _567, _157, _178];
		PointF[] d8Points = [top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], _178];

		var layout = new BoardLayout {
			Perimeter = MakePerimeter_CCW( _03, _0, _01 ),
			_spaces = [
				new SpaceLayout( d0Points ),
				new SpaceLayout( d1Points ),
				new SpaceLayout( d2Points ),
				new SpaceLayout( d3Points ),
				new SpaceLayout( d4Points ),
				new SpaceLayout( d5Points ),
				new SpaceLayout( d6Points ),
				new SpaceLayout( d7Points ),
				new SpaceLayout( d8Points )
			]
		};
		return layout;
	}

	static public BoardLayout BoardE() {

		// Edge points with ocean
		var _03 = new PointF( 0.0f, .15f );
		var _0 = new PointF( .18f, .67f ); // style point
		var _01 = new PointF( .4f, .846f );
		// Internal Points
		var _012 = new PointF( .35f, .65f );
		var _023 = new PointF( .15f, .23f );
		var _125 = new PointF( .59f, .63f );
		var _157 = new PointF( .87f, .65f );
		var _235 = new PointF( .46f, .23f );
		var _345 = new PointF( .62f, .21f );
		var _457 = new PointF( .82f, .42f );
		var _467 = new PointF( .93f, .41f );
		var _678 = new PointF( 1.13f, .52f );
		// style points
		var _67s = new PointF( 1.13f, .32f );

		PointF[] e0Points = [_01, _012, _023, _03, _0];
		PointF[] e1Points = [_01, topLeftCorner, top[0], top[1], top[2], top[3], top[4], _157, _125, _012];
		PointF[] e2Points = [_012, _125, _235, _023];
		PointF[] e3Points = [_03, _023, _235, _345, bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin];
		PointF[] e4Points = [bot[11], bot[10], bot[9], bot[8], bot[7], bot[6], bot[5], _345, _457, _467];
		PointF[] e5Points = [_157, _457, _345, _235, _125];
		PointF[] e6Points = [bottomRightCorner, bot[11], _467, _678, _67s, rig[8], rig[9], rig[10], rig[11]];
		PointF[] e7Points = [top[4], top[5], top[6], top[7], top[8], _678, _467, _457, _157];
		PointF[] e8Points = [top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], rig[5], rig[6], rig[7], rig[8], _67s, _678];

		var layout = new BoardLayout {
			Perimeter = MakePerimeter_CCW( _03, _0, _01 ),
			_spaces = [
				new SpaceLayout( e0Points ),
				new SpaceLayout( e1Points ),
				new SpaceLayout( e2Points ),
				new SpaceLayout( e3Points ),
				new SpaceLayout( e4Points ),
				new SpaceLayout( e5Points ),
				new SpaceLayout( e6Points ),
				new SpaceLayout( e7Points ),
				new SpaceLayout( e8Points )
			]
		};
		return layout;
	}

	static public BoardLayout BoardF() {

		// Edge points with ocean
		var _03 = new PointF( 0.0f, .15f );
		var _0 = new PointF( .18f, .67f ); // style point
		var _01 = new PointF( .4f, .846f );
		// Internal Points
		var _012 = new PointF( .35f, .65f );
		var _023 = new PointF( .15f, .23f );
		var _125 = new PointF( .59f, .63f );
		var _156 = new PointF( .87f, .65f );
		var _234 = new PointF( .46f, .23f );
		var _245 = new PointF( .6f, .30f );
		var _345 = new PointF( .56f, .3f );
		var _458 = new PointF( .88f, .38f );
		var _478 = new PointF( .95f, .31f );
		var _568 = new PointF( 1.05f, .60f );

		PointF[] f0Points = [_01, _012, _023, _03, _0];
		PointF[] f1Points = [_01, topLeftCorner, top[0], top[1], top[2], top[3], top[4], _156, _125, _012];
		PointF[] f2Points = [_012, _125, _245, _234, _023];
		PointF[] f3Points = [_03, _023, _234, bot[3], bot[2], bot[1], bot[0], origin];
		PointF[] f4Points = [bot[9], bot[8], bot[7], bot[6], bot[5], bot[4], bot[3], _234, _245, _458, _478];
		PointF[] f5Points = [_125, _156, _568, _458, _245];
		PointF[] f6Points = [top[4], top[5], top[6], top[7], top[8], top[9], top[10], _568, _156];
		PointF[] f7Points = [bottomRightCorner, bot[11], bot[10], bot[9], _478, rig[6], rig[7], rig[8], rig[9], rig[10], rig[11]];
		PointF[] f8Points = [top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], rig[5], rig[6], _478, _458, _568];

		var layout = new BoardLayout {
			Perimeter = MakePerimeter_CCW(_03,_0,_01),
			_spaces = [
				new SpaceLayout( f0Points ),
				new SpaceLayout( f1Points ),
				new SpaceLayout( f2Points ),
				new SpaceLayout( f3Points ),
				new SpaceLayout( f4Points ),
				new SpaceLayout( f5Points ),
				new SpaceLayout( f6Points ),
				new SpaceLayout( f7Points ),
				new SpaceLayout( f8Points )
			]
		};
		return layout;
	}

	#endregion

	/// <summary>
	/// The current locations of this boards perimeter in the (0,0,1.5,.866) coordinate system
	/// </summary>
	public PointF[] Perimeter {get;private set;} // counter-clockwise, starting at origin
	
	/// <summary>
	/// The current locations of this boards corners in the (0,0,1.5,.866) coordinate system
	/// </summary>
	public PointF[] BoardCorners => _boardCorners;
	public RectangleF Bounds => _bounds ??= CalcBounds();

	public SpaceLayout ForSpace( Space space ) {
		int index = space.Text[1] - 48;
		return _spaces[index];
	}

	/// <summary>
	/// Transforms the current Layout (does not create a new one)
	/// </summary>
	/// <returns>this BoardLayout</returns>
	public BoardLayout ReMap( Matrix3D transform ) {

		var mapper = new PointMapper( transform );

		// perimeter
		for(int i=0;i<Perimeter.Length;++i)
			Perimeter[i] = mapper.Map( Perimeter[i] );

		foreach(var space in this._spaces)
			space.ReMap(mapper);

		// origin
		for(int i = 0; i < _boardCorners.Length; ++i) {
			var src = _boardCorners[i];
			var dst = mapper.Map( src );
			_boardCorners[i] = dst;
		}
		return this;
	}
	

	#region private member

	RectangleF CalcBounds() {
		float minX = float.MaxValue;
		float minY = float.MaxValue;
		float maxX = float.MinValue;
		float maxY = float.MinValue;
		foreach(var p in Perimeter) {
			if(p.X < minX) minX = p.X;
			if(p.Y < minY) minY = p.Y;
			if(p.X > maxX) maxX = p.X;
			if(p.Y > maxY) maxY = p.Y;
		}
		return new RectangleF( minX, minY, maxX - minX, maxY - minY );
	}

	// counter-clockwise, starting at origin
	readonly PointF[] _boardCorners = [
		origin, // Side-2 origin (no rotation)
		bottomRightCorner, // Side-1 origin after -60 rotation
		topRightCorner, // Side-0 origin (after 180 rotation)
		topLeftCorner,
	];

	RectangleF? _bounds;
	SpaceLayout[] _spaces;

	#endregion

	#region private static GenericBoard

	static readonly GenericBoard Singleton = new GenericBoard();
	// Sides
	static readonly PointF[] top = Singleton.top;
	static readonly PointF[] rig = Singleton.rig;
	static readonly PointF[] bot = Singleton.bot;
	// corner
	static readonly PointF origin = Singleton.origin;
	static readonly PointF topLeftCorner = Singleton.topLeftCorner;
	static readonly PointF topRightCorner = Singleton.topRightCorner;
	static readonly PointF bottomRightCorner = Singleton.bottomRightCorner;
	// height
	static public readonly float boardHeight = Singleton.boardHeight;

	/// <summary>
	/// Starting at (0,0) and going CCW around the board. 
	/// Left/Ocean, Top, Right, Bottom
	/// </summary>
	static PointF[] MakePerimeter_CCW(PointF _03, PointF _0, PointF _01) => [
		// go: Up the left side
		origin, _03, _0, _01,
		// go: right along the TOP
		topLeftCorner, top[0], top[1], top[2], top[3], top[4], top[5], top[6], top[7], top[8], top[9],top[10],top[11],
		// go: down along the RIGHT
		topRightCorner,rig[0],rig[1],rig[2],rig[3],rig[4],rig[5],rig[6],rig[7],rig[8],rig[9],rig[10],rig[11],
		// go: left along the BOTTOM
		bottomRightCorner,bot[11],bot[10],bot[9],bot[8],bot[7],bot[6],bot[5],bot[4],bot[3],bot[2],bot[1],bot[0],
	];

	#endregion

}


class GenericBoard {

	/// <summary>
	/// Creates a board whose bounding rect is (0,0,1.5,.866)
	/// </summary>
	public GenericBoard() {

		boardHeight = (float)(0.5 * Math.Sqrt( 3 ));
		float xDelta = .06f;
		float yOffset = xDelta; // could be anything..
		float xOff = 0.09f;

		origin = new PointF( 0f, 0f );
		topLeftCorner = new PointF( 0.5f, boardHeight );
		bottomRightCorner = new PointF( 1f, 0f );
		topRightCorner = new PointF( 1.5f, boardHeight );

		// Create an array across the bottom (CCW)
		var arr = new PointF[12];
		arr[0] = new PointF( xOff + xDelta, 0 );
		arr[1] = new PointF( xOff + xDelta * 2, -yOffset );
		arr[2] = new PointF( xOff + xDelta * 3, -yOffset );
		arr[3] = new PointF( xOff + xDelta * 4, -yOffset );
		arr[4] = new PointF( xOff + xDelta * 5, 0 );
		arr[5] = new PointF( xOff + xDelta * 6, 0 );
		arr[6] = inflect( arr[5] );
		arr[7] = inflect( arr[4] );
		arr[8] = inflect( arr[3] );
		arr[9] = inflect( arr[2] );
		arr[10] = inflect( arr[1] );
		arr[11] = inflect( arr[0] );

		// !! Make these all go the same direction: CW or CCW

		// top (CW)
		top = [..arr.Select( orig => Translate( orig, 0.5f, boardHeight ) )];

		// right (CW)
		rig = [..arr.Select( orig => Translate( RotateAboutOrigin( orig, 240.0 ), 1.5f, boardHeight ) )];

		// bottom (CCW)
		bot = arr;

	}

	// Corners
	public readonly float boardHeight;
	public readonly PointF origin;
	public readonly PointF topLeftCorner;
	public readonly PointF bottomRightCorner;
	public readonly PointF topRightCorner;
	/// <summary> goes CCW (left to right) </summary>
	public readonly PointF[] bot;
	/// <summary> goes CW (left to right) </summary>
	public readonly PointF[] top;
	/// <summary> goes CW (top to bottom) </summary>
	public readonly PointF[] rig;

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

}

/// <summary>
/// Provides Layouts for Spaces and Boards for a given island.
/// </summary>
public class IslandLayout {

	public SpaceLayout GetLayoutFor( Space space ) {
		if( space is Space1 s1 )
			return this[s1.Board].ForSpace(s1);

		if(space is MultiSpace ms) {
			Space1[] spaces = ms.OrigSpaces;
			PointF[] merged = GetLayoutFor( spaces[0] ).Corners;
			for(int i = 1; i < spaces.Length; ++i)
				merged = Polygons.JoinAdjacentPolgons( merged, GetLayoutFor(spaces[i]).Corners );
			return new SpaceLayout( merged );
		}

		if(true) { // space == EndlessDark.Space
			const float x = .1f;
			const float y = .9f;
			const float f = .2f;
			return new SpaceLayout(
				new PointF( x + 0, y ),
				new PointF( x + f, y ),
				new PointF( x + f, y - f ),
				new PointF( x + 0, y - f )
			);
		}

		throw new ArgumentException( "Unknown space type" );
	}

	public BoardLayout this[Board board] {
		get {
			if(_lookupBoardLayout.TryGetValue(board,out BoardLayout bl)) return bl;
			var boardLayout = BoardLayout.Get( board.Name ).ReMap( board.Orientation.GetTransformMatrix() );
			_lookupBoardLayout.Add(board, boardLayout );
			return boardLayout;
		}
	}

	readonly Dictionary<Board, BoardLayout> _lookupBoardLayout = [];

}