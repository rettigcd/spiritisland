namespace SpiritIsland.JaggedEarth;

public class ThrivingCommunitites : BlightCardBase {

	public ThrivingCommunitites():base("Thriving Communitites", "Immediately, on each board: In 4 different lands with explorer/town, Replace 1 town with 1 city or Replace 1 explorer with 1 town.", 4) {}

	public override DecisionOption<GameCtx> Immediately 
		=> UpgradeExplorerOrTown
		.In().NDifferentLands( 4 ) // filter is on Command
		.ForEachBoard();

	static SpaceAction UpgradeExplorerOrTown => new SpaceAction( "Replace 1 town with a city or Replace 1 explorer with 1 town", async ctx=>{
		var spaceToken = await ctx.Decision( Select.Invader.ToReplace("upgrade",ctx.Space,ctx.Tokens.OfAnyHealthClass(Invader.Explorer_Town)));
		await ctx.Remove(spaceToken.Token,1,RemoveReason.Replaced);
		// !!! if select invader with strife, we should keep the strife.
		var newTokenClass = (spaceToken.Token.Class == Invader.Explorer)
			? Invader.Town
			: Invader.City;
		await ctx.AddDefault( newTokenClass, 1, AddReason.AsReplacement );
	}).OnlyExecuteIf( x => x.Tokens.HasAny( Invader.Explorer_Town ) );

}