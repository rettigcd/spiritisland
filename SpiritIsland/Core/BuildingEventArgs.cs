using System.Collections.Generic;

namespace SpiritIsland {
	public class BuildingEventArgs {

		public BuildingEventArgs(GameState gs, Dictionary<Space,BuildType> buildTypes ) {
			GameState = gs;
			BuildTypes = buildTypes;
		}

		public GameState GameState { get; }

		public CountDictionary<Space> SpaceCounts;
		public Dictionary<Space,BuildType> BuildTypes { get; }
		public enum BuildType { TownsAndCities, TownsOnly, CitiesOnly }

		public void Skip1(Space space ) { SpaceCounts[ space ]--; }
		public void Add(Space space ) { SpaceCounts[ space ]++; }

		public BuildingEventArgs.BuildType GetBuildType( Space space ) {
			return BuildTypes.ContainsKey( space )
				? BuildTypes[ space ]
				: BuildingEventArgs.BuildType.TownsAndCities;
		}


	}

	public class RavagingEventArgs {
		public RavagingEventArgs(GameState gs ) { GameState = gs; }
		public GameState GameState { get; }
		public List<Space> Spaces;
		public void Skip1(Space space) => Spaces.Remove(space);
	}

}
