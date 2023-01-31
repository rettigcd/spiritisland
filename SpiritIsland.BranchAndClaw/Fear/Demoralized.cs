namespace SpiritIsland.BranchAndClaw;

public class Demoralized : FearCardBase, IFearCard {

	public const string Name = "Demoralized";
	public string Text => Name;

	[FearLevel( 1, "Defend 1 in all lands." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.Defend(1).In().EachActiveLand().Execute( ctx );

	[FearLevel( 2, "Defend 2 in all lands." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Defend( 2 ).In().EachActiveLand().Execute( ctx );

	[FearLevel( 3, "Defend 3 in all lands." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Defend( 2 ).In().EachActiveLand().Execute( ctx );

}