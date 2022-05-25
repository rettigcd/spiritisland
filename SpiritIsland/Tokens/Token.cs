namespace SpiritIsland;

public interface Token : IOption {

	TokenClass Class { get; } // originally: readonly

	public char Initial { get; }

}