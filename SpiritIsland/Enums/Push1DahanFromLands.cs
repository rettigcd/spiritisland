namespace SpiritIsland;

/// <summary> Pushes 1 dahan from 1 of your lands. </summary>
public class Push1DahanFromLands : GrowthActionFactory, IActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var dahanOptions = ctx.Self.Presence.Spaces.Tokens()
			.SelectMany(space=> space.Dahan.NormalKeys.Select(t=>new SpaceToken(space.Space,t)));
		var source = await ctx.Decision(new Select.ASpaceToken("Select dahan to push from land",dahanOptions,Present.Done));
		if(source == null) return;

		await new TokenPusher( ctx.Self, source.Space.Tokens ).PushToken( source.Token );
	}
}
