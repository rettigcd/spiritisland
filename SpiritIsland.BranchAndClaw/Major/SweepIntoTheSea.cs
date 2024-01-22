namespace SpiritIsland.BranchAndClaw;

public partial class SweepIntoTheSea {

	[MajorCard( "Sweep Into the Sea", 4, Element.Sun, Element.Air, Element.Water ), Slow, FromPresence( 2 )]
	[Instructions( "Push all Explorer and Town one land towards the nearest Ocean. -or- If target land is Coastal, destroy all Explorer and Town. -If you have- 3 Sun, 2 Water: Repeat on an adjacent land." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		await DoPower( ctx );
		if(await ctx.YouHave("3 sun,2 water"))
			await DoPower( await ctx.SelectAdjacentLandAsync("Repeat power") );
	}

	static async Task DoPower( TargetSpaceCtx ctx ) {
		await ctx.SelectActionOption(
			new SpaceAction( "Push explorers and towns toward nearest ocean", PushExplorersAndTownsTowardsOcean ),
			new SpaceAction( "Destroy all explorers and towns", ctx => ctx.Invaders.DestroyAll( Human.Explorer_Town ) ).OnlyExecuteIf( x => x.IsCoastal )
		);
	}

	static async Task PushExplorersAndTownsTowardsOcean( TargetSpaceCtx ctx ) {
		// push all explorers and town one land toward the nearest ocean
		var closerSpace = (await SelectSpaceCloserToTheOcean( ctx )).Tokens;
		await PushAllTokensTo( ctx, closerSpace, Human.Explorer_Town );
	}

	static async Task<TargetSpaceCtx> SelectSpaceCloserToTheOcean( TargetSpaceCtx ctx ) {
		var oceans = GameState.Current.Island.Boards.Select( b => b.Ocean.Tokens );
		var distanceFromOceans = new DistanceFromOceanCalculator()
			.SetTargets( oceans )
			.Calculate();
		int curDistance = distanceFromOceans[ctx.Tokens];

		Space space = await ctx.Self.SelectAsync( 
			new A.Space( "Push explorer/town towards ocean", 
			ctx.Tokens.Adjacent.Where( tokens => distanceFromOceans[tokens] < curDistance ), 
			Present.Always 
		) );
		return space != null ? ctx.Target( space )
			: null;
	}

	static async Task PushAllTokensTo( TargetSpaceCtx ctx, SpaceState destination, params HumanTokenClass[] groups ) {
		// !!! This is supposed to be Push! not a Move - does it make a difference?
		while(ctx.Tokens.HasAny( groups ))
			await ctx.Tokens.HumanOfAnyTag( groups ).First().MoveAsync(ctx.Space,destination.Space);
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
					if(!shortestDistances.TryGetValue( a, out int shortedDistance ) || neighborDist < shortedDistance) {
						shortedDistance = neighborDist;
						shortestDistances[a] = shortedDistance;
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