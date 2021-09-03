using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	public class CallToTrade {

		[MinorCard( "Call to Trade", 1, Speed.Fast, Element.Air, Element.Water, Element.Earth, Element.Plant )]
		[FromPresence( 1, Target.Invaders )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.GatherUpToNDahan(1);

			if(ctx.GameState.Fear.TerrorLevel <= 2) {
				await ctx.GatherUpToNTokens(1,Invader.Town);
				// And the first ravage in target land becomes a build there instead.
				// !!!!!!!!!!
			}


		}

	}

}
