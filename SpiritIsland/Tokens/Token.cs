namespace SpiritIsland;

public interface Token : IOption {

	TokenClass Class { get; }

}

public interface IAppearInSpaceAbreviation : Token { // !!! If this isn't used many places, maybe not derive from Token
	/// <summary> 
	/// The text to display when showing a summary of the tokens in a space.
	/// null => don't show it in the Token Summary list.
	/// </summary>
	string SpaceAbreviation { get; }
}

public interface TokenWithEndOfRoundCleanup : Token {
	void EndOfRoundCleanup(SpaceState spaceState);
}

//public interface IAppearOnScreen : Token {}

public interface IVisibleToken : Token {
	Img Img { get; }
}