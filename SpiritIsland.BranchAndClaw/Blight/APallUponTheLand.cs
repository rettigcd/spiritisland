using System.Linq;

namespace SpiritIsland.BranchAndClaw {

	public class APallUponTheLand : BlightCardBase {

		public APallUponTheLand():base("A Pall Upon the Land", 3 ) { }

		public override ActionOption<GameState> Immediately => 
			// On Each Board
			Cmd.OnEachBoard(
				Cmd.Multiple(
					// destroy 1 presence 
					Destroy1PresenceOnBoard,
					// remove 1 town
					Cmd.RemoveTowns(1).InAnyLandOnBoard()
				)
			);

		static ActionOption<BoardCtx> Destroy1PresenceOnBoard 
			=> new ActionOption<BoardCtx>("Destroy 1 presence.", async ctx => {
				// !! this could be any spirits presence - Who picks?   When having multiple players, this should be a parallel decision where spirit volunteers
				// for now, just have the 1st spirit pick (House Rules advantage)
				bool IsOnBoard(Space space) => space.Board == ctx.Board;
				var spiritOptions = ctx.GameState.Spirits
					.Where( s=> s.Presence.Placed.Any(IsOnBoard))
					.ToArray();
				if(spiritOptions.Length == 0) return;
				var spirit = spiritOptions.Length == 1 ? spiritOptions[0]
					: await ctx.Decision( new Select.Spirit( "Destroy 1 presence.", spiritOptions ) );
				await new TargetSpiritCtx( ctx.Self, ctx.GameState, spirit, Cause.Fear )
					.Presence.DestroyOne( ActionType.BlightedIsland, IsOnBoard );
			} );

	}
}
