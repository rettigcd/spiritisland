
namespace SpiritIsland.FeatherAndFlame;

public class Depopulation : FearCardBase, IFearCard {
	public const string Name = "Depopulation";
	public string Text => Name;

	[FearLevel( 1, "On Each Board: Replace 1 Town with 1 Explorer." )]
	public Task Level1( GameCtx ctx )
		=> Replace1TownWith1Explorer
			.In().OneLandPerBoard()
			.ByPickingToken( Invader.Explorer )
			.ForEachBoard()
			.Execute( ctx );

	static SpaceAction Replace1TownWith1Explorer => new SpaceAction("Replace 1 Town with 1 Explorer", ctx => ReplaceInvader.Downgrade( ctx, Present.Done, Invader.Town ) );

	[FearLevel( 2, "On Each Board: Remove 1 Town." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.RemoveTowns( 1 )
			.In().OneLandPerBoard()
			.ByPickingToken( Invader.Town )
			.ForEachBoard()
			.Execute( ctx );

	[FearLevel( 3, "On Each Board: Remove 1 Town, or Replace 1 City with 1 Town." )]
	public Task Level3( GameCtx ctx )
		=> new SpaceAction( "Remove 1 Town or Replace 1 City with 1 Town", ctx => ctx.SelectActionOption( Cmd.RemoveTowns( 1 ), Replace1CityWith1Town ) )
			.In().OneLandPerBoard()
			.ByPickingToken( Invader.Town_City )
			.ForEachBoard()
			.Execute( ctx );

	static SpaceAction Replace1CityWith1Town => new SpaceAction( "Replace 1 City with 1 Town", ctx => ReplaceInvader.Downgrade( ctx, Present.Done, Invader.City ) );

}


