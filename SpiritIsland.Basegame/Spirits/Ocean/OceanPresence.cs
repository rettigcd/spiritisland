namespace SpiritIsland.Basegame;

public class OceanPresence : SpiritPresence {

	public OceanPresence( Spirit spirit, PresenceTrack energy, PresenceTrack cardPlays ) : base( spirit, energy, cardPlays ) {}

	public override bool CanBePlacedOn( SpaceState s ) {
		var tm = ActionScope.Current.TerrainMapper;
		return tm.MatchesTerrain( s, Terrain.Ocean ) || tm.IsCoastal( s );
	}

}