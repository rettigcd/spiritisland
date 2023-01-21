namespace SpiritIsland.BranchAndClaw;

public class DahanAttack : FearCardBase, IFearCard {

	public const string Name = "Dahan Attack";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 eplorer from a land with dahan" )]
	public Task Level1( GameCtx ctx ) 
		=> Cmd.RemoveExplorers( 1 )
			.In().SpiritPickedLand().Which( Has.Dahan )
			.ByPickingToken( Invader.Explorer )
			.ForEachSpirit()
			.Execute(ctx);

	[FearLevel( 2, "Each player chooses a different land with dahan.  1 damage per dahan there" )]
	public Task Level2( GameCtx ctx )
		=> Cmd.OneDamagePerDahan
			.In().SpiritPickedLand().AllDifferent()
			.ForEachSpirit().Execute(ctx);

	[FearLevel( 3, "Each player chooses a different land with towns/cities.  Gather 1 dahan into that land.  Then 2 damage per dahan there" )]
	public Task Level3( GameCtx ctx )
		=> GatherThen2Damage
			.In().SpiritPickedLand().AllDifferent().Which( Has.TownOrCity )
			.ForEachSpirit()
			.Execute( ctx );

	static SpaceAction GatherThen2Damage => new SpaceAction( "Gather 1 dahan. Then 2 dmage per dahan there.", async ctx => { 
		// Gather 1 dahan into that land.
		await ctx.GatherDahan( 1 );
		// Then 2 damage per dahan there
		await ctx.DamageInvaders( 2*ctx.Dahan.CountAll );
	});

}
