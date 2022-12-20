namespace SpiritIsland.Basegame;

public class OceanPresence : SpiritPresence {

	public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) {}

	public override bool CanBePlacedOn( SpaceState s, TerrainMapper tm ) 
		=> tm.MatchesTerrain( s, Terrain.Ocean ) || tm.IsCoastal( s.Space );

	public override async Task PlaceOn( SpaceState space, UnitOfWork actionId ) {
		await base.PlaceOn( space, actionId );
		currentBoards.Add( space.Space.Board );
	}

	public override void Adjust( SpaceState space, int count ) {
		space.Adjust( Token, count );
		if(space[Token]>0)
			currentBoards.Add( space.Space.Board );
		else
			currentBoards.Remove( space.Space.Board );
	}


	protected override async Task RemoveFrom_NoCheck( SpaceState space ) {
		await base.RemoveFrom_NoCheck( space );
		var board = space.Board;
		bool isOnBoard = board.Spaces.Any( IsOn );
		if(!isOnBoard )
			currentBoards.Remove( board.Board );
	}

	public bool IsOnBoard(Board board) => currentBoards.Contains(board);

	readonly HashSet<Board> currentBoards = new HashSet<Board>();

}