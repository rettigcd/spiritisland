namespace SpiritIsland;

public class FakeSpace : SpaceSpec {
	public FakeSpace( string name ) : base( name ) {
		Boards = [];
	}

	public override int InvaderActionCount => 0;
	public override bool Is( Terrain terrain ) => false;
	public override bool IsOneOf( params Terrain[] options ) => false;
}
