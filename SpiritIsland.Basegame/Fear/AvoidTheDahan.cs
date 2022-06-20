﻿namespace SpiritIsland.Basegame;

public class AvoidTheDahan : IFearOptions {
	public const string Name = "Avoid the Dahan";
	string IFearOptions.Name => Name;

	[FearLevel(1, "Invaders do not Explore into lands with at least 2 Dahan." )]
	public Task Level1( FearCtx ctx ) {

		ctx.GameState.PreExplore.ForRound.Add( ( args ) => {
			for(int i = 0; i < args.SpacesMatchingCards.Count; ++i) {
				var space = args.SpacesMatchingCards[i];
				if( 2<=args.GameState.DahanOn(space).Count)
					args.Skip(space);
			}
		} );

		return Task.CompletedTask;
	}

	[FearLevel( 2, "Invaders do not Build in lands where Dahan outnumber Town / City." )]
	public Task Level2( FearCtx ctx ) {
		ctx.GameState.PreBuilding.ForRound.Add( ( args ) => {
			foreach(var space in args.SpacesWithBuildTokens) {
				var tokens = args.GameState.Tokens[space];
				if(tokens.SumAny(Invader.City,Invader.Town) < tokens.Dahan.Count)
					args.GameState.AdjustTempToken( space, BuildStopper.StopAll( Name ) );
			}
		} );

		return Task.CompletedTask;
	}

	[FearLevel( 3, "Invaders do not Build in lands with Dahan." )]
	public Task Level3( FearCtx ctx ) {
		ctx.GameState.PreBuilding.ForRound.Add( ( args ) => {
			foreach(var space in args.SpacesWithBuildTokens) {
				if(0 < args.GameState.Tokens[space].Dahan.Count)
					args.GameState.AdjustTempToken( space, BuildStopper.StopAll( Name ) );
			}
		} );
		return Task.CompletedTask;
	}

}