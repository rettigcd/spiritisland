namespace SpiritIsland.BranchAndClaw;

public class PortentsOfDisaster {

	const string Name = "Portents of Disaster";

	[MinorCard( Name, 0, Element.Sun, Element.Moon, Element.Air ), Fast, FromSacredSite( 1, Filter.Invaders )]
	[Instructions( "2 Fear. The next time an Invader is destroyed in target land this turn, 1 Fear." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		// 2 fear
		await ctx.AddFear(2);

		// The next time an invader is destroyed in target land this turn, 1 fear
		ctx.Space.Adjust( new Add1FearForFirstDestroyedInvader(ctx), 1 );

	}

	public class Add1FearForFirstDestroyedInvader( TargetSpaceCtx ctx ) : BaseModEntity, IEndWhenTimePasses, IHandleTokenRemoved {
		bool _addFear = true;
		public async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
			if( _addFear
				&& args.Reason.IsDestroy()
				&& args.Removed.HasTag(TokenCategory.Invader)
			){ // !! create an override .IsInvader()
				await ctx.AddFear(1);
				_addFear = false;
			}
		}

	}

}