namespace SpiritIsland.BranchAndClaw;

public class PortentsOfDisaster {

	const string Name = "Portents of Disaster";

	[MinorCard( Name, 0, Element.Sun, Element.Moon, Element.Air ), Fast, FromSacredSite( 1, Filter.Invaders )]
	[Instructions( "2 Fear. The next time an Invader is destroyed in target land this turn, 1 Fear." ), Artist( Artists.NolanNasser )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		// 2 fear
		ctx.AddFear(2);

		// The next time an invader is destroyed in target land this turn, 1 fear
		bool addFear = true;
		Task Add1MoreFearForFirstDestroyedInvader( Space from, ITokenRemovedArgs args ) {
			if( addFear 
				&& args.Reason.IsDestroy()
				&& args.Removed.HasTag(TokenCategory.Invader)
			){ // !! create an override .IsInvader()
				ctx.AddFear(1);
				addFear = false;
			}
			return Task.CompletedTask;
		}
		ctx.Space.Adjust( new TokenRemovedHandlerAsync( Add1MoreFearForFirstDestroyedInvader ), 1 );

		return Task.CompletedTask;
	}

}