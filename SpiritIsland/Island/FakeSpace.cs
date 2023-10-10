namespace SpiritIsland;

public class FakeSpace : Space {
	public FakeSpace( string name ) : base( name ) {
		Boards = new Board[0];
	}

	public override int InvaderActionCount => 0;
	public override bool Is( Terrain terrain ) => false;
	public override bool IsOneOf( params Terrain[] options ) => false;
}
