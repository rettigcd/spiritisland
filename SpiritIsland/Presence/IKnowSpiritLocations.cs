namespace SpiritIsland;

/// <summary> Feeds the Source Calculator. </summary>
public interface IKnowSpiritLocations {

	IEnumerable<Space> Spaces { get; }
	IEnumerable<SpaceState> SpaceStates { get; }

	IEnumerable<Space> SacredSites { get; } // And Special Rules
	IEnumerable<SpaceState> SacredSiteStates { get;}  // And Special Rules
}