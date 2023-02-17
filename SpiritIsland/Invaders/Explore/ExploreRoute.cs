namespace SpiritIsland;

public class ExploreRoute {
	public SpaceState Source;
	public SpaceState Destination;
	public bool IsValid => Source == Destination || Source.IsConnected && Destination.IsConnected;
}


