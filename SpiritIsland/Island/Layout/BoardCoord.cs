namespace SpiritIsland;

/// <summary>
/// Non-orthogonal Coordinate system where 2 axis are at 60° to each other instead of 90°
/// </summary>
/// <param name="X">board units in the 0° direction</param>
/// <param name="D60">board units in the 60° direction</param>
public record BoardCoord( int X, int D60 ) {

	#region static
	// To keep board on integer BoardCoords, it deforms as it is rotated
	public static readonly BoardCoord Origin = new BoardCoord( 0, 0 );

	// the default side goes from origin out to (x=1,y=0)
	public static readonly BoardCoord[] Angles = [
		new BoardCoord(1,0),	// 0°  (x=1,y=0)		(no rotation)
		new BoardCoord(0,1),	// 60° (x=.5, y=.86)	(rotated 1 step)
		new BoardCoord(-1,1),	// 120° (x=-.5, y=.86)	(rotated 2 steps)
		new BoardCoord(-1,0),	// 180° (x=-1,y=0)		(rotated 3 steps)
		new BoardCoord(0,-1),	// 240° (x=-.5,y=-.86)	(rotated 4 steps)
		new BoardCoord(1,-1),	// 300° (x=.5,y=-.86)   (rotated 5 steps)
		new BoardCoord(1,0)		// repeat index 0 so have to worry about overflow
	];
	#endregion

	public override string ToString() => $"[{X},{D60}]";
	public static BoardCoord operator +( BoardCoord a, BoardCoord b ) => new BoardCoord( a.X + b.X, a.D60 + b.D60 );
	public static BoardCoord operator -( BoardCoord a, BoardCoord b ) => new BoardCoord( a.X - b.X, a.D60 - b.D60 );
	public static BoardCoord operator -( BoardCoord b ) => new BoardCoord( -b.X, -b.D60 );
}
