namespace SpiritIsland.BranchAndClaw;

public class DahanThreaten : FearCardBase, IFearCard {

	public const string Name = "Dahan Threaten";
	public string Text => Name;

	[FearLevel( 1, "each player adds 1 strife in a land with dahan" )]
	public Task Level1( GameCtx ctx )
		=> Cmd.AddStrife(1)
			.In()
			.SpiritPickedLand()
			.Which( Has.Dahan )
			.ByPickingToken( Invader.Any )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "each player adds 1 strife in a land with dahan. For the rest of this turn, invaders have -1 health per strife to a minimum of 1" )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
				Cmd.AddStrife( 1 ).In().SpiritPickedLand().Which( Has.Dahan ).ForEachSpirit(),
				Cmd.StrifePenalizesHealth
			).Execute( ctx );

	[FearLevel( 3, "Each player adds 1 strife in a land with dahan.  In every land with strife, 1 damage per dahan" )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife( 1 ).In().SpiritPickedLand().Which( Has.Dahan ).ForEachSpirit(),
			Cmd.OneDamagePerDahan.In().EachActiveLand().Which( Has.Strife )
		).Execute( ctx );

}