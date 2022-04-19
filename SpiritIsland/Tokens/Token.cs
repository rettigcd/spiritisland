namespace SpiritIsland;

public interface Token : IOption {

	TokenClass Class { get; } // originally: readonly

	string Summary { get; }

	public char Initial { get; }

	int RemainingHealth {get;}

	int FullHealth {get; }

}