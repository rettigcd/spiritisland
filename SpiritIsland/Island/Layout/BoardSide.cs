using SpiritIsland;

namespace SpiritIsland;

/// <summary>
/// Knows about Connected spaces but not about layout coordinates
/// </summary>
public class BoardSide( Board _board, params SpaceSpec[] _orderedSpacesOnThisSide ) {
	public Board Board { get; } = _board;
	public bool Joined { get; private set; }

	public void BreakAt( params int[] breakPoints ) {
		if(breakPoints.Length != _spacesOnThisSide.Length - 1)
			throw new InvalidOperationException( "there must be 1 fewer break point than spaces" );
		_breakPoints = [..breakPoints];
		_breakPoints.Insert( 0, 0 );
		_breakPoints.Add( 13 );
	}

	/// <summary>
	/// Connects this (new) Board to an existing Placed Board.
	/// Moves this boards layout to match coordinate system on existingBoard
	/// </summary>
	public void ConnectTo( BoardSide existingBoard ) {

		// reverse other
		var otherSpaces = existingBoard._spacesOnThisSide.Reverse().ToArray();
		var otherBreakPoints = existingBoard._breakPoints.Select( i => 13 - i ).Reverse().ToList();

		var thisSpaces = _spacesOnThisSide;
		var thisBreakPoints = _breakPoints.ToList();

		int thisIndex = 0;
		int otherIndex = 0;
		do {
			// current territories are adjacent
			thisSpaces[thisIndex].SetAdjacentToSpaces( otherSpaces[otherIndex] );
			// advance whichever board is shorter 
			if(thisBreakPoints[thisIndex + 1] < otherBreakPoints[otherIndex + 1])
				thisIndex++;
			else
				otherIndex++;
		} while(thisIndex < thisBreakPoints.Count - 2 || otherIndex < otherBreakPoints.Count - 2);
		thisSpaces[thisIndex].SetAdjacentToSpaces( otherSpaces[otherIndex] );

		Joined = true;
		existingBoard.Joined = true;
	}

	readonly SpaceSpec[] _spacesOnThisSide = _orderedSpacesOnThisSide;
	List<int> _breakPoints = [];
}