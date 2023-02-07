namespace SpiritIsland.BranchAndClaw;

public class Quarantine : FearCardBase, IFearCard {

	public const string Name = "Quarantine";
	public string Text => Name;

	[FearLevel( 1, "Explore does not affect Coastal lands." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.Skip.Explore( Name ).In().EachActiveLand().Which( Is.Coastal )
		.Execute( ctx );

	[FearLevel( 2, "Explore does not affect Coastal lands. Lands with Disease are not a source of Invaders when exploring." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
			Cmd.Skip.Explore( Name ).In().EachActiveLand().Which( Is.Coastal ),
			Cmd.Adjust1Token( "are not a source of invaders when exploring", new SkipExploreFrom() ).In().EachActiveLand().Which( Has.Disease )
		)
		.Execute(ctx);

	[FearLevel( 3, "Explore does not affect Coastal lands. Invaders do not act in lands with Disease." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
			Cmd.Skip.Explore( Name ).In().EachActiveLand().Which( Is.Coastal ),
			Cmd.Skip.AllInvaderActions( Name ).In().EachActiveLand().Which( Has.Disease )
		)
		.Execute( ctx );

}