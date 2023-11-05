namespace SpiritIsland.FeatherAndFlame;

public class Depopulation : FearCardBase, IFearCard {
	public const string Name = "Depopulation";
	public string Text => Name;

	[FearLevel( 1, "On Each Board: Replace 1 Town with 1 Explorer." )]
	public Task Level1( GameCtx ctx )
		=> Replace1TownWith1Explorer
			.In().OneLandPerBoard()
			.ByPickingToken( Human.Town )
			.ForEachBoard()
			.ActAsync( ctx );

	static SpaceCmd Replace1TownWith1Explorer => new SpaceCmd("Replace 1 Town with 1 Explorer", ctx => ReplaceInvader.Downgrade1( ctx, Present.Done, Human.Town ) );

	[FearLevel( 2, "On Each Board: Remove 1 Town." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.RemoveTowns( 1 )
			.In().OneLandPerBoard()
			.ByPickingToken( Human.Town )
			.ForEachBoard()
			.ActAsync( ctx );

	[FearLevel( 3, "On Each Board: Remove 1 Town, or Replace 1 City with 1 Town." )]
	public Task Level3( GameCtx ctx )
		=> DownGradeCityOrRemoveTown // ctx.SelectActionOption( Cmd.RemoveTowns( 1 ), Replace1CityWith1Town ) )
			.In().OneLandPerBoard()
			.ByPickingToken( Human.Town_City )
			.ForEachBoard()
			.ActAsync( ctx );

	// static SpaceCmd Replace1CityWith1Town => new SpaceCmd( "Replace 1 City with 1 Town", ctx => ReplaceInvader.Downgrade1( ctx, Present.Done, Human.City ) );

	static SpaceCmd DownGradeCityOrRemoveTown => new SpaceCmd("Remove 1 Town, or Replace 1 City with 1 Town", async ctx => {
		const string prompt = "Select City to downgrade or Town to remove";
		var options = ctx.Tokens.OfAnyHumanClass( Human.Town_City );
		var invader = await ctx.Decision(new Select.ASpaceToken(prompt,ctx.Space,options,Present.Always));
		if(invader == null) return;
		if(invader.Token.Class == Human.City)
			await ReplaceInvader.DowngradeSelectedInvader(ctx.Tokens,(HumanToken)invader.Token);
		else // must be town
			await ctx.Tokens.Remove(invader.Token,1);
	} );

}


