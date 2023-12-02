namespace SpiritIsland;

// !!! Is there any IToken that does not implement this Interface?  If not, roll this into IToken

public interface IAppearInSpaceAbreviation : ISpaceEntity {
	/// <summary> 
	/// The text to display when showing a summary of the tokens in a space.
	/// null => don't show it in the Token Summary list.
	/// </summary>
	string SpaceAbreviation { get; }
}
