namespace SpiritIsland.BranchAndClaw;

public class PanickedByWildBeasts : FearCardBase, IFearCard {
	public const string Name = "Panicked by Wild Beasts";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 Strife in a land with or adjacent to Beast." )]
	public Task Level1( GameState ctx )
		=> Cmd.AddStrife(1)
			.In().SpiritPickedLand().Which( Has.BeastOrIsAdjacentToBeast )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 2, "Each player adds 1 Strife in a land with or adjacent to Beast. Invaders skip their normal Explore and Build in lands with Beast." )]
	public Task Level2( GameState ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife(1).In().SpiritPickedLand().Which(Has.BeastOrIsAdjacentToBeast).ForEachSpirit(),
			Cmd.Multiple(Cmd.Skip.Build(Name),Cmd.Skip.Explore(Name)).In().EachActiveLand().Which( Has.Beast )
		).ActAsync( ctx );

	[FearLevel( 3, "Each player adds 1 Strife in a land with or adjacent to Beast. Invaders skip all normal actions in lands with Beast." )]
	public Task Level3( GameState ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife(1).In().SpiritPickedLand().Which(Has.BeastOrIsAdjacentToBeast).ForEachSpirit(),
			Cmd.Skip.AllInvaderActions( Name ).In().EachActiveLand().Which( Has.Beast )
		).ActAsync( ctx );

}