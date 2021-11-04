using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	class PlacePresenceAndBeast : GrowthActionFactory {
		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var from = await ctx.SelectPresenceSource();
			Space to = await ctx.SelectSpaceWithinRangeOfCurrentPresence( 3, Target.Any );
			await ctx.Self.Presence.PlaceFromBoard( from, to, ctx.GameState );
			ctx.Target(to).Beasts.Count++;
		}
	}

}
