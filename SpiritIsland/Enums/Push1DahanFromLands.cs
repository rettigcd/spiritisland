namespace SpiritIsland;

/// <summary> Pushes 1 dahan from 1 of your lands. </summary>
public class Push1DahanFromLands : GrowthActionFactory, IActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var dahanOptions = ctx.Self.Presence.Spaces
			.SelectMany(space=>ctx.Target(space).Dahan.Keys.Select(t=>new SpaceToken(space,t)));
		var source = await ctx.Decision(new Select.TokenFromManySpaces("Select dahan to push from land",dahanOptions,Present.Done));
		if(source == null) return;

		await new TokenPusher( ctx.Target(source.Space) ).PushToken( source.Token );
	}
}