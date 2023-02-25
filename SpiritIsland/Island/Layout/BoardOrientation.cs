using System;
using System.Reflection;

namespace SpiritIsland;

public class BoardOrientation {

	public static BoardOrientation ToMatchSide( int movingSideIndex, SideCoords stationary ) {
		int requiredRotation = stationary.RotationStep 
			- (movingSideIndex == 2 ? 3 : movingSideIndex) // starting angle of sides is 0,1,3
			+ 3;
		BoardCoord movingFrom = OrigCorner( movingSideIndex, requiredRotation );
		return new BoardOrientation( stationary.To-movingFrom, requiredRotation);
	}

	public static BoardCoord OrigCorner( int cornerIndex, int boardRotationStep )
		=> cornerIndex switch {
			0 => BoardCoord.Origin,
			1 => BoardCoord.Angles[boardRotationStep],
			2 => BoardCoord.Angles[boardRotationStep] + BoardCoord.Angles[boardRotationStep + 1],
			3 => BoardCoord.Angles[boardRotationStep + 1],
			_ => throw new IndexOutOfRangeException()
		};

	public static readonly BoardOrientation Home = new BoardOrientation(BoardCoord.Origin,0);

	public BoardOrientation(BoardCoord offset,int rotation ) {
		Offset = offset;
		RotationSteps= rotation % 6;
		var corners = new BoardCoord[4];
		for(int i=0;i<4;++i)
			corners[i] = Offset + OrigCorner( i, RotationSteps );
		Corners = Array.AsReadOnly( corners );

		Sides = new SideCoords[3];
		for(int index=0;index<3;++index)
			Sides[index] = new SideCoords( Corners[index], Corners[index + 1] );
	}

	public BoardCoord Offset { get; }
	public int RotationSteps { get; }

	public string QuickCorners => string.Join("",Corners.Cast<BoardCoord>());

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
	public SideCoords SideCoord( int index ) => new SideCoords( Corners[ index ], Corners[ index + 1 ]);

	public SideCoords[] Sides { get; }
	
	/// <summary> Pre-Reversed because we only use to compare against normal sizes. </summary>
	public SideCoords OceanSideReversed => new SideCoords( Corners[0], Corners[3] );

	/// <summary> Lower left corner of UP-triangle
	public BoardCoord OddCorner => Corners[(new int[] { 0, 3, 3, 2, 1, 1 })[RotationSteps]];
	/// <summary> Upper right corner of DOWN-triangle
	public BoardCoord EvenCorner => Corners[(new int[] { 2, 1, 1, 0, 3, 3 })[RotationSteps]];

	public Matrix3D GetTransformMatrix() {
		int rotate = RotationSteps * 60;
		float dx = Offset.x + 0.5f * Offset.d60;
		float dy = (float)(Offset.d60 * (0.5 * Math.Sqrt( 3 )));
		return RowVector.RotateDegrees( rotate )
			* RowVector.Translate( dx, dy );
	}

}