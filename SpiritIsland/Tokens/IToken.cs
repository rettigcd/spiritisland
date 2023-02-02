namespace SpiritIsland;

public interface IToken { 
	
	// !!! Should IToken really implement IOption??? Maybe IVisibleToken should implement it instead. When are we going to select an invisible token?

	TokenClass Class { get; }

}
