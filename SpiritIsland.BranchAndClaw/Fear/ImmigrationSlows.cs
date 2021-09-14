using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class ImmigrationSlows : IFearOptions {
		public const string Name = "Immigration Slows";

		[FearLevel( 1, "During the next normal build, skip the lowest numbered land matching the invader card on each board" )]
		public Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			// During the next normal build, skip the lowest numbered land matching the invader card on each board
			var spacesToSkip = gs.ScheduledBuildSpaces.Keys
				.GroupBy( s => s.Board )
				.SelectMany( grp => grp.OrderBy( x => x.Label ).Take( 1 ) )
				.ToArray();

			foreach(var space in spacesToSkip)
				gs.ScheduledBuildSpaces[space]--;

			return Task.CompletedTask;
		}

		[FearLevel( 2, "Skip the next normal build.  The build card remains in place instead of shifting left" )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			// Skip the next normal build.  The build card remains in place instead of shifting left

			// !!! this doubles up the cards for build but does not double them up for RAVAGE the next turn.

			// Save for later
			var copy = gs.ScheduledBuildSpaces.ToDictionary(p=>p.Key,p=>p.Value);

			// clear this round
			gs.ScheduledBuildSpaces.Clear();

			// pre load for next build
			gs.TimePasses_ThisRound.Push( laterGameState => {
				foreach(var pair in copy)
					laterGameState.ScheduledBuildSpaces[pair.Key] += pair.Value;
				return Task.CompletedTask;
			} );

			return Task.CompletedTask;
		}

		[FearLevel( 3, "Skip the next normal build.  The build card shifts left as usual" )]
		public Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			// Skip the next normal build.  The build card shifts left as usual
			gs.ScheduledBuildSpaces.Clear();
			return Task.CompletedTask;
		}

	}

}
