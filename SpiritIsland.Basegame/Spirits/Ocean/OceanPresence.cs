namespace SpiritIsland.Basegame;

public class OceanPresence : SpiritPresence {

	public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) {}

	public override bool CanBePlacedOn( SpaceState s ) {
		var tm = UnitOfWork.Current.TerrainMapper;
		return tm.MatchesTerrain( s, Terrain.Ocean ) || tm.IsCoastal( s );
	}

}