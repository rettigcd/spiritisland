namespace SpiritIsland.Basegame;

public class DahanRaid : FearCardBase, IFearCard {

	public const string Name = "Dahan Raid";
	public string Text => Name;

	[FearLevel(1, "Each player chooses a different land with Dahan. 1 Damage there.")]
	public Task Level1( GameCtx ctx )
		=> new SpaceAction("1 Damage", ctx=>ctx.DamageInvaders(1,Invader.Any) )
			.In().SpiritPickedLand().AllDifferent().Which( Has.Dahan )
			.ByPickingToken( Invader.Any )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 2, "Each player chooses a different land with Dahan. 1 Damage per Dahan there." )]
	public Task Level2( GameCtx ctx )
		=> new SpaceAction( "1 Damage per dahan", ctx => ctx.DamageInvaders( ctx.Dahan.CountAll, Invader.Any ) )
			.In().SpiritPickedLand().AllDifferent().Which( Has.Dahan )
			.ByPickingToken( Invader.Any )
			.ForEachSpirit()
			.Execute( ctx );

	[FearLevel( 3, "Each player chooses a different land with Dahan. 2 Damage per Dahan there." )]
	public Task Level3( GameCtx ctx )
		=> new SpaceAction( "2 Damage per dahan", ctx => ctx.DamageInvaders( ctx.Dahan.CountAll*2, Invader.Any ) )
			.In().SpiritPickedLand().AllDifferent().Which( Has.Dahan )
			.ByPickingToken( Invader.Any )
			.ForEachSpirit()
			.Execute( ctx );

}