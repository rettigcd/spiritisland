namespace SpiritIsland.FeatherAndFlame;

class CompoundActionFactory( params SpiritAction[] _parts ) 
	: SpiritAction( string.Join( ":", _parts.Select( x => x.Description ) ) ) 
{
	public override async Task ActAsync( Spirit spirit ) {
		foreach(var part in _parts )
			await part.ActAsync( spirit );
	}
}
