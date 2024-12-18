namespace SpiritIsland.FeatherAndFlame;

public class Depopulation : FearCardBase, IFearCard {
	public const string Name = "Depopulation";
	public string Text => Name;

	[FearLevel( 1, "On Each Board: Replace 1 Town with 1 Explorer." )]
	public override Task Level1( GameState ctx )
		=> Replace1TownWith1Explorer
			.In().OneLandPerBoard()
			.ByPickingToken( Human.Town )
			.ForEachBoard()
			.ActAsync( ctx );

	static SpaceAction Replace1TownWith1Explorer => new SpaceAction("Replace 1 Town with 1 Explorer", ctx => ReplaceInvader.Downgrade1( ctx.Self, ctx.Space, Present.Done, Human.Town ) );

	[FearLevel( 2, "On Each Board: Remove 1 Town." )]
	public override Task Level2( GameState ctx )
		=> Cmd.RemoveTowns( 1 )
			.In().OneLandPerBoard()
			.ByPickingToken( Human.Town )
			.ForEachBoard()
			.ActAsync( ctx );

	[FearLevel( 3, "On Each Board: Remove 1 Town, or Replace 1 City with 1 Town." )]
	public override Task Level3( GameState ctx )
		=> DownGradeCityOrRemoveTown // ctx.SelectActionOption( Cmd.RemoveTowns( 1 ), Replace1CityWith1Town ) )
			.In().OneLandPerBoard()
			.ByPickingToken( Human.Town_City )
			.ForEachBoard()
			.ActAsync( ctx );

	static SpaceAction DownGradeCityOrRemoveTown => new SpaceAction("Remove 1 Town, or Replace 1 City with 1 Town", async ctx => {
		const string prompt = "Select City to downgrade or Town to remove";
		var options = ctx.Space.HumanOfAnyTag( Human.Town_City );
		var invader = await ctx.Self.SelectAsync(new A.SpaceTokenDecision(prompt, options.On( ctx.Space ),Present.Always));
		if(invader is null) return;
		if(invader.Token.HasTag(Human.City))
			await ReplaceInvader.DowngradeSelectedInvader(ctx.Space,(HumanToken)invader.Token);
		else // must be town
			await ctx.Space.RemoveAsync(invader.Token,1);
	} );

}


