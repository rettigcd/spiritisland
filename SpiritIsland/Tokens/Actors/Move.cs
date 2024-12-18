namespace SpiritIsland;

public class Move : IOption {
	public required SpaceToken Source;
	public required Space Destination;
	string IOption.Text => $"{Source} => {Destination.Label}";
}

