namespace SpiritIsland;

public class BoardOrientation {

	public static BoardOrientation ToMatchSide( int movingSideIndex, SideCoords stationary ) {
		int requiredRotation = stationary.RotationStep
			- (movingSideIndex == 2 ? 3 : movingSideIndex) // starting angle of sides is 0,1,3
			+ 3;
		BoardCoord movingFrom = CalcCornerOffset( movingSideIndex, requiredRotation );
		return new BoardOrientation( stationary.To - movingFrom, requiredRotation );
	}

	/// <summary>
	/// Calculates the corner offset from the board-origin. But NOT translated.
	/// </summary>
	/// <param name="cornerIndex">with ocean on left, 0=bottom left, 1=bottom right, 2=top right, 3=top left</param>
	/// <param name="boardRotationStep"></param>
	/// <returns>coordinate of rotated corner in (x,d60) units</returns>
	public static BoardCoord CalcCornerOffset( int cornerIndex, int boardRotationStep )
		=> cornerIndex switch {
			0 => BoardCoord.Origin, // origin is at (0,0) and does not move.
			1 => BoardCoord.Angles[boardRotationStep],  // corner-1 is at 0° (step-index 0+rotation)
			2 => BoardCoord.Angles[boardRotationStep] + BoardCoord.Angles[boardRotationStep + 1], // corner-2 is at 0° step + 60° step + (rotation)
			3 => BoardCoord.Angles[boardRotationStep + 1], // corner-3 is at 60° (step-index 1+rotation)
			_ => throw new IndexOutOfRangeException()
		};

	public static readonly BoardOrientation Home = new BoardOrientation( BoardCoord.Origin, 0 );

	#region constructor

	public BoardOrientation( BoardCoord offset, int rotation ) {
		Offset = offset;
		RotationSteps = rotation % 6;

		// Record Corners - Translated, rotated
		BoardCoord[] corners = new BoardCoord[4];
		for(int i = 0; i < 4; ++i)
			corners[i] = Offset + CalcCornerOffset( i, RotationSteps );
		Corners = Array.AsReadOnly( corners );

		Sides = new SideCoords[3];
		for(int index = 0; index < 3; ++index)
			Sides[index] = new SideCoords( Corners[index], Corners[index + 1] );
	}

	#endregion

	public BoardCoord Offset { get; }
	public int RotationSteps { get; }

	// public string QuickCorners => string.Join("",Corners.Cast<BoardCoord>());

	// with ocean on the left, go Counter Clock-wise starting at origin
	// 0: bottom left (by ocean)
	// 1: bottom right (obtuse)
	// 2: top right (point)
	// 3: top left (by ocean)
	// Ordering like this so origin is on corner-0
	// Board defines sides in reverse order (clock-wise)
	public ReadOnlyCollection<BoardCoord> Corners { get; }

	// going Counter Clockwise
	// Ocean on the left
	// 0: bottom
	// 1: right
	// 2: top
	public SideCoords SideCoord( int index ) => new SideCoords( Corners[index], Corners[index + 1] );

	public SideCoords[] Sides { get; }

	/// <summary> Pre-Reversed because we only use to compare against normal sizes. </summary>
	public SideCoords OceanSideReversed => new SideCoords( Corners[0], Corners[3] );

	/// <summary> Location in (x,d60) coords of Lower left corner of UP-triangle</summary>
	/// <remarks> Boards with the Same Odd Corner or Even Corner location will overlap.  Should not happen.</remarks>
	public BoardCoord OddCorner => Corners[OddCornerIndex];
	// Depending on rotated, which corner is the odd corner?
	// 6 options because boards may be rotated in 6 distinct orientations
	int OddCornerIndex => (new int[] { 0, 3, 3, 2, 1, 1 })[RotationSteps];

	/// <summary> Upper right corner of DOWN-triangle
	/// <remarks> Boards with the Same Odd Corner or Even Corner location will overlap.  Should not happen.</remarks>
	public BoardCoord EvenCorner => Corners[EvenCornerIndex];
	// 6 options because boards may be rotated in 6 distinct orientations
	int EvenCornerIndex => (new int[] { 2, 1, 1, 0, 3, 3 })[RotationSteps];

	public Matrix3D GetTransformMatrix() {
		int rotate = RotationSteps * 60;
		float dx = Offset.X + 0.5f * Offset.D60;
		float dy = (float)(Offset.D60 * (0.5 * Math.Sqrt( 3 )));
		return RowVector.RotateDegrees( rotate )
			* RowVector.Translate( dx, dy );
	}

}