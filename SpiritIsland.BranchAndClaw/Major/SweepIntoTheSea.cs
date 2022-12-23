namespace SpiritIsland.BranchAndClaw;

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
		var oceans = ctx.GameState.Island.Boards.Select( b => ctx.GameState.Tokens[b.Ocean] );
		var distanceFromOceans = new DistanceFromOceanCalculator()
			.SetTargets( oceans )
			.Calculate();
		int curDistance = distanceFromOceans[ctx.Tokens];
		var closerSpace = await ctx.SelectAdjacentLand( "Push explorer/town towards ocean", null, a => distanceFromOceans[a.Tokens] < curDistance );
		return closerSpace;
	}

	static async Task PushAllTokensTo( TargetSpaceCtx ctx, TargetSpaceCtx destination, params HealthTokenClass[] groups ) {
		while(ctx.Tokens.HasAny( groups ))
			await ctx.Move( ctx.Tokens.OfAnyHealthClass( groups ).First(), ctx.Space, destination.Space );
	}

	#region DistanceFromOceanCalculator

	class DistanceFromOceanCalculator {

		readonly Queue<SpaceState> spacesLessThanLimit = new();
		readonly Dictionary<SpaceState, int> shortestDistances = new();

		public DistanceFromOceanCalculator SetTargets( IEnumerable<SpaceState> targets ) {
			foreach(var target in targets) {
				shortestDistances.Add( target, 0 );
				spacesLessThanLimit.Enqueue( target );
			}
			return this;
		}

		public DistanceFromOceanCalculator Calculate() {
			while(spacesLessThanLimit.Count > 0) {
				// get next
				var cur = spacesLessThanLimit.Dequeue(); // Instead of just getting next, we could select the lowest value first

				// add neighbors to dictionary and evaluated its neighbors
				int neighborDist = shortestDistances[cur] + 1;
				foreach(var a in cur.Adjacent) {
					if(!shortestDistances.ContainsKey( a ) || neighborDist < shortestDistances[a]) {
						shortestDistances[a] = neighborDist;
						spacesLessThanLimit.Enqueue( a );
					}
				}
			}
			return this;
		}

		public int this[SpaceState space] => shortestDistances[space];

	}
	#endregion DistanceFromOceanCalculator

}