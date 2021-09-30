using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ImmigrationSlows : IFearOptions {
		public const string Name = "Immigration Slows";

		[FearLevel( 1, "During the next normal build, skip the lowest numbered land matching the invader card on each board." )]
		public Task Level1( FearCtx ctx ) {

			// During the next normal build, skip the lowest numbered land matching the invader card on each board
			var spacesToSkip = ctx.GameState.ScheduledBuildSpaces.Keys
				.GroupBy( s => s.Board )
				.SelectMany( grp => grp.OrderBy( x => x.Label ).Take( 1 ) )
				.ToArray();

			foreach(var space in spacesToSkip)
				ctx.GameState.Skip1Build(space);

			return Task.CompletedTask;
		}

		[FearLevel( 2, "Skip the next normal build. The build card remains in place instead of shifting left." )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;
			// Skip the next normal build.  The build card remains in place instead of shifting left

			// clear this round
			gs.ScheduledBuildSpaces.Clear();
			gs.InvaderDeck.KeepBuildCards = true;

			return Task.CompletedTask;
		}

		[FearLevel( 3, "Skip the next normal build.  The build card shifts left as usual." )]
		public Task Level3( FearCtx ctx ) {
			var gs = ctx.GameState;
			// Skip the next normal build.  The build card shifts left as usual
			gs.ScheduledBuildSpaces.Clear();
			return Task.CompletedTask;
		}

	}

}
