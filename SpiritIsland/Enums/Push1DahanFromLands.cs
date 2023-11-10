namespace SpiritIsland;

/// <summary> Pushes 1 dahan from 1 of your lands. </summary>
public class Push1DahanFromLands : SpiritAction {

	public Push1DahanFromLands():base("Push 1 Dahan from Lands" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		var dahanOptions = ctx.Self.Presence.Spaces.Tokens()
			.SelectMany(space=> space.Dahan.NormalKeys.Select(t=>new SpaceToken(space.Space,t)));
		var source = await ctx.SelectAsync(new A.SpaceToken("Select dahan to push from land",dahanOptions,Present.Done));
		if(source == null) return;

		await new TokenPusher( ctx.Self, source.Space.Tokens ).PushToken( source.Token );
	}
}
