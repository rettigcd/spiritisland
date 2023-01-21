namespace SpiritIsland.BranchAndClaw;

class TreadCarefully : FearCardBase, IFearCard {

	public const string Name = "Tread Carefully";
	public string Text => Name;

	[FearLevel( 1, "Each player may choose a land with dahan or adjacent to at least 5 dahan.  Invaders do not ravage there this turn." )]
	public Task Level1( GameCtx ctx ) => StopRavage
			.In().SpiritPickedLand().Which( Has.DahanOrAdjacentTo( 5 ) ).MakeOptional()
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "Each player may choose a land with dahan or adjacent to at least 3 dahan.  Invaders do not ravage there this turn." )]
	public Task Level2( GameCtx ctx ) => StopRavage
			.In().SpiritPickedLand().Which( Has.DahanOrAdjacentTo( 3 ) ).MakeOptional()
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 3, "Each player may choose a land with dahan or adjacent to dahan.  Invaders do not ravage their this turn." )]
	public Task Level3( GameCtx ctx ) => StopRavage
			.In().SpiritPickedLand().Which( Has.DahanOrAdjacentTo( 1 ) ).MakeOptional()
			.ForEachSpirit()
			.Execute( ctx );

	readonly SpaceAction StopRavage = new SpaceAction( "Invaders do not ravage there this turn.", ctx => {
		// Skip twice in case there are 2 ravage cards. !!!
		ctx.Skip1Ravage( Name );
		ctx.Skip1Ravage( Name );
	} );

}