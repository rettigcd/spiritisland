namespace SpiritIsland.BranchAndClaw;

class TreadCarefully : FearCardBase, IFearCard {

	public const string Name = "Tread Carefully";
	public string Text => Name;

	[FearLevel( 1, "Each player may choose a land with Dahan or adjacent to at least 5 Dahan. Invaders do not Ravage there this turn." )]
	public override Task Level1( GameState ctx ) => Cmd.Skip.AllRavages( Name )
			.In().SpiritPickedLand().Which( Has.DahanOrAdjacentTo( 5 ) ).MakeOptional()
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 2, "Each player may choose a land with Dahan or adjacent to at least 3 Dahan. Invaders do not Ravage there this turn." )]
	public override Task Level2( GameState ctx ) => Cmd.Skip.AllRavages( Name )
			.In().SpiritPickedLand().Which( Has.DahanOrAdjacentTo( 3 ) ).MakeOptional()
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 3, "Each player may choose a land with Dahan or adjacent to Dahan. Invaders do not Ravage their this turn." )]
	public override Task Level3( GameState ctx ) => Cmd.Skip.AllRavages( Name )
			.In().SpiritPickedLand().Which( Has.DahanOrAdjacentTo( 1 ) ).MakeOptional()
			.ForEachSpirit()
			.ActAsync( ctx );

}