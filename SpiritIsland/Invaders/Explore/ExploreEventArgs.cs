namespace SpiritIsland;

public class ExploreRoute {
	public SpaceState Source;
	public SpaceState Destination;
	public bool IsValid => Source == Destination || Source[Token.Isolate] == 0 && Destination[Token.Isolate] == 0;
}


