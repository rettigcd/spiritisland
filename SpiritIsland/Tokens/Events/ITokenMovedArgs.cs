namespace SpiritIsland;

public interface ITokenMovedArgs {
	int Count { get; }

	IToken Removed { get; }
	Space From { get; }

	IToken Added { get; }
	Space To { get; }
}

