namespace SpiritIsland;

public class Push1TownOrCityFromLands : GrowthActionFactory, IActionFactory {

	public override async Task ActivateAsync( SelfCtx ctx ) {
		var dahanOptions = ctx.Self.Presence.Spaces.Tokens()
			.SelectMany( space => space.InvaderTokens().Where(t=>t.Class != Human.Explorer).Select( t => new SpaceToken( space.Space, t ) ) );
		var source = await ctx.Decision( new Select.TokenFromManySpaces( "Select town/city to push from land", dahanOptions, Present.Done ) );
		if(source == null) return;

		await new TokenPusher( ctx.Self, source.Space.Tokens ).PushToken( source.Token );
	}
}