namespace SpiritIsland;

/// <summary> Feeds the Source Calculator. </summary>
public interface IKnowSpiritLocations {
	IEnumerable<Space> Spaces { get; }
	IEnumerable<Space> SacredSites( TerrainMapper mapper ); // And Special Rules
}