namespace SpiritIsland.BranchAndClaw;

public class DahanAttack : FearCardBase, IFearCard {

	public const string Name = "Dahan Attack";
	public string Text => Name;

	[FearLevel( 1, "Each player removes 1 Explorer from a land with Dahan." )]
	public override Task Level1( GameState ctx ) 
		=> Cmd.RemoveExplorers( 1 )
			.In().SpiritPickedLand().Which( Has.Dahan ).ByPickingToken( Human.Explorer )
			.ForEachSpirit()
			.ActAsync(ctx);

	[FearLevel( 2, "Each player chooses a different land with Dahan. 1 Damage per Dahan there." )]
	public override Task Level2( GameState ctx )
		=> Cmd.OneDamagePerDahan
			.In().SpiritPickedLand().AllDifferent()
			.ForEachSpirit().ActAsync(ctx);

	[FearLevel( 3, "Each player chooses a different land with Town/City. Gather 1 Dahan into that land. Then 2 Damage per Dahan there." )]
	public override Task Level3( GameState ctx )
		=> GatherThen2Damage
			.In().SpiritPickedLand().AllDifferent().Which( Has.TownOrCity )
			.ForEachSpirit()
			.ActAsync( ctx );

	static SpaceAction GatherThen2Damage => new SpaceAction( "Gather 1 dahan. Then 2 Damage per Dahan there.", async ctx => { 
		// Gather 1 dahan into that land.
		await ctx.GatherDahan( 1 );
		// Then 2 damage per dahan there
		await ctx.DamageInvaders( 2*ctx.Dahan.CountAll );
	});

}
