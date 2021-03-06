namespace SpiritIsland.JaggedEarth;

public class PlacePresenceOrDisease : PlacePresence {

	public PlacePresenceOrDisease() : base( 1 ) { }

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(await ctx.Self.UserSelectsFirstText("Place presence or disease?", "Presence", "Disease" )) {
			await base.ActivateAsync(ctx);
			return;
		}

		Space to = await ctx.Presence.SelectDestinationWithinRange( 1, Target.Any );
		await ctx.Target(to).Disease.Add(1);

	}

}