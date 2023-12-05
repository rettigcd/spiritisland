namespace SpiritIsland.BranchAndClaw;

public class Panic : FearCardBase, IFearCard {
	public const string Name = "Panic";
	public string Text => Name;

	[FearLevel( 1, "Each player adds 1 Strife in a land with Beast/Disease/Dahan." )]
	public Task Level1( GameState ctx )
		=> Cmd.AddStrife( 1 )
			.In().SpiritPickedLand().Which( Has.BeastDiseaseOrDahan )
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 2, "Each player adds 1 Strife in a land with Beast/Disease/Dahan. For the rest of this turn, Invaders have -1 health per Strife to a minimum of 1." )]
	public Task Level2( GameState ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife( 1 ).In().SpiritPickedLand().Which( Has.BeastDiseaseOr2Dahan ).ForEachSpirit(),
			Cmd.StrifePenalizesHealth
		).ActAsync( ctx );

	[FearLevel( 3, "Each player adds 1 Strife to an Invader. For the rest of this turn, Invaders have -1 health per Strife to a minimum of 1." )]
	public Task Level3( GameState ctx )
		=> Cmd.Multiple(
			Cmd.AddStrife( 1 ).In().SpiritPickedLand().ForEachSpirit(),
			Cmd.StrifePenalizesHealth
		).ActAsync( ctx );


}