namespace SpiritIsland.JaggedEarth;

public class VengeanceSpaceState( SpaceState src ) : SpaceState( src ) {
	public override TokenBinding Badlands => new WreakVengeanceForTheLandsCorruption( this );
}
