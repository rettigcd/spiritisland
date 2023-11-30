namespace SpiritIsland;

public class Push1TownOrCityFromLands : SpiritAction {

	public Push1TownOrCityFromLands():base( "Push1TownOrCityFromLands" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		await new SourceSelector( ctx.Self.Presence.Lands.Tokens() )
			.AddGroup(1,Human.Town_City)
			.PushN( ctx.Self );
	}
}