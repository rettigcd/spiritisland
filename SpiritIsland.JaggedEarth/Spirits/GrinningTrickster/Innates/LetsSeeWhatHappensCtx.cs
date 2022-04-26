namespace SpiritIsland.JaggedEarth;

class LetsSeeWhatHappensCtx : TargetSpaceCtx {

	public LetsSeeWhatHappensCtx(TargetSpaceCtx ctx ) : base( ctx, ctx.Space ) {}

	/// <summary>
	/// Operates All Options, not just 1
	/// </summary>
	// Does this work things that operate on Commands???
	override protected async Task SelectAction_Inner<T>( string prompt, IExecuteOn<T>[] options, Present present, T ctx ) {
		foreach(var opt in options)
			await opt.Execute( ctx );
	}

	/// <summary>
	/// Change PushUpTo to PushAll
	/// </summary>
	public override Task<Space[]> PushUpTo( int countToPush, params TokenClass[] groups ) 
		=> new TokenPusher( this )
			.AddGroup( countToPush, groups )
			.MoveN();

	/// <summary>
	/// Change GatherUpTO to GatherAll
	/// </summary>
	public override Task GatherUpTo( int countToGather, params TokenClass[] ofType )
		=> this.Gather( countToGather, ofType );

}