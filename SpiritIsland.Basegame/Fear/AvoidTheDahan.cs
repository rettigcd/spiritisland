namespace SpiritIsland.Basegame;

public class AvoidTheDahan : FearCardBase, IFearCard {

	public const string Name = "Avoid the Dahan";
	public string Text => Name;

	[FearLevel(1, "Invaders do not Explore into lands with at least 2 Dahan." )]
	public Task Level1( GameCtx ctx )
		=> StopExploreInLandsWithAtLeast2Dahan
			.In().EachActiveLand()
			.ActAsync( ctx );

	static SpaceCmd StopExploreInLandsWithAtLeast2Dahan => new SpaceCmd( "Stop Explore if 2 dahan",
		ctx => {
			var token = new SkipExploreTo_Custom( true, ( space ) => 2 <= space.Dahan.CountAll );
			ctx.Tokens.Adjust( token, 1 );
		}
	);

	[FearLevel( 2, "Invaders do not Build in lands where Dahan outnumber Town/City." )]
	public Task Level2( GameCtx ctx )
		=> StopBuildWhereDahanOutnumberTownsCities
			.In().EachActiveLand()
			.ActAsync( ctx );

	static SpaceCmd StopBuildWhereDahanOutnumberTownsCities => new SpaceCmd( "Stop Build if dahan outnumber towns/cities.",
		ctx => {
			var token = new SkipBuild_Custom( Name, true, ( space ) => space.SumAny( Human.Town_City ) < space.Dahan.CountAll );
			ctx.Tokens.Adjust( token, 1 );
		}
	);


	[FearLevel( 3, "Invaders do not Build in lands with Dahan." )]
	public Task Level3( GameCtx ctx )
		=> DoNotBuildInLandsWithDahan
			.In().EachActiveLand()
			.ActAsync( ctx );

	static SpaceCmd DoNotBuildInLandsWithDahan => new SpaceCmd( "Stop Build in lands with Dahan",
		ctx => {
			var token = new SkipBuild_Custom( Name, true, ( space ) => space.Dahan.Any );
			ctx.Tokens.Adjust( token, 1 );
		}
	);


}
