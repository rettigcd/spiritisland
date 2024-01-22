namespace SpiritIsland.Log;

public class SpaceExplored( Space space ) 
	: InvaderActionEntry( space + ":gains explorer" )
{
	public Space Space { get; } = space;
}
