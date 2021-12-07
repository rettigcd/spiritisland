using System.Threading.Tasks;

namespace SpiritIsland {

	public class MovePresence : GrowthActionFactory, ITrackActionFactory {

		public bool RunAfterGrowthResult => true; // might receive additional presence

		public override async Task ActivateAsync( SpiritGameStateCtx ctx) {
			var from = await ctx.Self.Action.Decision( new Decision.Presence.Deployed("Move presence from:", ctx.Self ) );
			var to = await ctx.Self.Action.Decision( new Decision.AdjacentSpace("Move preseence to:", from, Decision.AdjacentDirection.Outgoing, from.Adjacent));
			ctx.Presence.Move( from, to );
		}
	}


}
