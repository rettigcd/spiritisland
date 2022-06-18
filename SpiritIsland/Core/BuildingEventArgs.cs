namespace SpiritIsland;

public class BuildingEventArgs {

	public BuildingEventArgs(GameState gs,Space[] spacesWithBuildTokens) {
		GameState = gs;
		SpacesWithBuildTokens = spacesWithBuildTokens;
	}

	public GameState GameState { get; }

	public Space[] SpacesWithBuildTokens { get; }

}

public class RavagingEventArgs {
	public RavagingEventArgs(GameState gs, Guid actionId ) { 
		GameState = gs;
		ActionId = actionId;
	}
	public GameState GameState { get; }
	public List<Space> Spaces;
	public void Skip1(Space space) => Spaces.Remove(space);
	public Guid ActionId { get; }
}

