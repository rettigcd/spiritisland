namespace SpiritIsland.Basegame;

public class EmigrationAccelerates : FearCardBase, IFearCard {

	public const string Name = "Emigration Accelerates";
	string IOption.Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer from a Coastal land." )]
	public override Task Level1( GameState ctx ) 
		=> Cmd.RemoveExplorers( 1 )
			.From().SpiritPickedLand().Which( Is.Coastal )
			.ByPickingToken(Human.Explorer)
			.ForEachSpirit().ActAsync(ctx);

	[FearLevel( 2, "Each player removes 1 Explorer/Town from a Coastal land." )]
	public override Task Level2( GameState ctx ) 
		=> Cmd.RemoveExplorersOrTowns( 1 )
			.From().SpiritPickedLand().Which( Is.Coastal )
			.ByPickingToken( Human.Explorer_Town )
			.ForEachSpirit()
		.ActAsync( ctx );

	[FearLevel( 3, "Each player removes 1 Explorer/Town from any land." )]
	public override Task Level3( GameState ctx )
		=> Cmd.RemoveExplorersOrTowns( 1 )
			.In().SpiritPickedLand()
			.ByPickingToken( Human.Explorer_Town )
			.ForEachSpirit()
			.ActAsync( ctx );

}