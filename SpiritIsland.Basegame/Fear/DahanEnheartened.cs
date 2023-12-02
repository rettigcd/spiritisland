namespace SpiritIsland.Basegame;

public class DahanEnheartened : FearCardBase, IFearCard {

	public const string Name = "Dahan Enheartened";
	public string Text => Name;

	[FearLevel( 1, "Each player may Push 1 Dahan from a land with Invaders or Gather 1 Dahan into a land with Invaders." )]
	public async Task Level1( GameCtx ctx ) {
		await PushOrGather1
			.In().SpiritPickedLand().Which( Has.Invaders )
			.ForEachSpirit()
			.ActAsync( ctx );
	}

	[FearLevel( 2, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage if Dahan are present." )]
	public Task Level2( GameCtx ctx ) 
		=> Gather2DahanThen1DamageIfDahan
			.In().SpiritPickedLand().AllDifferent()
			.ForEachSpirit()
			.ActAsync( ctx );

	[FearLevel( 3, "Each player chooses a different land. In chosen lands: Gather up to 2 Dahan, then 1 Damage per Dahan present." )]
	public Task Level3( GameCtx ctx )
		=> Gather2DahanThenDamagePerDahan.In().SpiritPickedLand()
			.AllDifferent()
			.ForEachSpirit()
			.ActAsync( ctx );

	static BaseCmd<TargetSpaceCtx> PushOrGather1 => Cmd.Pick1( Cmd.PushNDahan( 1 ), Cmd.GatherUpToNDahan( 1 ) );

	static SpaceAction Gather2DahanThen1DamageIfDahan => new SpaceAction( "Gather up to 2 dahan then 1 damage if dahan present.", async ctx => {
		await ctx.GatherUpToNDahan( 2 );
		if( ctx.Dahan.Any )
			await ctx.DamageInvaders( 1 );
	} );

	static SpaceAction Gather2DahanThenDamagePerDahan => new SpaceAction( "Gather up to 2 dahan then 1 damage/dahan.", async ctx => {
		await ctx.GatherUpToNDahan( 2 );
		await ctx.DamageInvaders( ctx.Dahan.CountAll );
	} );

}