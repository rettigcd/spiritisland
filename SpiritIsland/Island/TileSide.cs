namespace SpiritIsland;

public class TileSide {
	public Board Board { get; }
	public TileSide( Board board, params Space[] spaces){
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
			
	public void IsAdjacentTo(TileSide other){
				
		// reverse other
		var otherSpaces = other.spaces.Reverse().ToArray();
		var otherBreakPoints = other.breakPoints.Select(i=>13-i).Reverse().ToList();

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
	}


	public void MoveLayoutTo( TileSide stationarySide ) {
		var movingSide = this;
		// =================
		// Remap the layout 
		// =================
		int stationarySideIndex = Array.IndexOf( stationarySide.Board.Sides, stationarySide );
		int movingSideIndex = Array.IndexOf( movingSide.Board.Sides, movingSide );

		var stationaryLayout = stationarySide.Board.Layout;
		var movingLayout = movingSide.Board.Layout;

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
