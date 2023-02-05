namespace SpiritIsland.Basegame;

public class OceanPresence : SpiritPresence {

	public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) {}

	public override bool CanBePlacedOn( SpaceState s, TerrainMapper tm ) => tm.MatchesTerrain( s, Terrain.Ocean ) || tm.IsCoastal( s );

}