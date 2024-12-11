namespace SpiritIsland;

/// <summary> Feeds the Source Calculator. </summary>
public interface IKnowSpiritLocations {
	IEnumerable<Space> Lands { get; }
	IEnumerable<Space> SacredSites { get; } // And Special Rules
//	IEnumerable<Space> SuperSacredSites { get; }
}