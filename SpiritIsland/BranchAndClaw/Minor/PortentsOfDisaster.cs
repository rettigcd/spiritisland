using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	public class PortentsOfDisaster {

		[MinorCard( "Portents of Disaster", 0, Speed.Fast, Element.Sun, Element.Moon, Element.Air )]
		[FromSacredSite( 0, Target.Inland )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {

			ctx.AddFear(2);

			bool addFear = true;
			Task Add1MoreFearForFirstDestoryedInvader(GameState gs,TokenDestroyedArgs args ) {
				if(addFear && args.space == ctx.Target && IsInvader(args.Token)){
					ctx.AddFear(1);
					addFear = false;
				}
				return Task.CompletedTask;
			}
			ctx.GameState.Tokens.TokenDestroyed.Handlers.Add( Add1MoreFearForFirstDestoryedInvader );

			return Task.CompletedTask;
		}

		static bool IsInvader(TokenGroup group ) {
			return group == Invader.Town
				|| group == Invader.City
				|| group == Invader.Explorer;
		}
	}

}
