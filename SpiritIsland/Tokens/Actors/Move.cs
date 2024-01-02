namespace SpiritIsland;

public class Move : IOption {
	public SpaceToken Source;
	public Space Destination;
	string IOption.Text => $"{Source.Text} => {Destination}";
}

