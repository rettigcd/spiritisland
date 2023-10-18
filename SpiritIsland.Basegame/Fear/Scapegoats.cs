namespace SpiritIsland.Basegame;

public class Scapegoats : FearCardBase, IFearCard {

	public const string Name = "Scapegoats";
	public string Text => Name;

	[FearLevel( 1, "Each Town destroys 1 Explorer in its land." )]
	public Task Level1( GameCtx ctx )
		=> EachTownDestorys1Explorer
			.In().EachActiveLand()
			.Execute( ctx );

	[FearLevel( 2, "Each Town destroys 1 Explorer in its land. Each City destroys 2 Explorer in its land." )]
	public Task Level2( GameCtx ctx )
		=> EachTownDestroys1AndCityDestroys2
			.In().EachActiveLand()
			.Execute( ctx );

	[FearLevel( 3, "Destroy all Explorer in lands with Town/City. Each City destroys 1 Town in its land." )]
	public Task Level3( GameCtx ctx )
		=> DestroyAllExplorersAnd1TowPerCity
			.In().EachActiveLand()
			.Execute( ctx );

	static SpaceCmd EachTownDestorys1Explorer => new SpaceCmd( "each town destorys 1 explorer", ctx => ctx.Invaders.DestroyNOfClass( ctx.Tokens.Sum( Human.Town ), Human.Explorer ) );

	static SpaceCmd EachTownDestroys1AndCityDestroys2 =>
		new SpaceCmd( "each town destroys 1 explorer and each City destroys 2", async ctx => {
			int numToDestroy = ctx.Tokens.Sum( Human.Town ) + ctx.Tokens.Sum( Human.City ) * 2;
			await ctx.Invaders.DestroyNOfClass( numToDestroy, Human.Explorer );
		});

	static SpaceCmd DestroyAllExplorersAnd1TowPerCity => new SpaceCmd(
		"Destroy all explorers and 1 town per city", async ctx => {
			await ctx.Invaders.DestroyAll( Human.Explorer );
			await ctx.Invaders.DestroyNOfClass( ctx.Tokens.Sum( Human.City ), Human.Town );
	});
}