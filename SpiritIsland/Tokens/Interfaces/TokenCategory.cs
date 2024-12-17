namespace SpiritIsland;

/// <summary>
/// Values that can be attached to Token to help more easily filter them.
/// </summary>
public class TokenCategory : ITag {

	TokenCategory(string tagName ) { Label = tagName; }
	public string Label { get; }


	static public readonly TokenCategory Category_None = new TokenCategory("[none]");
	
	// All visible Invaders - Includes Dreaming Invaders
	// Don't use as substitute for Explorer, Town, City because Dreaming Invaders have this too.
	static public readonly TokenCategory Invader = new TokenCategory("Invader");
	static public readonly TokenCategory Dahan = new TokenCategory("Dahan");
	static public readonly TokenCategory Presence = new TokenCategory("Presence");
	static public readonly TokenCategory Incarna = new TokenCategory("Incarna");
}



