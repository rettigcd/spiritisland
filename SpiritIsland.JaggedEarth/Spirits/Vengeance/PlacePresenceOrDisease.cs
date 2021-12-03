using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class PlacePresenceOrDisease : PlacePresence {

		public PlacePresenceOrDisease() : base( 1 ) { }

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			if(await ctx.Self.UserSelectsFirstText("Place presence or disease?", "Presence", "Disease" )) {
				await base.ActivateAsync(ctx);
				return;
			}

			Space to = await ctx.Presence.SelectDestinationWithinRange( 1, Target.Any );
			ctx.Target(to).Disease.Add(1);

		}

	}

}
