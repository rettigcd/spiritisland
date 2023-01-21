namespace SpiritIsland.BranchAndClaw;

public class PanickedByWildBeasts : FearCardBase, IFearCard {
	public const string Name = "Panicked by Wild Beasts";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 strife in a land with or adjacent to beast" )]
	public Task Level1( GameCtx ctx )
		=> Cmd.AddStrife(1)
			.In().SpiritPickedLand().Which( Has.BeastOrIsAdjacentToBeast )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "Each player adds 1 strife in a land with or adjacent to beast.  Invaders skip their normal explore and build in lands ith beast" )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife( 1 ).In().SpiritPickedLand().Which( Has.BeastOrIsAdjacentToBeast ).ForEachSpirit(),
			Cmd.Multiple(Cmd.Skip1Build(Name),Cmd.Skip1Explore(Name)).In().EachActiveLand().Which( Has.Beast )
		).Execute( ctx );

	[FearLevel( 3, "Each player adds 1 strife in a land with or adjacent to beast.  Invaders skip all normal actions in lands with beast." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife( 1 ).In().SpiritPickedLand().Which( Has.BeastOrIsAdjacentToBeast ).ForEachSpirit(),
			Cmd.SkipAllInvaderActions( Name ).In().EachActiveLand().Which( Has.Beast )
		).Execute( ctx );

}