namespace SpiritIsland.FeatherAndFlame;

public class ExtendRange( int extension ) 
	: SpiritAction( $"Extend Range {extension}" )
	, ICanAutoRun
{
	public override Task ActAsync( Spirit self ) {
		RangeExtender.Extend( self, _extension );
		return Task.CompletedTask;
	}

	readonly int _extension = extension;

}

public class IgnoreRange : SpiritAction, ICanAutoRun {

	public IgnoreRange() : base( $"Ignore Range" ) {}

	public override Task ActAsync( Spirit self ) {
		RangeExtender.Extend( self, 256 );
		return Task.CompletedTask;
	}

}
