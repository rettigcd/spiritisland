using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class APallUponTheLand : BlightCardBase {

		public APallUponTheLand():base("A Pall Upon the Land", 3 ) { }

		protected override Task BlightAction( GameState gs ) {
			// On Each Board
			return GameCmd.OnEachBoard(
				Cmd.Multiple(
					// destroy 1 presence 
					Destory1PresenceOnBoard,
					// remove 1 town
					BoardCmd.PickSpaceThenTakeAction("Remove 1 Town." ,Cmd.RemoveTowns(1) ,_ => true )
				)
			).Execute( gs );

		}

		static ActionOption<BoardCtx> Destory1PresenceOnBoard 
			=> new ActionOption<BoardCtx>("Destroy 1 presence", async ctx => {
				// !! this could be any spirits presence - Who picks?   When having multiple players, this should be a parallel decision where spirit volunteers
				// for now, just have the 1st spirit pick (House Rules advantage)
				bool IsOnBoard(Space space) => space.Board == ctx.Board;
				var spiritOptions = ctx.GameState.Spirits
					.Where( s=> s.Presence.Placed.Any(IsOnBoard))
					.ToArray();
				if(spiritOptions.Length == 0) return;
				var spirit = spiritOptions.Length == 1 ? spiritOptions[0]
					: await ctx.Decision( new Select.Spirit( "Destroy 1 presence", spiritOptions ) );
				await new TargetSpiritCtx( ctx.Self, ctx.GameState, spirit, Cause.Fear )
					.Presence.DestoryOne( ActionType.BlightedIsland, IsOnBoard );
			} );

	}
}
