using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public partial class SweepIntoTheSea {

		[MajorCard( "Sweep into the Sea", 4, Element.Sun, Element.Air, Element.Water )]
		[Slow]
		[FromPresence( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			await DoPower( ctx );
			if(await ctx.YouHave("3 sun,2 water"))
				await DoPower( await ctx.SelectAdjacentLand("Repeat power") );
		}

		static async Task DoPower( TargetSpaceCtx ctx ) {
			await ctx.SelectActionOption(
				new SpaceAction( "Push explorers and towns toward nearest ocean", PushExplorersAndTownsTowardsOcean ),
				new SpaceAction( "Destroy all explorers and towns", ctx => ctx.Invaders.DestroyAny( int.MaxValue, Invader.Explorer, Invader.Town ) ).Matches( x => x.IsCoastal )
			);
		}

		static async Task PushExplorersAndTownsTowardsOcean( TargetSpaceCtx ctx ) {
			// push all explorers and town one land toward the nearest ocean
			var closerSpace = await SelectSpaceCloserToTheOcean( ctx );
			await PushAllTokensTo( ctx, closerSpace, Invader.Explorer, Invader.Town );
		}

		static async Task<TargetSpaceCtx> SelectSpaceCloserToTheOcean( TargetSpaceCtx ctx ) {
			var oceans = ctx.GameState.Island.Boards.Select( b => b.Ocean );
			var distanceFromOceans = new MinDistanceCalculator()
				.SetTargets( oceans )
				.Calculate();
			int curDistance = distanceFromOceans[ctx.Space];
			var closerSpace = await ctx.SelectAdjacentLand( "Push explorer/town towards ocean", a => distanceFromOceans[a.Space] < curDistance );
			return closerSpace;
		}

		static async Task PushAllTokensTo( TargetSpaceCtx ctx, TargetSpaceCtx destination, params TokenClass[] groups ) {
			while(ctx.Tokens.HasAny( groups ))
				await ctx.Move( ctx.Tokens.OfAnyType( groups ).First(), ctx.Space, destination.Space );
		}

	}

}
