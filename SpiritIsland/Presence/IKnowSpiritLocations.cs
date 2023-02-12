namespace SpiritIsland;

/// <summary> Feeds the Source Calculator. </summary>
public interface IKnowSpiritLocations {

	IEnumerable<SpaceState> SpaceStates { get; }

	IEnumerable<SpaceState> SacredSiteStates { get; } // And Special Rules
}