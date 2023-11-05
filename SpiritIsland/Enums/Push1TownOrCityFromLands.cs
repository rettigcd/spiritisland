namespace SpiritIsland;

public class Push1TownOrCityFromLands : SpiritAction {

	public Push1TownOrCityFromLands():base( "Push1TownOrCityFromLands" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		var dahanOptions = ctx.Self.Presence.Spaces.Tokens()
			.SelectMany( space => space.InvaderTokens().Where(t=>t.Class != Human.Explorer).Select( t => new SpaceToken( space.Space, t ) ) );
		var source = await ctx.Decision( new Select.ASpaceToken( "Select town/city to push from land", dahanOptions, Present.Done ) );
		if(source == null) return;

		await new TokenPusher( ctx.Self, source.Space.Tokens ).PushToken( source.Token );
	}
}