using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	class ExplorersAreReluctant : IFearOptions {
		public const string Name = "Explorers are Reluctant";

		[FearLevel( 1, "During the next normal explore, skip the lowest-numbered land matching the invader card on each board." )]
		public Task Level1( FearCtx ctx ) {
			var gs = ctx.GameState;
			// During the next normal explore, skip the lowest - numbered land matching the invader card on each board.
			gs.PreExplore.ForRound.Add( (gs, args) => {
				// This is correct as long as this runs first.  If something else clears out lowest # lands, this will pick a higher # lands.
				args.SpacesMatchingCards = args.SpacesMatchingCards
					.GroupBy(s=>s.Board)
					.SelectMany(grp=>grp.OrderBy(s=>s.Label).Skip(1))
					.ToList();
				return Task.CompletedTask;
			});
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Skip the next normal explore.  During the next invader phase, draw an adidtional explore card" )]
		public Task Level2( FearCtx ctx ) {
			var gs = ctx.GameState;

			// Skip the next normal explore.  During the next invader phase, draw an additional explore card
			gs.PreExplore.ForRound.Add( ( gs, args ) => {

				// Alternative #1, we could capture the explore card and replay it at the end of the round.
				// Alternative #2, we could make the InvaderDeck flexible where we could pop the card out of the current space and double up a later space.
					// - maybe replace the top card in the deck with a null card, followed by a manufactured 'double' card.

				// !!! this doubles up the explorers the next round, but it DOES NOT double up the BUILD and RAVAGE like it should

				// Capture spaces intended for this round.
				Space[] exploreSpacesForThisRound = args.SpacesMatchingCards.ToArray();
				// clear them out so they don't run this round.
				args.SpacesMatchingCards.Clear();

				// at end of turn, put them back in the list so they run next round.

				// !!! tis is wrong, we need to manage cards

				//gs.TimePasses_ThisRound.Push( ( laterGameState ) => {
				//	laterGameState.ScheduledExplorationSpaces.AddRange( exploreSpacesForThisRound );
				//	return Task.CompletedTask;
				//} );

				return Task.CompletedTask;
			} );
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual." )]
		public Task Level3( FearCtx ctx ) {
			// Skip the next normal explore, but still reveal a card. Perform the flag if relavant. Cards shift left as usual
			ctx.GameState.PreExplore.ForRound.Add( ( gs, args ) => {
				args.SpacesMatchingCards.Clear();
				// !! Not doing flags at the moment....
				return Task.CompletedTask;
			} );
			return Task.CompletedTask;
		}

	}

}
