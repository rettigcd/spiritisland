namespace SpiritIsland.BranchAndClaw;

public class ConfoundingMists {

	[MinorCard( "Confounding Mists", 1, Element.Air, Element.Water )]
	[Fast]
	[FromPresence( 1 )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceAction("Defend 4", ctx => ctx.Defend(4) ),
			new SpaceAction("Invaders added to target are immediately pushed", PushFutureInvadersFromLands )
		);
	}

	static void PushFutureInvadersFromLands( TargetSpaceCtx ctx ) {

		// each invader added to target land this turn may be immediatley pushed to any adjacent land
		ctx.GameState.Tokens.TokenAdded.ForRound.Add( PushAddedInvader );

		async Task PushAddedInvader( ITokenAddedArgs args ) {
			if(args.Space == ctx.Space 	&& (args.Reason == AddReason.Explore || args.Reason == AddReason.Build))  // ??? is there any other way to add invaders?

				await ctx.Pusher.PushToken( args.Token );
		}
	}

}