
namespace SpiritIsland.JaggedEarth;

public class PlacePresenceOrDisease : PlacePresence {

	public PlacePresenceOrDisease() : base( 1 ) { }

	public override async Task ActivateAsync( SelfCtx ctx ) {
		if(await ctx.Self.UserSelectsFirstText("Place presence or disease?", "Presence", "Disease" )) {
			await base.ActivateAsync(ctx);
			return;
		}

		var options = DefaultRangeCalculator.Singleton.GetTargetOptionsFromKnownSource(ctx.Self.Presence.Spaces.Tokens(), new TargetCriteria(1));
		Space to = await ctx.Self.Gateway.Decision( Select.ASpace.ToPlacePresence( options, Present.Always, Token.Disease ) );

		await ctx.Target(to).Disease.AddAsync(1);

	}

}