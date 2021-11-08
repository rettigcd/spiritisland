
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class Setup_PlacePresenceInCostal : GrowthActionFactory {

		// ! Can't used normal PlacePresence, because it must be range-1, range 0 not allowed.
		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var options = ctx.Self.Presence.Spaces.First().Adjacent;
			var space = await ctx.Self.Action.Decision( new Decision.TargetSpace( "Add presence to", options, Present.Always ) );
			ctx.Presence.PlaceOn( space );
		}

	}

}
