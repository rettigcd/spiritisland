namespace SpiritIsland;

public class RavagingEventArgs( GameState _gs ) {
	public GameState GameState { get; } = _gs;
	public required List<Space> Spaces;
	public void Skip1(Space space) => Spaces.Remove(space);
}

