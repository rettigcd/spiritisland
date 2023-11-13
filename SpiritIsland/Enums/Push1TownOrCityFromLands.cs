namespace SpiritIsland;

public class Push1TownOrCityFromLands : SpiritAction {

	public Push1TownOrCityFromLands():base( "Push1TownOrCityFromLands" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		var dahanOptions = ctx.Self.Presence.Spaces.Tokens()
			.SelectMany( spaceState 
				=> spaceState.InvaderTokens()
					.Where(t=>t.Class != Human.Explorer)
					.On(spaceState.Space)
			);
		var source = await ctx.SelectAsync( new A.SpaceToken( "Select town/city to push from land", dahanOptions, Present.Done ) );
		if(source == null) return;

		await source.Space.Tokens.Pusher(ctx.Self).MoveSomewhereAsync( source );
	}
}