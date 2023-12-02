namespace SpiritIsland;

/// <summary> Feeds the Source Calculator. </summary>
public interface IKnowSpiritLocations {
	IEnumerable<Space> Lands { get; }
	IEnumerable<SpaceState> SacredSites { get; } // And Special Rules
	IEnumerable<SpaceState> SuperSacredSites { get; }
}