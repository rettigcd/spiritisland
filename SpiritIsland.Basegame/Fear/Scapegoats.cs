namespace SpiritIsland.Basegame;

public class Scapegoats : FearCardBase, IFearCard {

	public const string Name = "Scapegoats";
	string IOption.Text => Name;

	[FearLevel( 1, "Each Town destroys 1 Explorer in its land." )]
	public Task Level1( GameState ctx )
		=> EachTownDestorys1Explorer
			.In().EachActiveLand()
			.ActAsync( ctx );

	[FearLevel( 2, "Each Town destroys 1 Explorer in its land. Each City destroys 2 Explorer in its land." )]
	public Task Level2( GameState ctx )
		=> EachTownDestroys1AndCityDestroys2
			.In().EachActiveLand()
			.ActAsync( ctx );

	[FearLevel( 3, "Destroy all Explorer in lands with Town/City. Each City destroys 1 Town in its land." )]
	public Task Level3( GameState ctx )
		=> DestroyAllExplorersAnd1TowPerCity
			.In().EachActiveLand()
			.ActAsync( ctx );

	static SpaceAction EachTownDestorys1Explorer => new SpaceAction( "each town destorys 1 explorer", ctx => ctx.Invaders.DestroyNOfClass( ctx.Space.Sum( Human.Town ), Human.Explorer ) );

	static SpaceAction EachTownDestroys1AndCityDestroys2 =>
		new SpaceAction( "each town destroys 1 explorer and each City destroys 2", async ctx => {
			int numToDestroy = ctx.Space.Sum( Human.Town ) + ctx.Space.Sum( Human.City ) * 2;
			await ctx.Invaders.DestroyNOfClass( numToDestroy, Human.Explorer );
		});

	static SpaceAction DestroyAllExplorersAnd1TowPerCity => new SpaceAction(
		"Destroy all explorers and 1 town per city", async ctx => {
			await ctx.Invaders.DestroyAll( Human.Explorer );
			await ctx.Invaders.DestroyNOfClass( ctx.Space.Sum( Human.City ), Human.Town );
	});
}