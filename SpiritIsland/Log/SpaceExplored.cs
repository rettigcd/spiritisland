namespace SpiritIsland.Log;

public class SpaceExplored( SpaceSpec space ) 
	: InvaderActionEntry( space + ":gains explorer" )
{
	public SpaceSpec Space { get; } = space;
}
