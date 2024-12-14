namespace SpiritIsland.Basegame;

public class RiversDomain( Spirit spirit, PresenceTrack t1, PresenceTrack t2 ) 
	: SpiritPresence( spirit, t1, t2 ) 
{

	public const string Name = "Rivers Domain";
	const string Description = "Your presense in wetlands count as sacred.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	public override bool IsSacredSite( Space space ) => base.IsSacredSite( space ) 
		|| (1 <= space[Token] && TerrainMapper.Current.MatchesTerrain( space, Terrain.Wetland ));

}