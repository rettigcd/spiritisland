namespace SpiritIsland.FeatherAndFlame;

public class ExtendRange : SpiritAction, ICanAutoRun {

	public ExtendRange( int extension ):base( $"Extend Range {extension}" ) {
		_extension = extension;
	}

	public override Task ActAsync( SelfCtx ctx ) {
		RangeCalcRestorer.Save( ctx.Self );
		RangeExtender.Extend( ctx.Self, _extension );
		return Task.CompletedTask;
	}

	readonly int _extension;

}

public class IgnoreRange : SpiritAction, ICanAutoRun {

	public IgnoreRange() : base( $"IgnoreRange" ) {}

	public override Task ActAsync( SelfCtx ctx ) {
		RangeCalcRestorer.Save( ctx.Self );
		RangeExtender.Extend( ctx.Self, 256 );
		return Task.CompletedTask;
	}

}
