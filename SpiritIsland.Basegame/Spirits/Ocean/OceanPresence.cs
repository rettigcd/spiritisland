namespace SpiritIsland.Basegame;

public class OceanPresence : SpiritPresence {

	public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) {
		Token = new OceanToken();
	}

	public override bool CanBePlacedOn( SpaceState s, TerrainMapper tm ) 
		=> tm.MatchesTerrain( s, Terrain.Ocean ) || tm.IsCoastal( s );

	public override void Adjust( SpaceState space, int count ) {
		space.Adjust( Token, count );
		((OceanToken)Token).Adjust(space.Space,count);
	}

	public bool IsOnBoard(Board board) => ((OceanToken) Token).IsOnBoard(board); // _currentBoards.Contains(board);


}

public class OceanToken : SpiritPresenceToken, IHandleTokenAdded, IHandleTokenRemoved {
	public OceanToken() {
		_currentBoards = new HashSet<Board>();
	}

	public Task HandleTokenAdded( ITokenAddedArgs args ) {
		if(args.Token!=this) return Task.CompletedTask;
		
		_currentBoards.Add(args.AddedTo.Space.Board);
		_spaces[args.AddedTo.Space] += args.Count;

		return Task.CompletedTask;
	}
	public Task HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(args.Token==this) {
			_spaces[args.RemovedFrom.Space] -= args.Count;
			CheckIfBoardStillPresent( args.RemovedFrom.Space.Board );
		}
		return Task.CompletedTask;
	}

	public void Adjust( Space space, int delta) { 
		_spaces[space] += delta;
		if(0 <= delta)
			_currentBoards.Add(space.Board);
		else
			CheckIfBoardStillPresent( space.Board );
	}

	void CheckIfBoardStillPresent( Board board ) {
		if(!_spaces.Keys.Any( s => s.Board == board ))
			_currentBoards.Remove( board );
	}

	public bool IsOnBoard( Board board ) => _currentBoards.Contains( board );

	readonly CountDictionary<Space> _spaces = new CountDictionary<Space>(); // !!! Save state in memento
	readonly HashSet<Board> _currentBoards;
}