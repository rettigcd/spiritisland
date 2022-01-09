using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class PortentsOfDisaster {

		[MinorCard( "Portents of Disaster", 0, Element.Sun, Element.Moon, Element.Air ), Fast, FromSacredSite( 1, Target.Invaders )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			// 2 fear
			ctx.AddFear(2);

			// The next time an invader is destroyed in target land this turn, 1 fear
			bool addFear = true;
			Task Add1MoreFearForFirstDestroyedInvader( ITokenRemovedArgs args ) {
				if( addFear 
					&& args.Reason.IsDestroy()
					&& args.Space == ctx.Space 
					&& args.Token.Class.IsOneOf(Invader.Town,Invader.City,Invader.Explorer) 
				){ // !! create an override .IsInvader()
					ctx.AddFear(1);
					addFear = false;
				}
				return Task.CompletedTask;
			}
			ctx.GameState.Tokens.TokenRemoved.ForRound.Add( Add1MoreFearForFirstDestroyedInvader );

			return Task.CompletedTask;
		}

	}

}
