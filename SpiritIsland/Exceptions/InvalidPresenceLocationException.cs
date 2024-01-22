namespace SpiritIsland;

public class InvalidPresenceLocationException( string invalidSpace, string[] allowed ) 
	: Exception($"Invalid:{invalidSpace} allowed:"+allowed.Join(","))
{
}