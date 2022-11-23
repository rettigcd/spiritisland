namespace SpiritIsland.Basegame;

public class OceanPresence : SpiritPresence {

	public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) {}

	public override bool CanBePlacedOn( SpaceState s, TerrainMapper tm ) => tm.MatchesTerrain( s, Terrain.Ocean ) || tm.IsCoastal( s.Space );

	public override async Task PlaceOn( SpaceState space ) {
		await base.PlaceOn( space );
		currentBoards.Add( space.Space.Board );
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