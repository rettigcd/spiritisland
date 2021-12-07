using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CallToTrade {

		public const string Name = "Call to Trade";

		[MinorCard( CallToTrade.Name, 1, Element.Air, Element.Water, Element.Earth, Element.Plant )]
		[Fast]
		[FromPresence( 1, Target.Invaders )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// You may Gather 1 dahan
			await ctx.GatherUpToNDahan(1);

			// If the Terro Level is 2 or lower
			if( ctx.GameState.Fear.TerrorLevel <= 2 ) {
				// Gather 1 town
				await ctx.Gather( 1, Invader.Town );

				// And the first ravage in target land becomes a build there instead.
				FirstRavageBecomesABuild( ctx );
			}

		}

		static void FirstRavageBecomesABuild( TargetSpaceCtx ctx ) {
			ctx.GameState.PreRavaging.ForRound.Add( (gs, args)=>{
				if(!args.Spaces.Contains( ctx.Space )) return;

				// Stop Ravage
				args.Skip1(ctx.Space); // Stop Ravage

				// Add Build
				gs.PreBuilding.ForRound.Add( ( _, buildArgs ) => buildArgs.Add( ctx.Space ) );

			});
		}

	}

}
