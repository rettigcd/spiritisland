namespace SpiritIsland.JaggedEarth;

public class VengeanceSpaceState : SpaceState {
	public VengeanceSpaceState( SpaceState src ) : base( src ) { }
	public override TokenBinding Badlands => new WreakVengeanceForTheLandsCorruption( this );
}
