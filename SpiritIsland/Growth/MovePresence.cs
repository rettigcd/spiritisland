using System.Threading.Tasks;

namespace SpiritIsland {

	public class MovePresence : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx) {
			var self = ctx.Self;
			var from = await self.Action.Decision( new Decision.Presence.Deployed("Move presence from:", self ) );
			var to = await self.Action.Decision( new Decision.AdjacentSpace("Move preseence to:", from, Decision.AdjacentDirection.Outgoing, from.Adjacent));
			self.Presence.Move( from, to );
		}
	}


}
