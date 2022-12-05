namespace SpiritIsland;

public class BuildingEventArgs {

	public BuildingEventArgs(GameState gs,SpaceState[] spacesWithBuildTokens) {
		GameState = gs;
		SpacesWithBuildTokens = spacesWithBuildTokens;
	}

	public GameState GameState { get; }

	public SpaceState[] SpacesWithBuildTokens { get; }

}
