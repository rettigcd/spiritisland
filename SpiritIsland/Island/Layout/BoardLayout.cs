//using System.Data;

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
		XY _03 = new( 0.01f, .15f );
		XY _0 = new( .14f, .67f ); // style point
		XY _01 = new( .4f, .846f );
		// Internal Points
		XY _012 = new( .48f, .756f );
		XY _023 = new( .18f, .22f );
		XY _124 = new( .55f, .4f );
		XY _145 = new( .69f, .45f );
		XY _156 = new( .79f, .57f );
		XY _234 = new( .45f, .2f );
		XY _568 = new( 1f, .67f );
		XY _578 = new( 1.17f, .62f );
		// style points
		XY _02 = new( .34f, .67f );
		XY _45 = new( .81f, .35f );
		XY _56 = new( .94f, .54f );

		XY[] a0Points = [_01, _012, _02, _023, _03, _0];
		XY[] a1Points = [_01, topLeftCorner, top[0], top[1], top[2], _156, _145, _124, _012];
		XY[] a2Points = [_012, _124, _234, _023, _02];
		XY[] a3Points = [_03, _023, _234, bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin];
		XY[] a4Points = [bot[9], bot[8], bot[7], bot[6], bot[5], _234, _124, _145, _45];
		XY[] a5Points = [bot[11], bot[10], bot[9], _45, _145, _156, _56, _568, _578,];
		XY[] a6Points = [top[2], top[3], top[4], top[5], top[6], _568, _56, _156,];
		XY[] a7Points = [rig[4], rig[5], rig[6], rig[7], rig[8], rig[9], rig[10], rig[11], bottomRightCorner, bot[11], _578,];
		XY[] a8Points = [top[6], top[7], top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], _578, _568,];

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
		XY _03 = new( 0.01f, .15f );
		XY _0  = new( .14f, .67f ); // style point
		XY _01 = new( .4f, .846f );
		// Internal Points
		XY _012 = new( .36f, .6f );
		XY _023 = new( .15f, .20f );
		XY _156 = new( .7f, .7f );
		XY _145 = new( .67f, .58f );
		XY _124 = new( .53f, .57f );
		XY _234 = new( .57f, .17f );
		XY _457 = new( .91f, .27f );
		XY _567 = new( 1.05f, .5f );
		XY _678 = new( 1.15f, .6f );

		XY[] b0Points = [_01, _012, _023, _03, _0];
		XY[] b1Points = [_01, topLeftCorner, top[0], _156, _145, _124, _012];
		XY[] b2Points = [_012, _124, _234, _023];
		XY[] b3Points = [_03, _023, _234, bot[7], bot[6], bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin];
		XY[] b4Points = [bot[11], bot[10], bot[9], bot[8], bot[7], _234, _124, _145, _457];
		XY[] b5Points = [_145, _156, _567, _457];
		XY[] b6Points = [top[0], top[1], top[2], top[3], top[4], top[5], top[6], _678, _567, _156,];
		XY[] b7Points = [rig[4], rig[5], rig[6], rig[7], rig[8], rig[9], rig[10], rig[11], bottomRightCorner, bot[11], _457, _567, _678];
		XY[] b8Points = [top[6], top[7], top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], _678];

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
		XY _01 = new ( .4f, .846f );
		XY _03 = new ( 0.01f, .08f );
		// Internal Points
		XY _012 = new ( .29f, .6f );
		XY _023 = new ( .19f, .20f );
		XY _125 = new ( .56f, .5f );
		XY _156 = new ( .69f, .53f );
		XY _145 = new ( .67f, .58f );
		XY _234 = new ( .53f, .08f );
		XY _245 = new ( .6f, .2f );
		XY _457 = new ( 1.05f, .25f );
		XY _567 = new ( .93f, .55f );
		XY _678 = new ( 1.15f, .62f );
		// style points
		XY _0 = new ( .14f, .67f );

		XY[] c0Points = [_01, _012, _023, _03, _0];
		XY[] c1Points = [_01, topLeftCorner, top[0], top[1], top[2], _156, _125, _012];
		XY[] c2Points = [_012, _125, _245, _234, _023];
		XY[] c3Points = [_03, _023, _234, bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin];
		XY[] c4Points = [bottomRightCorner, bot[11], bot[10], bot[9], bot[8], bot[7], bot[6], bot[5], _234, _245, _457, rig[10], rig[11]];
		XY[] c5Points = [_567, _457, _245, _125, _156];
		XY[] c6Points = [top[2], top[3], top[4], top[5], top[6], _678, _567, _156,];
		XY[] c7Points = [rig[6], rig[7], rig[8], rig[9], rig[10], _457, _567, _678];
		XY[] c8Points = [top[6], top[7], top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], rig[5], rig[6], _678];

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
		XY _03 = new ( 0.0f, .15f );
		XY _0  = new ( .14f, .67f ); // style
		XY _01 = new ( .4f, .846f );
		// Internal Points
		XY _012 = new ( .35f, .67f );
		XY _023 = new ( .15f, .23f );
		XY _125 = new ( .59f, .66f );
		XY _157 = new ( .87f, .68f );
		XY _178 = new ( 1.06f, .73f );
		XY _234 = new ( .42f, .17f );
		XY _245 = new ( .56f, .27f );
		XY _456 = new ( .74f, .31f );
		XY _567 = new ( .93f, .42f );

		XY[] d0Points = [_01, _012, _023, _03, _0];
		XY[] d1Points = [_01, topLeftCorner, top[0], top[1], top[2], top[3], top[4], top[5], top[6], top[7], top[8], top[9], top[10], _178, _157, _125, _012];
		XY[] d2Points = [_012, _125, _245, _234, _023];
		XY[] d3Points = [_03, _023, _234, bot[3], bot[2], bot[1], bot[0], origin];
		XY[] d4Points = [bot[9], bot[8], bot[7], bot[6], bot[5], bot[4], bot[3], _234, _245, _456];
		XY[] d5Points = [_157, _567, _456, _245, _125];
		XY[] d6Points = [_456, _567, rig[10], rig[11], bottomRightCorner, bot[11], bot[10], bot[9]];
		XY[] d7Points = [rig[4], rig[5], rig[6], rig[7], rig[8], rig[9], rig[10], _567, _157, _178];
		XY[] d8Points = [top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], _178];

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
		XY _03 = new ( 0.0f, .15f );
		XY _0  = new ( .18f, .67f ); // style point
		XY _01 = new ( .4f, .846f );
		// Internal Points
		XY _012 = new ( .35f, .65f );
		XY _023 = new ( .15f, .23f );
		XY _125 = new ( .59f, .63f );
		XY _157 = new ( .87f, .65f );
		XY _235 = new ( .46f, .23f );
		XY _345 = new ( .62f, .21f );
		XY _457 = new ( .82f, .42f );
		XY _467 = new ( .93f, .41f );
		XY _678 = new ( 1.13f, .52f );
		// style points
		XY _67s = new ( 1.13f, .32f );

		XY[] e0Points = [_01, _012, _023, _03, _0];
		XY[] e1Points = [_01, topLeftCorner, top[0], top[1], top[2], top[3], top[4], _157, _125, _012];
		XY[] e2Points = [_012, _125, _235, _023];
		XY[] e3Points = [_03, _023, _235, _345, bot[5], bot[4], bot[3], bot[2], bot[1], bot[0], origin];
		XY[] e4Points = [bot[11], bot[10], bot[9], bot[8], bot[7], bot[6], bot[5], _345, _457, _467];
		XY[] e5Points = [_157, _457, _345, _235, _125];
		XY[] e6Points = [bottomRightCorner, bot[11], _467, _678, _67s, rig[8], rig[9], rig[10], rig[11]];
		XY[] e7Points = [top[4], top[5], top[6], top[7], top[8], _678, _467, _457, _157];
		XY[] e8Points = [top[8], top[9], top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], rig[5], rig[6], rig[7], rig[8], _67s, _678];

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
		XY _03 = new ( 0.0f, .15f );
		XY _0  = new ( .18f, .67f ); // style point
		XY _01 = new ( .4f, .846f );
		// Internal Points
		XY _012 = new ( .35f, .65f );
		XY _023 = new ( .15f, .23f );
		XY _125 = new ( .59f, .63f );
		XY _156 = new ( .87f, .65f );
		XY _234 = new ( .46f, .23f );
		XY _245 = new ( .6f, .30f );
		XY _345 = new ( .56f, .3f );
		XY _458 = new ( .88f, .38f );
		XY _478 = new ( .95f, .31f );
		XY _568 = new ( 1.05f, .60f );

		XY[] f0Points = [_01, _012, _023, _03, _0];
		XY[] f1Points = [_01, topLeftCorner, top[0], top[1], top[2], top[3], top[4], _156, _125, _012];
		XY[] f2Points = [_012, _125, _245, _234, _023];
		XY[] f3Points = [_03, _023, _234, bot[3], bot[2], bot[1], bot[0], origin];
		XY[] f4Points = [bot[9], bot[8], bot[7], bot[6], bot[5], bot[4], bot[3], _234, _245, _458, _478];
		XY[] f5Points = [_125, _156, _568, _458, _245];
		XY[] f6Points = [top[4], top[5], top[6], top[7], top[8], top[9], top[10], _568, _156];
		XY[] f7Points = [bottomRightCorner, bot[11], bot[10], bot[9], _478, rig[6], rig[7], rig[8], rig[9], rig[10], rig[11]];
		XY[] f8Points = [top[10], top[11], topRightCorner, rig[0], rig[1], rig[2], rig[3], rig[4], rig[5], rig[6], _478, _458, _568];

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
	public XY[] Perimeter {get;private set;} // counter-clockwise, starting at origin
	
	/// <summary>
	/// The current locations of this boards corners in the (0,0,1.5,.866) coordinate system
	/// </summary>
	public XY[] BoardCorners => _boardCorners;
	public Bounds Bounds => _bounds ??= CalcBounds();
	public XY Size => Bounds.Size;

	public SpaceLayout ForSpaceSpec( SpaceSpec space ) {
		int index = space.Label[1] - 48;
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

	Bounds CalcBounds() {
		var builder = new BoundsBuilder();
		foreach(XY p in Perimeter)
			builder.Include(p);
		return builder.GetBounds();
	}

	// counter-clockwise, starting at origin
	readonly XY[] _boardCorners = [
		origin, // Side-2 origin (no rotation)
		bottomRightCorner, // Side-1 origin after -60 rotation
		topRightCorner, // Side-0 origin (after 180 rotation)
		topLeftCorner,
	];

	Bounds? _bounds;
	SpaceLayout[] _spaces;

	#endregion

	#region private static GenericBoard

	static readonly GenericBoard Singleton = new GenericBoard();
	// Sides
	static readonly XY[] top = Singleton.top;
	static readonly XY[] rig = Singleton.rig;
	static readonly XY[] bot = Singleton.bot;
	// corner
	static readonly XY origin = Singleton.origin;
	static readonly XY topLeftCorner = Singleton.topLeftCorner;
	static readonly XY topRightCorner = Singleton.topRightCorner;
	static readonly XY bottomRightCorner = Singleton.bottomRightCorner;
	// height
	static public readonly float boardHeight = Singleton.boardHeight;

	/// <summary>
	/// Starting at (0,0) and going CCW around the board. 
	/// Left/Ocean, Top, Right, Bottom
	/// </summary>
	static XY[] MakePerimeter_CCW(XY _03, XY _0, XY _01 ) => [
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

/// <remarks>
/// Not part of Bounds class because it is generic and we don't want to clutter it up.
/// </remarks>
public class BoundsBuilder {

	static public Bounds ForPoints(IEnumerable<XY> corners) {
		var builder = new BoundsBuilder();
		foreach( var p in corners )
			builder.Include(p);
		return builder.GetBounds();
	}

	#region constructors

	public BoundsBuilder() { }

	public BoundsBuilder(IEnumerable<Bounds> parts) {
		foreach( var part in parts )
			Include(part);
	}

	#endregion constructors

	public void Include(Bounds b) {
		Include(b.X,b.Y);
		Include(b.Right,b.Bottom);
	}
	public void Include(XY p) => Include(p.X,p.Y);
	public void Include(float x,float y) {
		if( x < minX ) minX = x;
		if( y < minY ) minY = y;
		if( x > maxX ) maxX = x;
		if( y > maxY ) maxY = y;
	}
	public Bounds GetBounds() => new Bounds(minX, minY, maxX - minX, maxY - minY);

	float minX = float.MaxValue;
	float minY = float.MaxValue;
	float maxX = float.MinValue;
	float maxY = float.MinValue;
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

		origin = new XY( 0f, 0f );
		topLeftCorner = new XY( 0.5f, boardHeight );
		bottomRightCorner = new XY( 1f, 0f );
		topRightCorner = new XY( 1.5f, boardHeight );

		// Create an array across the bottom (CCW)
		var arr = new XY[12];
		arr[0] = new XY( xOff + xDelta, 0 );
		arr[1] = new XY( xOff + xDelta * 2, -yOffset );
		arr[2] = new XY( xOff + xDelta * 3, -yOffset );
		arr[3] = new XY( xOff + xDelta * 4, -yOffset );
		arr[4] = new XY( xOff + xDelta * 5, 0 );
		arr[5] = new XY( xOff + xDelta * 6, 0 );
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
	public readonly XY origin;
	public readonly XY topLeftCorner;
	public readonly XY bottomRightCorner;
	public readonly XY topRightCorner;
	/// <summary> goes CCW (left to right) </summary>
	public readonly XY[] bot;
	/// <summary> goes CW (left to right) </summary>
	public readonly XY[] top;
	/// <summary> goes CW (top to bottom) </summary>
	public readonly XY[] rig;

	static XY inflect( XY orig ) => new XY( 1.0f - orig.X, -orig.Y );
	static XY Translate( XY orig, float deltaX, float deltaY )
		=> new XY( orig.X + deltaX, orig.Y + deltaY );
	static XY RotateAboutOrigin( XY orig, double thetaDegrees ) {
		double theta = thetaDegrees * Math.PI / 180;
		double sinTheta = Math.Sin( theta );
		double cosTheta = Math.Cos( theta );
		return new XY(
			(float)(orig.X * cosTheta - orig.Y * sinTheta),
			(float)(orig.X * sinTheta + orig.Y * cosTheta)
		);
	}

}