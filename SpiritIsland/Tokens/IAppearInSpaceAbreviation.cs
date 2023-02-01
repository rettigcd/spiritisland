namespace SpiritIsland;

public interface IAppearInSpaceAbreviation : IToken { // !!! If this isn't used many places, maybe not derive from Token
	/// <summary> 
	/// The text to display when showing a summary of the tokens in a space.
	/// null => don't show it in the Token Summary list.
	/// </summary>
	string SpaceAbreviation { get; }
}
