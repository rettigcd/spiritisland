namespace SpiritIsland.Basegame;

public class WaryOfTheInterior : IFearOptions {

	public const string Name = "Wary of the Interior";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Each player removes 1 Explorer from an Inland land." )]
	public Task Level1( FearCtx ctx ) => EachSpiritRemoves1Invader( ctx, IsInland, Invader.Explorer );

	[FearLevel( 2, "Each player removes 1 Explorer / Town from an Inland land." )]
	public Task Level2( FearCtx ctx ) => EachSpiritRemoves1Invader( ctx, IsInland, Invader.Explorer, Invader.Town );

	[FearLevel( 3, "Each player removes 1 Explorer / Town from any land." )]
	public Task Level3( FearCtx ctx ) => EachSpiritRemoves1Invader( ctx, s=>true, Invader.Explorer, Invader.Town );

	static bool IsInland(Space space) => !space.IsCoastal;

	static async Task EachSpiritRemoves1Invader( FearCtx ctx, Func<Space,bool> spaceCondition, params TokenClass[] removable ) {
		foreach(var spiritCtx in ctx.Spirits) {
			var options = spiritCtx.AllSpaces
				.Where( spaceCondition )
				.Where( s => ctx.GameState.Tokens[ s ].HasAny( removable ) )
				.ToArray();
			if(options.Length == 0) return;

			await spiritCtx.RemoveTokenFromOneSpace(options,1,removable);
		}
	}

}