namespace SpiritIsland;

/// <summary> Pushes 1 dahan from 1 of your lands. </summary>
public class Push1DahanFromLands : GrowthActionFactory, IActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var dahanOptions = ctx.Presence.ActiveSpaceStates
			.SelectMany(space=> space.Dahan.NormalKeys.Select(t=>new SpaceToken(space.Space,t)));
		var source = await ctx.Decision(new Select.TokenFromManySpaces("Select dahan to push from land",dahanOptions,Present.Done));
		if(source == null) return;

		await new TokenPusher( ctx.Target(source.Space) ).PushToken( source.Token );
	}
}
