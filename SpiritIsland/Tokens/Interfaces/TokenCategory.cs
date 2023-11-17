namespace SpiritIsland;

public interface ITag { }

/// <summary>
/// Values that can be attached to Token to help more easily filter them.
/// </summary>
public class TokenCategory : ITag {

	static public readonly TokenCategory Category_None = new TokenCategory();
	
	// All visible Invaders - Includes Dreaming Invaders
	// Don't use as substitute for Explorer, Town, City because Dreaming Invaders have this too.
	static public readonly TokenCategory Invader = new TokenCategory();
	static public readonly TokenCategory Dahan = new TokenCategory();
	static public readonly TokenCategory Presence = new TokenCategory();
	static public readonly TokenCategory Incarna = new TokenCategory();
}



