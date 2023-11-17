namespace SpiritIsland.Basegame;

public class HarbingersOfTheLightning {

	public const string Name = "Harbingers of the Lightning";

	[SpiritCard(HarbingersOfTheLightning.Name,0,Element.Fire,Element.Air), Slow,FromPresence(1,Target.Dahan)]
	[Instructions( "Push up to 2 Dahan. 1 Fear if you pushed any Dahan into a land with Town / City" ), Artist( Artists.RockyHammer )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){

		// Push up to 2 dahan.
		await ctx.Pusher
			.AddGroup(2,Human.Dahan)
			.Config( mover => AddFearIfPushedTo(mover,1, Human.Town_City ) )
			.DoUpToN();
	}

	static TokenMover AddFearIfPushedTo( TokenMover mover, int fear, ITokenClass[] classes ) {
		bool addedFear = false;
		return mover
			.Track( moved => {
				if(!addedFear && moved.To.HasAny( classes )) {
					GameState.Current.Fear.AddDirect( new FearArgs( fear ) { space = moved.To.Space } );
					addedFear = true;
				}
			} );
	}


}

