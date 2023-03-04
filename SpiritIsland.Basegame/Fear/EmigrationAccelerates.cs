namespace SpiritIsland.Basegame;

public class EmigrationAccelerates : FearCardBase, IFearCard {

	public const string Name = "Emigration Accelerates";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer from a Coastal land." )]
	public Task Level1( GameCtx ctx ) 
		=> Cmd.RemoveExplorers( 1 )
			.From().SpiritPickedLand().Which( Is.Coastal )
			.ByPickingToken(Human.Explorer)
			.ForEachSpirit().Execute(ctx);

	[FearLevel( 2, "Each player removes 1 Explorer/Town from a Coastal land." )]
	public Task Level2( GameCtx ctx ) 
		=> Cmd.RemoveExplorersOrTowns( 1 )
			.From().SpiritPickedLand().Which( Is.Coastal )
			.ByPickingToken( Human.Explorer_Town )
			.ForEachSpirit()
		.Execute( ctx );

	[FearLevel( 3, "Each player removes 1 Explorer/Town from any land." )]
	public Task Level3( GameCtx ctx )
		=> Cmd.RemoveExplorersOrTowns( 1 )
			.In().SpiritPickedLand()
			.ByPickingToken( Human.Explorer_Town )
			.ForEachSpirit()
			.Execute( ctx );

}