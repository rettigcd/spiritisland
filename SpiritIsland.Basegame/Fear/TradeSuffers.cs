namespace SpiritIsland.Basegame;

public class TradeSuffers : FearCardBase, IFearCard {

	public const string Name = "Trade Suffers";
	public string Text => Name;

	[FearLevel( 1, "Invaders do not Build in lands with City." )]
	public Task Level1( GameCtx ctx )
		=> new SpaceAction("Invaders do not build", StopBuild)
			.In().EachActiveLand().Which( Has.City )
			.Execute( ctx );

	[FearLevel( 2, "Each player may replace 1 Town with 1 Explorer in a Coastal land." )]
	public Task Level2( GameCtx ctx )
		=> new SpaceAction("replace 1 town with 1 explorer", ctx=> ReplaceInvader.Downgrade( ctx, Present.Done, Human.Town ) )
			.In().SpiritPickedLand().Which( Is.Coastal )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 3, "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
	public Task Level3( GameCtx ctx )
		=> new SpaceAction( "replace 1 City with 1 Town or replace 1 town with 1 explorer", ctx => ReplaceInvader.Downgrade( ctx, Present.Done, Human.Town_City ) )
			.In().SpiritPickedLand().Which( Is.Coastal )
			.ForEachSpirit()
			.Execute( ctx );

	static void StopBuild( TargetSpaceCtx ctx ) {
		ctx.Tokens.Adjust( new SkipBuild_Custom( Name, true, ( space ) => space.HasAny( Human.City ) ), 1);
	}

}