namespace SpiritIsland.BranchAndClaw;

public class TooManyMonsters : FearCardBase, IFearCard {

	public const string Name = "Too Many Monsters";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 explorer / town from a land with beast." )]
	public Task Level1( GameCtx ctx ) 
		=> Cmd.RemoveExplorersOrTowns(1)
			.In().SpiritPickedLand().Which( Has.Beast )
			.ByPickingToken(Invader.Explorer_Town)
			.ForEachSpirit()
			.Execute(ctx);

	[FearLevel( 2, "Each player removes 1 explorer and 1 town from a land with beast or 1 explorer from a land adjacent to beast" )]
	public Task Level2( GameCtx ctx )
		=> new SpaceAction("remove 1 explorer (+1 Town if land has beasts)", Remove_Level2)
			.In().SpiritPickedLand().Which( Has.BeastOrIsAdjacentToBeast )
			.ForEachSpirit()
			.Execute(ctx);

	[FearLevel( 3, "Each player removes 2 explorers and 2 towns from a land with beast or 1 explorer/town from a land adjacent to beast" )]
	public Task Level3( GameCtx ctx )
		=> new SpaceAction( "removes 2 explorers and 2 towns from a land with beast or 1 explorer/town from a land adjacent to beast", Remove_Level3 )
			.In().SpiritPickedLand().Which( Has.BeastOrIsAdjacentToBeast )
			.ForEachSpirit()
			.Execute( ctx );

	Task Remove_Level2( TargetSpaceCtx ctx ) {
		var remover = new TokenRemover( ctx ).AddGroup( 1, Invader.Explorer );
		if(ctx.Beasts.Any) remover.AddGroup( 1, Invader.Town );
		return remover.RemoveN();
	}

	Task Remove_Level3( TargetSpaceCtx ctx ) {
		var remover = ctx.Beasts.Any
			? new TokenRemover( ctx ).AddGroup( 2, Invader.Explorer ).AddGroup( 2, Invader.Town )
			: new TokenRemover( ctx ).AddGroup( 1, Invader.Explorer_Town );

		return remover.RemoveN();
	}

}