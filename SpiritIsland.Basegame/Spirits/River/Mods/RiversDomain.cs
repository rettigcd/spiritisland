namespace SpiritIsland.Basegame;

public class RiversDomain(Spirit spirit) : ICreateSacredSites {

	public const string Name = "Rivers Domain";
	const string Description = "Your presense in wetlands count as sacred.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);


	public bool IsSacredSite(Space space) {
		return (1 <= space[spirit.Presence.Token] && TerrainMapper.Current.MatchesTerrain(space, Terrain.Wetland));
	}
}