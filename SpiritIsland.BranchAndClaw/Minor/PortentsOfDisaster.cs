namespace SpiritIsland.BranchAndClaw;

public class PortentsOfDisaster {

	const string Name = "Portents of Disaster";

	[MinorCard( Name, 0, Element.Sun, Element.Moon, Element.Air ), Fast, FromSacredSite( 1, Target.Invaders )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		// 2 fear
		ctx.AddFear(2);

		// The next time an invader is destroyed in target land this turn, 1 fear
		bool addFear = true;
		Task Add1MoreFearForFirstDestroyedInvader( ITokenRemovedArgs args ) {
			if( addFear 
				&& args.Reason.IsDestroy()
				&& args.Token.Class.Category == TokenCategory.Invader
			){ // !! create an override .IsInvader()
				ctx.AddFear(1);
				addFear = false;
			}
			return Task.CompletedTask;
		}
		ctx.Tokens.Adjust( new TokenRemovedHandler( Name, Add1MoreFearForFirstDestroyedInvader ), 1 );
//		ctx.GameState.Tokens.TokenRemoved.ForRound.Add( Add1MoreFearForFirstDestroyedInvader );

		return Task.CompletedTask;
	}

}