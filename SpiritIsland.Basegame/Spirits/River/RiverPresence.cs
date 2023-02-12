namespace SpiritIsland.Basegame;

public class RiverPresence : SpiritPresence {
	public RiverPresence( PresenceTrack t1, PresenceTrack t2 ) : base( t1, t2 ) { }

	public override bool IsSacredSite( SpaceState space ) => base.IsSacredSite( space ) 
		|| (1 <= space[Token] && TerrainMapper.Current.MatchesTerrain( space, Terrain.Wetland ));

}