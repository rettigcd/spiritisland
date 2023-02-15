namespace SpiritIsland.BranchAndClaw;

public class ConfoundingMists {

	public const string Name = "Confounding Mists";

	[MinorCard( Name, 1, Element.Air, Element.Water )]
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
		ctx.Tokens.Adjust( new TokenAddedHandler(PushAddedInvader), 1);

		async Task PushAddedInvader( ITokenAddedArgs args ) {
			if( args.Reason.IsOneOf( AddReason.Explore, AddReason.Build) ) // ??? is there any other way to add invaders?
				await ctx.Pusher.PushToken( (IToken)args.Added );
		}
	}

}