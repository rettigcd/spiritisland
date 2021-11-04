using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	class LetsSeeWhatHappensCtx : TargetSpaceCtx {

		public LetsSeeWhatHappensCtx(TargetSpaceCtx ctx ) : base( ctx, ctx.Space ) {}

		public override async Task SelectActionOption( string prompt, params ActionOption[] options ) {
			foreach(var opt in options)
				await opt.Action();
		}

		public override Task<Space[]> PushUpTo( int countToPush, params TokenGroup[] groups ) 
			=> new TokenPusher( this )
				.AddGroup( countToPush, groups )
				.MoveN();

		public override Task GatherUpTo( int countToGather, params TokenGroup[] ofType )
			=> this.Gather( countToGather, ofType );

	}

}
