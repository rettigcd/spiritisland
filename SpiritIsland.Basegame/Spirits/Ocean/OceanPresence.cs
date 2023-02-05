namespace SpiritIsland.Basegame;

public class OceanPresence : SpiritPresence {

	public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) {
	}

	public override bool CanBePlacedOn( SpaceState s, TerrainMapper tm ) 
		=> tm.MatchesTerrain( s, Terrain.Ocean ) || tm.IsCoastal( s );

	public override void Adjust( SpaceState space, int count ) {
		space.Adjust( Token, count );
		Token.Adjust(space.Space,count);
	}

	public bool IsOnBoard(Board board) => Token.IsOn(board);


}