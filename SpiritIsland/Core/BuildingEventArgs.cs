using System.Collections.Generic;

namespace SpiritIsland {
	public class BuildingEventArgs {

		public CountDictionary<Space> SpaceCounts;
		public Dictionary<Space,BuildType> BuildTypes;
		public enum BuildType { TownsAndCities, TownsOnly, CitiesOnly }

		public void Skip1(Space space ) { SpaceCounts[ space ]--; }
		public void Add(Space space ) { SpaceCounts[ space ]++; }

	}

}
