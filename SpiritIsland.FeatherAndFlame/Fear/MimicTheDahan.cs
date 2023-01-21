
namespace SpiritIsland.FeatherAndFlame;

public class MimicTheDahan : FearCardBase, IFearCard {

	public const string Name = "Mimic the Dahan";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer / Town from a land with 2 or more Dahan." )]
	public Task Level1( GameCtx ctx ) => Cmd.RemoveExplorersOrTowns(1)
		.In().SpiritPickedLand().Which( Has.TwoOrMoreDahan )
		.ByPickingToken(Invader.Explorer_Town)
		.ForEachSpirit()
		.Execute( ctx );


	[FearLevel( 2, "Each player replaces 1 Explorer / Town with 1 Dahan in a land with 2 or more Dahan." )]
	public Task Level2( GameCtx ctx ) => ReplaceExplorerOrTownWith1Dahan
		.In().SpiritPickedLand().Which( Has.TwoOrMoreDahan )
		.ByPickingToken( Invader.Explorer_Town )
		.ForEachSpirit()
		.Execute( ctx );

	[FearLevel( 3, "Each player replaces 1 Explorer / Town with 1 Dahan in a land with Dahan, or adjacent to 3 or more Dahan." )]
	public Task Level3( GameCtx ctx ) => ReplaceExplorerOrTownWith1Dahan
		.In().SpiritPickedLand().Which( Has.DahanOrIsAdjacentTo3 )
		.ByPickingToken( Invader.Explorer_Town )
		.ForEachSpirit()
		.Execute( ctx );

	static SpaceAction ReplaceExplorerOrTownWith1Dahan => new SpaceAction("Replace 1 Explorer/Town with 1 Dahan", async ctx => {
		await ctx.Invaders.RemoveLeastDesirable( Invader.Explorer_Town );
		ctx.Tokens.AdjustDefault(TokenType.Dahan, 1);
	} ).OnlyExecuteIf( x=>x.Tokens.HasAny(Invader.Explorer_Town));
}


