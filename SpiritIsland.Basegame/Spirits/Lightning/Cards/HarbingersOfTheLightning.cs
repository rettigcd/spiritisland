namespace SpiritIsland.Basegame;

public class HarbingersOfTheLightning {

	public const string Name = "Harbingers of the Lightning";

	[SpiritCard(HarbingersOfTheLightning.Name,0,Element.Fire,Element.Air), Slow,FromPresence(1,Target.Dahan)]
	[Instructions( "Push up to 2 Dahan. 1 Fear if you pushed any Dahan into a land with Town / City" ), Artist( Artists.RockyHammer )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){

		// Push up to 2 dahan.
		await ctx.SourceSelector
			.AddGroup(2,Human.Dahan)
			.ConfigDestination( AddFearIfPushedTo(1,Human.Town_City) )
			.PushUpToN(ctx.Self );
	}

	static Action<DestinationSelector> AddFearIfPushedTo( int fear, ITokenClass[] classes ) {
		return (d) => {
			bool addedFear = false;
			d.Track( to => {
				if(!addedFear && to.HasAny( classes )) {
					GameState.Current.Fear.AddDirect( new FearArgs( fear ) { space = to.Space } );
					addedFear = true;
				}
			} );
		};
	}


}

