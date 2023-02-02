namespace SpiritIsland;

public class BoardSide {
	public Board Board { get; }
	public bool Joined { get; private set; }

	public BoardSide( Board board, params Space[] spaces){
		this.Board = board;
		this.spaces = spaces;
	}

	public void BreakAt(params int[] breakPoints){
		if(breakPoints.Length != spaces.Length-1)
			throw new System.InvalidOperationException("there must be 1 fewer break point than spaces");
		this.breakPoints = breakPoints.ToList();
		this.breakPoints.Insert(0,0);
		this.breakPoints.Add(13);
	}

	/// <summary>
	/// Connects this (new) Board to an existing Placed Board.
	/// Moves this boards layout to match coordinate system on existingBoard
	/// </summary>
	public void ConnectTo(BoardSide existingBoard,bool moveLayoutToMatchOther){
				
		// reverse other
		var otherSpaces = existingBoard.spaces.Reverse().ToArray();
		var otherBreakPoints = existingBoard.breakPoints.Select(i=>13-i).Reverse().ToList();

		var thisSpaces = this.spaces;
		var thisBreakPoints = this.breakPoints.ToList();

		int thisIndex = 0;
		int otherIndex = 0;
		do{
			// current territories are adjacent
			thisSpaces[thisIndex].SetAdjacentToSpaces(otherSpaces[otherIndex]);
			// advance whichever board is shorter 
			if(thisBreakPoints[thisIndex+1] < otherBreakPoints[otherIndex+1])
				thisIndex++;
			else
				otherIndex++;
		} while(thisIndex<thisBreakPoints.Count-2 || otherIndex<otherBreakPoints.Count-2 );
		thisSpaces[thisIndex].SetAdjacentToSpaces(otherSpaces[otherIndex]);

		if(moveLayoutToMatchOther)
			this.MoveLayoutTo( existingBoard );

		Joined = true;
		existingBoard.Joined = true;
	}


	public void MoveLayoutTo( BoardSide stationarySide ) {
		var movingSide = this;
		// =================
		// Remap the layout 
		// =================
		int stationarySideIndex = Array.IndexOf( stationarySide.Board.Sides, stationarySide );
		int movingSideIndex = Array.IndexOf( movingSide.Board.Sides, movingSide );

		var stationaryLayout = stationarySide.Board.OriginalLayout;
		var movingLayout = movingSide.Board.OriginalLayout;

		var stationaryTarget = stationaryLayout.boardCorners[ stationarySideIndex ]; // 1st point on the side
		var movingAlignmentPoint = movingLayout.boardCorners[ movingSideIndex + 1 ]; // 2nd point on the side - the point we are going to move to the stationaryTarget

		double rotate = stationaryLayout.SideRotationDegrees( stationarySideIndex ) // target angle
			- movingLayout.SideRotationDegrees( movingSideIndex ) // less angle already turned
			+ 180.0; // moving board we use 2nd side point, we need to point backwards to 1st point

		var transform = RowVector.Translate( -movingAlignmentPoint.X, -movingAlignmentPoint.Y ) // move alignment point to (0,0) so we can rotate it
			* RowVector.RotateDegrees( rotate )                                 // rotate it
			* RowVector.Translate( stationaryTarget.X, stationaryTarget.Y );    // move aligment point out to target
		var mapper = new PointMapper( transform );

		movingLayout.ReMap( mapper );
	}

	readonly Space[] spaces;
	List<int> breakPoints;
}
