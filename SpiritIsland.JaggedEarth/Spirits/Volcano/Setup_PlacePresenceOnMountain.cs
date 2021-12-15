using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	class Setup_PlacePresenceOnMountain : GrowthActionFactory { // Similar to SharpFang initialization

		public override async Task ActivateAsync( SelfCtx ctx ) {
			// Put 1 presence on your starting board in a mountain of your choice.
			var options = ctx.AllSpaces.Where( space=>space.Terrain == Terrain.Mountain );
			var space = await ctx.Self.Action.Decision(new Decision.TargetSpace("Add presence to",options, Present.Always));
			ctx.Presence.PlaceOn(space);

			// Push all dahan from that land.
			var targetCtx = ctx.Target(space);
			if(targetCtx.Dahan.Any)
				await targetCtx.PushDahan(targetCtx.Dahan.Count);
		}

	}


}
