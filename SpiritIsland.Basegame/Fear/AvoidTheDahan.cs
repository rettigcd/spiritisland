namespace SpiritIsland.Basegame;

public class AvoidTheDahan : IFearOptions {
	public const string Name = "Avoid the Dahan";
	string IFearOptions.Name => Name;

	[FearLevel(1, "Invaders do not Explore into lands with at least 2 Dahan." )]
	public Task Level1( GameCtx ctx ) {

		ctx.GameState.PreExplore.ForRound.Add( ( args ) => {
			for(int i = 0; i < args.SpacesMatchingCards.Count; ++i) {
				var space = args.SpacesMatchingCards[i];
				if( 2 <= space.Dahan.Count)
					args.Skip(space);
			}
		} );

		return Task.CompletedTask;
	}

	[FearLevel( 2, "Invaders do not Build in lands where Dahan outnumber Town / City." )]
	public Task Level2( GameCtx ctx ) {
		ctx.GameState.PreBuilding.ForRound.Add( ( args ) => {
			foreach(var space in args.SpacesWithBuildTokens) {
				var tokens = space;
				if(tokens.SumAny(Invader.City,Invader.Town) < tokens.Dahan.Count)
					args.GameState.AdjustTempToken( space.Space, BuildStopper.StopAll( Name ) );
			}
		} );

		return Task.CompletedTask;
	}

	[FearLevel( 3, "Invaders do not Build in lands with Dahan." )]
	public Task Level3( GameCtx ctx ) {
		ctx.GameState.PreBuilding.ForRound.Add( ( args ) => {
			foreach(var space in args.SpacesWithBuildTokens) {
				if(0 < space.Dahan.Count)
					args.GameState.AdjustTempToken( space.Space, BuildStopper.StopAll( Name ) );
			}
		} );
		return Task.CompletedTask;
	}

}