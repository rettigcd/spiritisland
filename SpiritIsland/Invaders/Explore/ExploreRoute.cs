namespace SpiritIsland;

public class ExploreRoute {
	public Space Source;
	public Space Destination;
	public bool IsValid => Source == Destination || Source.IsConnected && Destination.IsConnected;
}


