namespace SpiritIsland.JaggedEarth;

class LetsSeeWhatHappensCtx : TargetSpaceCtx {

	public LetsSeeWhatHappensCtx(TargetSpaceCtx ctx ) : base( ctx, ctx.Space ) {}

	/// <summary> ExecutesAll Options, not just 1 </summary>
	// Does this work on things that operate on Commands???
	public override async Task SelectActionOption( params IExecuteOn<TargetSpaceCtx>[] options ) {
		foreach(var opt in options)
			await opt.Execute( this );
	}

}