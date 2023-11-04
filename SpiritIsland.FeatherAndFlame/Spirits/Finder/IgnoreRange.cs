namespace SpiritIsland.FeatherAndFlame;

public class ExtendRange : GrowthActionFactory {

	public ExtendRange(int extension) {
		_extension = extension;
	}

	public override string Name => $"Extend Range {_extension}";

	public override bool AutoRun => true;

	readonly int _extension;

	public override Task ActivateAsync( SelfCtx ctx ) {
		RangeCalcRestorer.Save( ctx.Self );
		RangeExtender.Extend( ctx.Self, _extension );
		return Task.CompletedTask;
	} 

}

public class IgnoreRange : ExtendRange {

	public override string Name => "IgnoreRange";

	public IgnoreRange() : base( 256 ) { }

}
