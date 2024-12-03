namespace SpiritIsland.Basegame;

public class AvoidTheDahan : FearCardBase, IFearCard {

	public const string Name = "Avoid the Dahan";
	public string Text => Name;

	[FearLevel("Invaders do not Explore into lands with at least 2 Dahan." )]
	public override Task Level1( GameState gs )
		=> StopExploreInLandsWithAtLeast2Dahan
			.In().EachActiveLand()
			.ActAsync( gs );

	static SpaceAction StopExploreInLandsWithAtLeast2Dahan => new SpaceAction( "Stop Explore if 2 dahan",
		ctx => {
			var token = new SkipExploreTo_Custom( true, ( space ) => 2 <= space.Dahan.CountAll );
			ctx.Space.Adjust( token, 1 );
		}
	);

	[FearLevel("Invaders do not Build in lands where Dahan outnumber Town/City." )]
	public override Task Level2( GameState gs )
		=> StopBuildWhereDahanOutnumberTownsCities
			.In().EachActiveLand()
			.ActAsync( gs );

	static SpaceAction StopBuildWhereDahanOutnumberTownsCities => new SpaceAction( 
		"Stop Build if dahan outnumber towns/cities.",
		ctx => ctx.Space.Adjust(new SkipBuild_Custom(Name, true, DahanOutnumberCities), 1 )
	);

	static bool DahanOutnumberCities( Space space )
		=> space.SumAny(Human.Town_City) < space.Dahan.CountAll;

	[FearLevel("Invaders do not Build in lands with Dahan." )]
	public override Task Level3( GameState gs )
		=> DoNotBuildInLandsWithDahan
			.In().EachActiveLand()
			.ActAsync( gs );

	static SpaceAction DoNotBuildInLandsWithDahan => new SpaceAction( "Stop Build in lands with Dahan",
		ctx => {
			var token = new SkipBuild_Custom( Name, true, ( space ) => space.Dahan.Any );
			ctx.Space.Adjust( token, 1 );
		}
	);


}
