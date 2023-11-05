namespace SpiritIsland.BranchAndClaw;

public class DahanThreaten : FearCardBase, IFearCard {

	public const string Name = "Dahan Threaten";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 Strife in a land with Dahan." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.AddStrife(1)
			.In()
			.SpiritPickedLand()
			.Which( Has.Dahan )
			.ByPickingToken( Human.Invader )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 2, "Each player adds 1 Strife in a land with Dahan. For the rest of this turn, Invaders have -1 health per Strife to a minimum of 1." )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
				Cmd.AddStrife( 1 ).In().SpiritPickedLand().Which( Has.Dahan ).ForEachSpirit(),
				Cmd.StrifePenalizesHealth
			).ActAsync( ctx );

	[FearLevel( 3, "Each player adds 1 Strife in a land with Dahan. In every land with Strife, 1 Damage per Dahan." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife( 1 ).In().SpiritPickedLand().Which( Has.Dahan ).ForEachSpirit(),
			Cmd.OneDamagePerDahan.In().EachActiveLand().Which( Has.Strife )
		).ActAsync( ctx );

}