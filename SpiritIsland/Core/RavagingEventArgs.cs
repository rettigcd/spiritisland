namespace SpiritIsland;

public class RavagingEventArgs( GameState _gs ) {
	public GameState GameState { get; } = _gs;
	public List<SpaceState> Spaces;
	public void Skip1(SpaceState space) => Spaces.Remove(space);
}

