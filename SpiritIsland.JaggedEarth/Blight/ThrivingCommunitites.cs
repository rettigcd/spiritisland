namespace SpiritIsland.JaggedEarth;

public class ThrivingCommunitites : BlightCard {

	public ThrivingCommunitites():base( "Thriving Communitites", "Immediately, on each board: In 4 different lands with explorer/town, Replace 1 town with 1 city or Replace 1 explorer with 1 town.", 4) {}

	public override IActOn<GameState> Immediately 
		=> UpgradeExplorerOrTown
		.In().NDifferentLandsPerBoard( 4 ) // filter is on Command
		.ForEachBoard();

	static SpaceAction UpgradeExplorerOrTown => new SpaceAction( "Replace 1 town with a city or Replace 1 explorer with 1 town", async ctx=>{
		var spaceToken = await ctx.Self.SelectAsync( An.Invader.ToReplace("upgrade",ctx.Space.HumanOfAnyTag(Human.Explorer_Town).On(ctx.Space)) );
		if(spaceToken is null) return; // no explorer or town
		var oldInvader = spaceToken.Token.AsHuman();

		var newTokenClass = (oldInvader.Class == Human.Explorer)
			? Human.Town
			: Human.City;

		await ctx.Space.ReplaceHumanAsync( oldInvader, newTokenClass );


	} ).OnlyExecuteIf( x => x.Space.HasAny( Human.Explorer_Town ) );

}