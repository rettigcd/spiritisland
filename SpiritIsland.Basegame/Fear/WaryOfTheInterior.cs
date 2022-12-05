namespace SpiritIsland.Basegame;

public class WaryOfTheInterior : IFearOptions {

	public const string Name = "Wary of the Interior";
	string IFearOptions.Name => Name;


	[FearLevel( 1, "Each player removes 1 Explorer from an Inland land." )]
	public Task Level1( GameCtx ctx ) => EachSpiritRemoves1Invader( ctx, x => x.IsInland, Invader.Explorer );

	[FearLevel( 2, "Each player removes 1 Explorer / Town from an Inland land." )]
	public Task Level2( GameCtx ctx ) => EachSpiritRemoves1Invader( ctx, x=>x.IsInland, Invader.Explorer, Invader.Town );

	[FearLevel( 3, "Each player removes 1 Explorer / Town from any land." )]
	public Task Level3( GameCtx ctx ) => EachSpiritRemoves1Invader( ctx, s=>true, Invader.Explorer, Invader.Town );

	static async Task EachSpiritRemoves1Invader( GameCtx ctx, Func<TargetSpaceCtx,bool> spaceCondition, params TokenClass[] removable ) {
		foreach(var spiritCtx in ctx.Spirits) {
			var options = spiritCtx.GameState.AllActiveSpaces
				.Select( x=>spiritCtx.Target(x.Space) )
				.Where( x=>x.IsInPlay )
				.Where( spaceCondition )
				.Where( ctx => ctx.Tokens.HasAny( removable ) )
				.Select(x=>x.Space)
				.ToArray();
			if(options.Length == 0) return;

			await spiritCtx.RemoveTokenFromOneSpace(options,1,removable);
		}
	}

}