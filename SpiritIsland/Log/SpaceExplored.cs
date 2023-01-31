namespace SpiritIsland.Log;

public class SpaceExplored : InvaderActionEntry {
	public SpaceExplored( Space space ) : base( space + ":gains explorer" ) { Space = space; }
	public Space Space { get; }
}
