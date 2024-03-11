namespace SpiritIsland.FeatherAndFlame;

public class MimicTheDahan : FearCardBase, IFearCard {

	public const string Name = "Mimic the Dahan";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer/Town from a land with 2 or more Dahan." )]
	public Task Level1( GameState ctx ) => Cmd.RemoveExplorersOrTowns(1)
		.In().SpiritPickedLand().Which( Has.TwoOrMoreDahan )
//		.ByPickingToken(Human.Explorer_Town)// Generating an Exceptoin
		.ForEachSpirit()
		.ActAsync( ctx );


	[FearLevel( 2, "Each player replaces 1 Explorer/Town with 1 Dahan in a land with 2 or more Dahan." )]
	public Task Level2( GameState ctx ) => ReplaceExplorerOrTownWith1Dahan
		.In().SpiritPickedLand().Which( Has.TwoOrMoreDahan )
//		.ByPickingToken( Human.Explorer_Town ) // Generating an Exceptoin
		.ForEachSpirit()
		.ActAsync( ctx );

	[FearLevel( 3, "Each player replaces 1 Explorer/Town with 1 Dahan in a land with Dahan, or adjacent to 3 or more Dahan." )]
	public Task Level3( GameState ctx ) => ReplaceExplorerOrTownWith1Dahan
		.In().SpiritPickedLand().Which( Has.DahanOrIsAdjacentTo3 )
//		.ByPickingToken( Human.Explorer_Town )// Generating an Exceptoin
		.ForEachSpirit()
		.ActAsync( ctx );

	static SpaceAction ReplaceExplorerOrTownWith1Dahan => new SpaceAction("Replace 1 Explorer/Town with 1 Dahan", async ctx => {
		await ReplaceInvader.WithDahanAsync(ctx.Space,Human.Explorer_Town);
	} ).OnlyExecuteIf( x=>x.Space.HasAny(Human.Explorer_Town));
}


