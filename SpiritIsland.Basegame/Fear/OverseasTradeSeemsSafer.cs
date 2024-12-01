namespace SpiritIsland.Basegame;

public class OverseasTradeSeemsSafer : FearCardBase, IFearCard {

	public const string Name = "Overseas Trade Seems Safer";
	string IOption.Text => Name;

	[FearLevel( 1, "Defend 3 in all Coastal lands." )]
	public override Task Level1( GameState ctx )
		=> Cmd.Defend( 3 )
			.In().EachActiveLand().Which( Is.Coastal )
			.ActAsync( ctx );


	[FearLevel( 2, "Defend 6 in all Coastal lands. Invaders do not Build City in Coastal lands this turn." )]
	public override Task Level2( GameState ctx )
		=> Cmd.Multiple( Cmd.Defend(6), DoNotBuildCity )
			.In().EachActiveLand().Which( Is.Coastal )
			.ActAsync( ctx );

	[FearLevel( 3, "Defend 9 in all Coastal lands. Invaders do not Build in Coastal lands this turn." )]
	public override Task Level3( GameState ctx )
		=> Cmd.Multiple( Cmd.Defend( 9 ), DoNotBuild )
			.In().EachActiveLand().Which( Is.Coastal )
			.ActAsync( ctx );

	static SpaceAction DoNotBuildCity => new SpaceAction( "do not build city", ctx => ctx.Space.SkipAllBuilds( $"{Name}(city)", Human.City ) );
	static SpaceAction DoNotBuild => new SpaceAction( "do not build city", ctx => ctx.Space.SkipAllBuilds( Name ) );

}