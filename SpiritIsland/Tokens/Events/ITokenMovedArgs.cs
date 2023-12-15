namespace SpiritIsland;

public interface ITokenMovedArgs {
	int Count { get; }

	IToken Removed { get; }
	ILocation From { get; }

	IToken Added { get; }
	ILocation To { get; }
}

