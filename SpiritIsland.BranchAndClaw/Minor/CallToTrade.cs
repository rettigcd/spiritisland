using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CallToTrade {

		[MinorCard( "Call to Trade", 1, Speed.Fast, Element.Air, Element.Water, Element.Earth, Element.Plant )]
		[FromPresence( 1, Target.Invaders )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.GatherUpToNDahan(1);

			if(ctx.GameState.Fear.TerrorLevel <= 2) {
				await ctx.GatherUpToNTokens(1,Invader.Town);

				// And the first ravage in target land becomes a build there instead.
				bool hasRavage = false;
				Task RemoveRavage(GameState gs,Space[] spaces ) {
					hasRavage = spaces.Contains(ctx.Target);
					ctx.GameState.ModifyRavage(ctx.Target,cfg=>cfg.ShouldRavage = false);
					return Task.CompletedTask;
				}

				Task AddBuild(GameState gs,BuildingEventArgs args ) {
					if( hasRavage )
						args.Spaces[ctx.Target]++;
					return Task.CompletedTask;
				}

				ctx.GameState.PreRavaging.ForRound.Add( RemoveRavage );
				ctx.GameState.PreBuilding.ForRound.Add( AddBuild );

			}


		}

	}

}
