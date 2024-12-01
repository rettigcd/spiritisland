namespace SpiritIsland.BranchAndClaw;

public class Demoralized : FearCardBase, IFearCard {

	public const string Name = "Demoralized";
	public string Text => Name;

	[FearLevel( 1, "Defend 1 in all lands." )]
	public override Task Level1( GameState ctx )
		=> Cmd.Defend(1).In().EachActiveLand().ActAsync( ctx );

	[FearLevel( 2, "Defend 2 in all lands." )]
	public override Task Level2( GameState ctx )
		=> Cmd.Defend( 2 ).In().EachActiveLand().ActAsync( ctx );

	[FearLevel( 3, "Defend 3 in all lands." )]
	public override Task Level3( GameState ctx )
		=> Cmd.Defend( 2 ).In().EachActiveLand().ActAsync( ctx );

}