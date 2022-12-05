namespace SpiritIsland;

public class RavagingEventArgs {
	public RavagingEventArgs( GameState gs ) { 
		GameState = gs;
	}
	public GameState GameState { get; }
	public List<SpaceState> Spaces;
	public void Skip1(SpaceState space) => Spaces.Remove(space);
}

