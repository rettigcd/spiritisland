using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	class PlacePresenceAndBeast : GrowthActionFactory {

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			var from = await ctx.Presence.SelectSource();
			Space to = await ctx.Presence.SelectDestinationWithinRange( 3, Target.Any );
			await ctx.Self.Presence.PlaceFromTracks( from, to, ctx.GameState );
			ctx.Target(to).Beasts.Count++;
		}

	}

}
