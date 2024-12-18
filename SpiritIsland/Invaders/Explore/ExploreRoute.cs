namespace SpiritIsland;

public class ExploreRoute {
	public required Space Source;
	public required Space Destination;
	public bool IsValid => Source == Destination || Source.IsConnected && Destination.IsConnected;
}


