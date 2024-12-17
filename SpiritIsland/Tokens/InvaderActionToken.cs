namespace SpiritIsland;

/// <summary>
/// Generic implementation class used for marking Explores, Builds, and Ravages.
/// </summary>
public class InvaderActionToken( string label ) : ISpaceEntity {
	public string Label { get; } = label;


	// Fake Tokens that are not visible.
	static readonly public InvaderActionToken DoExplore = new InvaderActionToken("Explore");
	static readonly public InvaderActionToken DoBuild = new InvaderActionToken("Build");
	static readonly public InvaderActionToken DoRavage = new InvaderActionToken("Ravage");

}