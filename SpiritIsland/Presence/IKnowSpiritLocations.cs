namespace SpiritIsland;

/// <summary> Feeds the Source Calculator. </summary>
public interface IKnowSpiritLocations {

	IEnumerable<Space> Spaces( GameState gs );
	IEnumerable<SpaceState> SpaceStates(GameState gs);

	IEnumerable<Space> SacredSites( GameState gs, TerrainMapper mapper ); // And Special Rules
	IEnumerable<SpaceState> SacredSiteStates( GameState gs, TerrainMapper mapper ); // And Special Rules
}