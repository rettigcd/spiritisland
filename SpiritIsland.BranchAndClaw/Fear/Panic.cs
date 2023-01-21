namespace SpiritIsland.BranchAndClaw;

public class Panic : FearCardBase, IFearCard {
	public const string Name = "Panic";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 strife in a land with beast/disease/dahan." )]
	public Task Level1( GameCtx ctx )
		=> Cmd.AddStrife( 1 )
			.In().SpiritPickedLand().Which( Has.BeastDiseaseOrDahan )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "Each player adds 1 strife in a land with beast/disease/dahan.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1" )]
	public Task Level2( GameCtx ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife( 1 ).In().SpiritPickedLand().Which( Has.BeastDiseaseOr2Dahan ).ForEachSpirit(),
			Cmd.StrifePenalizesHealth
		).Execute( ctx );

	[FearLevel( 3, "Each player adds 1 strife to an invader.  For the rest of this turn, invaders have -1 health per strife to a minimum of 1." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife( 1 ).In().SpiritPickedLand().ForEachSpirit(),
			Cmd.StrifePenalizesHealth
		).Execute( ctx );


}