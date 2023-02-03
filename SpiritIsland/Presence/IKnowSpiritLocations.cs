namespace SpiritIsland;

/// <summary> Feeds the Source Calculator. </summary>
public interface IKnowSpiritLocations {

	IEnumerable<SpaceState> ActiveSpaceStates { get; }

	IEnumerable<SpaceState> SacredSites { get; } // And Special Rules
}