namespace SpiritIsland.Basegame;

public class TradeSuffers : FearCardBase, IFearCard {

	public const string Name = "Trade Suffers";
	public string Text => Name;

	[FearLevel( 1, "Invaders do not Build in lands with City." )]
	public override Task Level1( GameState ctx )
		=> new SpaceAction("Invaders do not build", StopBuild)
			.In().EachActiveLand()
			.ActAsync( ctx );

	[FearLevel( 2, "Each player may replace 1 Town with 1 Explorer in a Coastal land." )]
	public override Task Level2( GameState ctx )
		=> new SpaceAction("replace 1 town with 1 explorer", ctx=> ReplaceInvader.Downgrade1( ctx.Self, ctx.Space, Present.Done, Human.Town ) )
			.In().SpiritPickedLand().Which( Is.Coastal )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 3, "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
	public override Task Level3( GameState ctx )
		=> new SpaceAction( "replace 1 City with 1 Town or replace 1 town with 1 explorer", ctx => ReplaceInvader.Downgrade1( ctx.Self, ctx.Space, Present.Done, Human.Town_City ) )
			.In().SpiritPickedLand().Which( Is.Coastal )
			.ForEachSpirit()
			.ActAsync( ctx );

	static void StopBuild( TargetSpaceCtx ctx ) {
		ctx.Space.Adjust( new SkipBuild_Custom( Name, true, ( space ) => space.HasAny( Human.City ) ), 1);
	}

}