using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CallToFerocity {

		[MinorCard( "Call to Ferocity", 0, Speed.Slow, Element.Sun, Element.Fire, Element.Earth )]
		[FromPresence( 1, Target.Invaders )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			bool shouldGather = !ctx.HasDahan  // no dahan, can't do 2nd option
				|| await ctx.Self.UserSelectsFirstText("Select power", "Gather up to 3 dhan", "1 fear and push 1 explorer and 1 town" ); // use selects gather

			if( shouldGather) {
				await ctx.GatherUpToNDahan(3);
			} else {
				ctx.AddFear(1);
				await ctx.PushNTokens(ctx.Target,1,Invader.Explorer);
				await ctx.PushNTokens( ctx.Target, 1, Invader.Town );
			}

		}

	}

}
