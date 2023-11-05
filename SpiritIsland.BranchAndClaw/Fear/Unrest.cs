namespace SpiritIsland.BranchAndClaw;

public class Unrest : FearCardBase, IFearCard {
	public const string Name = "Unrest";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 Strife to a Town." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.AddStrifeTo(1,Human.Town)
			.In().SpiritPickedLand()
			.ByPickingToken(Human.Town)
			.ForEachSpirit()
			.ActAsync(ctx);

	[FearLevel( 2, "Each player adds 1 Strife to a Town. For the rest of this turn, Invaders have -1 health per Strife to a minimum of 1." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
				Cmd.AddStrifeTo( 1, Human.Town ).In().SpiritPickedLand().ByPickingToken( Human.Town ).ForEachSpirit(),
				Cmd.StrifePenalizesHealth
			)
			.ActAsync(ctx);

	[FearLevel( 3, "Each player adds 1 Strife to an Invader. For the rest of this turn, Invaders have -1 health per Strife to a minimum of 1." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
				Cmd.AddStrife(1).In().SpiritPickedLand().ByPickingToken( Human.Invader ).ForEachSpirit(),
				Cmd.StrifePenalizesHealth
			)
			.ActAsync( ctx );
}