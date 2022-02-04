namespace SpiritIsland;

public class InvalidPresenceLocationException : Exception {

	public InvalidPresenceLocationException(string invalidSpace,string[] allowed)
		:base($"Invalid:{invalidSpace} allowed:"+allowed.Join(","))
	{}
		
}