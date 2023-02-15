namespace SpiritIsland;

public interface ITokenMovedArgs {
	int Count { get; }

	IToken Removed { get; }
	SpaceState From { get; }

	IToken Added { get; }
	SpaceState To { get; }
}

