namespace SpiritIsland.FeatherAndFlame;

class CompoundActionFactory : SpiritAction {

	readonly SpiritAction[] _parts;

	public CompoundActionFactory(params SpiritAction[] parts)
		:base( string.Join( ":", parts.Select( x => x.Description ) ) ) {
		_parts = parts;
	}

	public override async Task ActAsync( Spirit spirit ) {
		foreach(var part in _parts )
			await part.ActAsync( spirit );
	}
}
