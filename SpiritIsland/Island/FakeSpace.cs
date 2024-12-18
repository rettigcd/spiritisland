#nullable enable
namespace SpiritIsland;

public class FakeSpace : SpaceSpec {

	public FakeSpace(string name, SpaceLayout layout) : base(name, []) {
		_layout = layout;
	}

	public FakeSpace( string name ) : base( name, [] ) {
		_layout = null;
	}

	public override SpaceLayout Layout {
		get {
			return _layout is not null ? _layout : throw new InvalidOperationException($"FakeSpace '{Label}' has no layout.");
		}
	}
	readonly SpaceLayout? _layout;

	public override int InvaderActionCount => 0;
	public override bool Is( Terrain terrain ) => false;
	public override bool IsOneOf( params Terrain[] options ) => false;
}

#nullable disable