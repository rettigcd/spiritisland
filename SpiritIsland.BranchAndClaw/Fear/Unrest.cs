namespace SpiritIsland.BranchAndClaw;

public class Unrest : FearCardBase, IFearCard {
	public const string Name = "Unrest";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 strife to a town." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.AddStrifeTo(1,Invader.Town)
			.In().SpiritPickedLand()
			.ByPickingToken(Invader.Town)
			.ForEachSpirit()
			.Execute(ctx);

	[FearLevel( 2, "Each player adds 1 strife to a town.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
				Cmd.AddStrifeTo( 1, Invader.Town ).In().SpiritPickedLand().ByPickingToken( Invader.Town ).ForEachSpirit(),
				Cmd.StrifePenalizesHealth
			)
			.Execute(ctx);

	[FearLevel( 3, "Each player adds 1 strife to an invader.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
				Cmd.AddStrife(1).In().SpiritPickedLand().ByPickingToken( Invader.Any ).ForEachSpirit(),
				Cmd.StrifePenalizesHealth
			)
			.Execute( ctx );
}