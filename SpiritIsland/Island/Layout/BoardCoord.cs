namespace SpiritIsland;

/// <summary>
/// Coordinate system where 2 axis are at 60° to each other instead of 90°
/// </summary>
/// <param name="x">board units in the 0° direction</param>
/// <param name="d60">board units in the 60° direction</param>
public record BoardCoord( int x, int d60 ) {

	#region static
	// To keep board on integer BoardCoords, it deforms as it is rotated
	public static readonly BoardCoord Origin = new BoardCoord( 0, 0 );
	public static readonly BoardCoord[] Angles = new BoardCoord[] {
		new BoardCoord(1,0),
		new BoardCoord(0,1),
		new BoardCoord(-1,1),
		new BoardCoord(-1,0),
		new BoardCoord(0,-1),
		new BoardCoord(1,-1),
		new BoardCoord(1,0) // repeat index 0 so have to worry about overflow
	};
	#endregion

	public override string ToString() => $"[{x},{d60}]";
	public static BoardCoord operator +( BoardCoord a, BoardCoord b ) => new BoardCoord( a.x + b.x, a.d60 + b.d60 );
	public static BoardCoord operator -( BoardCoord a, BoardCoord b ) => new BoardCoord( a.x - b.x, a.d60 - b.d60 );
	public static BoardCoord operator -( BoardCoord b ) => new BoardCoord( -b.x, -b.d60 );
}
