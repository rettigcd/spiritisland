namespace SpiritIsland;

public class BuildingEventArgs {

	public BuildingEventArgs(GameState gs,SpaceState[] spacesWithBuildTokens) {
		GameState = gs;
		SpacesWithBuildTokens = spacesWithBuildTokens;
	}

	public GameState GameState { get; }

	public SpaceState[] SpacesWithBuildTokens { get; }

}

public class RavagingEventArgs {
	public RavagingEventArgs(GameState gs, Guid actionId ) { 
		GameState = gs;
		ActionId = actionId;
	}
	public GameState GameState { get; }
	public List<SpaceState> Spaces;
	public void Skip1(SpaceState space) => Spaces.Remove(space);
	public Guid ActionId { get; }
}

