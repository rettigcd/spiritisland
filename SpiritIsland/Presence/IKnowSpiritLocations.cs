using System.Collections.Generic;

namespace SpiritIsland {
	// For the Sources Calculator
	public interface IKnowSpiritLocations {
		IEnumerable<Space> Spaces { get; }
		IEnumerable<Space> SacredSites { get; }
	}

}