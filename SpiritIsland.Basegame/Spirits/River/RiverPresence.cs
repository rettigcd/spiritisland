namespace SpiritIsland.Basegame;

public class RiverPresence( Spirit spirit, PresenceTrack t1, PresenceTrack t2 ) 
	: SpiritPresence( spirit, t1, t2 ) 
{
	public override bool IsSacredSite( Space space ) => base.IsSacredSite( space ) 
		|| (1 <= space[Token] && TerrainMapper.Current.MatchesTerrain( space, Terrain.Wetland ));
}