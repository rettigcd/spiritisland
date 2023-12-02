namespace SpiritIsland.BranchAndClaw;

public class TooManyMonsters : FearCardBase, IFearCard {

	public const string Name = "Too Many Monsters";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer/Town from a land with Beast." )]
	public Task Level1( GameCtx ctx ) 
		=> Cmd.RemoveExplorersOrTowns(1)
			.In().SpiritPickedLand().Which( Has.Beast ).ByPickingToken(Human.Explorer_Town)
			.ForEachSpirit()
			.ActAsync(ctx);

	[FearLevel( 2, "Each player removes 1 Explorer and 1 Town from a land with Beast or 1 Explorer from a land adjacent to Beast." )]
	public Task Level2( GameCtx ctx )
		=> new SpaceAction("Remove 1 explorer (+1 Town if land has beasts)", Remove_Level2)
			.In().SpiritPickedLand().Which( Has.BeastOrIsAdjacentToBeast )
			.ForEachSpirit()
			.ActAsync(ctx);

	[FearLevel( 3, "Each player removes 2 Explorer and 2 Town from a land with Beast or 1 Explorer/Town from a land adjacent to Beast." )]
	public Task Level3( GameCtx ctx )
		=> new SpaceAction( "Removes 2 Explorer and 2 Town from a land with beast or 1 Explorer/Town from a land adjacent to Beast.", Remove_Level3 )
			.In().SpiritPickedLand().Which( Has.BeastOrIsAdjacentToBeast )
			.ForEachSpirit()
			.ActAsync( ctx );

	Task Remove_Level2( TargetSpaceCtx ctx ) {
		var remover = ctx.SourceSelector.AddGroup( 1, Human.Explorer );
		if(ctx.Beasts.Any) remover.AddGroup( 1, Human.Town );
		return remover.RemoveN( ctx.Self );
	}

	Task Remove_Level3( TargetSpaceCtx ctx ) {
		SourceSelector ss = ctx.Beasts.Any
			? ctx.SourceSelector.AddGroup( 2, Human.Explorer ).AddGroup( 2, Human.Town )
			: ctx.SourceSelector.AddGroup( 1, Human.Explorer_Town );
		return ss.RemoveN(ctx.Self);
	}

}