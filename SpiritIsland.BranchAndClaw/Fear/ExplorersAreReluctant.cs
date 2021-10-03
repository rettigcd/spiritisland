using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ExplorersAreReluctant : IFearOptions {
		public const string Name = "Explorers are Reluctant";

		[FearLevel( 1, "During the next normal explore, skip the lowest-numbered land matching the invader card on each board." )]
		public Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			// During the next normal explore, skip the lowest - numbered land matching the invader card on each board.
			gs.PreExplore.ForThisRound( (gs, args) => {
				var spacesToSkip = args.SpacesMatchingCards
					.GroupBy(s=>s.Board)
					.Select(grp=>grp.OrderBy(s=>s.Label).First()) // lowest #'d land
					.ToList();
				foreach(var s in spacesToSkip)
					args.Skip(s);
				return Task.CompletedTask;
			});
			return Task.CompletedTask;

		}

		[FearLevel( 2, "Skip the next normal explore.  During the next invader phase, draw an adidtional explore card." )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;

			gs.InvaderDeck.drawCount[0] = 0;
			if(gs.InvaderDeck.drawCount.Count>1)
				gs.InvaderDeck.drawCount[1]++;

			return Task.CompletedTask;
		}

		[FearLevel( 3, "Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual." )]
		public Task Level3( FearCtx ctx ) {

			// Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual
			ctx.GameState.PreExplore.ForThisRound( ( gs, args ) => {
				args.SkipAll();
				// !! Not doing flags at the moment....
				return Task.CompletedTask;
			} );
			return Task.CompletedTask;
		}

	}

}
