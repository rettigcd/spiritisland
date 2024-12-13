namespace SpiritIsland.Basegame;

public class HarbingersOfTheLightning {

	public const string Name = "Harbingers of the Lightning";

	[SpiritCard(Name,0,Element.Fire,Element.Air), Slow,FromPresence(1,Filter.Dahan)]
	[Instructions( "Push up to 2 Dahan. 1 Fear if you pushed any Dahan into a land with Town / City" ), Artist( Artists.RockyHammer )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){

		List<Space> fearSpaces = [];

		// Push up to 2 dahan.
		await ctx.SourceSelector
			.AddGroup(2,Human.Dahan)
			.ConfigDestination( AddFearIfPushedTo(Human.Town_City,fearSpaces) )
			.PushUpToN(ctx.Self );

		foreach(var fearSpace in  fearSpaces)
			await fearSpace.AddFear(1);
	}

	static Action<DestinationSelector> AddFearIfPushedTo( ITokenClass[] classes, List<Space> fearSpaces ) {
		return (d) => {
			bool addedFear = false;
			d.Track( to => {
				if(!addedFear && to.HasAny( classes )) {
					fearSpaces.Add( to );
					addedFear = true;
				}
			} );
		};
	}


}

