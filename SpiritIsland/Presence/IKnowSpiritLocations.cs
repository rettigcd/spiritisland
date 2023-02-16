namespace SpiritIsland;

/// <summary> Feeds the Source Calculator. </summary>
public interface IKnowSpiritLocations {

	IEnumerable<Space> Spaces { get; }

	IEnumerable<SpaceState> SacredSites { get; } // And Special Rules
}