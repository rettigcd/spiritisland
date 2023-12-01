namespace SpiritIsland.JaggedEarth;

class LetsSeeWhatHappensCtx : TargetSpaceCtx {

	public LetsSeeWhatHappensCtx(TargetSpaceCtx ctx ) : base( ctx.Self, ctx.Space ) {}

	/// <summary> ExecutesAll Options, not just 1 </summary>
	// Does this work on things that operate on Commands???
	public override async Task SelectActionOption( params IActOn<TargetSpaceCtx>[] options ) {
		foreach(var opt in options)
			await opt.ActAsync( this );
	}

}