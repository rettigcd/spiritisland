namespace SpiritIsland.JaggedEarth;

public class VengeanceSpace( Space src ) : Space( src ) {
	public override TokenBinding Badlands => new WreakVengeanceForTheLandsCorruption( this );
}
