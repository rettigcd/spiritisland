using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CallToTrade {

		[MinorCard( "Call to Trade", 1, Speed.Fast, Element.Air, Element.Water, Element.Earth, Element.Plant )]
		[FromPresence( 1, Target.Invaders )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.GatherUpToNDahan(1);

			if( ctx.GameState.Fear.TerrorLevel <= 2 ) {
				await ctx.Gather( 1,Invader.Town );

				// And the first ravage in target land becomes a build there instead.
				if(ctx.GameState.ScheduledRavageSpaces.Contains( ctx.Space )) {
					ctx.GameState.ScheduledRavageSpaces.Remove(ctx.Space);
					ctx.GameState.ScheduledBuildSpaces[ctx.Space]++;
				}
			}

		}

	}

}
