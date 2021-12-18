using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	class LetsSeeWhatHappensCtx : TargetSpaceCtx {

		public LetsSeeWhatHappensCtx(TargetSpaceCtx ctx ) : base( ctx, ctx.Space ) {}

		override protected async Task SelectAction_Inner<T>( string prompt, IExecuteOn<T>[] options, Present present, T ctx ) {
			foreach(var opt in options)
				await opt.Execute( ctx );
		}

		public override Task<Space[]> PushUpTo( int countToPush, params TokenCategory[] groups ) 
			=> new TokenPusher( this )
				.AddGroup( countToPush, groups )
				.MoveN();

		public override Task GatherUpTo( int countToGather, params TokenCategory[] ofType )
			=> this.Gather( countToGather, ofType );

	}

}
