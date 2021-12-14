using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ConfoundingMists {

		[MinorCard( "Confounding Mists", 1, Element.Air, Element.Water )]
		[Fast]
		[FromPresence( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectActionOption(
				new ActionOption("Defend 4", ctx => ctx.Defend(4) ),
				new ActionOption("Invaders added to target are immediately pushed", PushFutureInvadersFromLands )
			);
		}

		static void PushFutureInvadersFromLands( TargetSpaceCtx ctx ) {

			// each invader added to target land this turn may be immediatley pushed to any adjacent land
			ctx.GameState.Tokens.TokenAdded.ForRound.Add( PushAddedInvader );

			async Task PushAddedInvader( GameState gs, TokenAddedArgs args ) {
				if(args.Space == ctx.Space && args.Token.Generic.IsOneOf( Invader.Explorer, Invader.Town, Invader.City )) {
					// create a new Ctx that targets the new GameState
					var newCtx = new TargetSpaceCtx( ctx.Self, gs, ctx.Space, ctx.Cause );
					await newCtx.Pusher.PushToken( args.Token );
				}
			}
		}

	}

}
