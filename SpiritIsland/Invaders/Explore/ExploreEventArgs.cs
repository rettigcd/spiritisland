namespace SpiritIsland;

public class ExploreRoute {
	public SpaceState Source;
	public SpaceState Destination;
	public bool IsValid => Source == Destination || Source[TokenType.Isolate] == 0 && Destination[TokenType.Isolate] == 0;
}


