using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {
	public static class IMakeGamestateDecisionsExtensions {

		static public IMakeGamestateDecisions MakeDecisionsFor(this Spirit spirit, GameState gameState ) => new GsDecisionMaker(spirit,gameState);

		class GsDecisionMaker : IMakeGamestateDecisions {
			public GsDecisionMaker(Spirit spirit,GameState gs ) { this.Self = spirit; GameState = gs; }
			public Spirit Self { get; }

			public GameState GameState { get; }
		}

		#region Gather

		static public async Task GatherUpToNTokens( this IMakeGamestateDecisions ctx, Space target, int countToGather, params TokenGroup[] groups ) {
			Token[] calcTokens(Space space) => ctx.GameState.Tokens[space].OfAnyType(groups);
			Space[] CalcSource() => target.Adjacent
				.Where(s=>calcTokens(s).Any())
				.ToArray();

			string label = groups.Select(it=>it.Label).Join("/");

			Space[] neighborsWithItems = CalcSource();
  			int gathered = 0;
			while(gathered<countToGather && neighborsWithItems.Length>0){
				var source = await ctx.Self.Action.Choose( new GatherTokensFromDecision( countToGather-gathered, groups, target, neighborsWithItems, Present.Done ));
				if(source == null) break;

				var invader = await ctx.Self.Action.Choose( new SelectTokenToGatherDecision( source, target, calcTokens(source), Present.IfMoreThan1 ) );

				await ctx.GameState.Move(invader, source, target);

				++gathered;
				neighborsWithItems = CalcSource();
			}

		}

		#endregion Gather

		#region Push

		static public Task PushNTokens( this IMakeGamestateDecisions ctx, Space source, int countToPush , params TokenGroup[] groups ) 
			=> ctx.FearPushTokens(source,true,countToPush,groups);

		static public Task FearPushUpToNTokens( this IMakeGamestateDecisions ctx, Space source, int countToPush , params TokenGroup[] groups ) 
			=> ctx.FearPushTokens( source, false, countToPush, groups );

		static public async Task FearPushTokens( this IMakeGamestateDecisions ctx, Space source, bool force, int countToPush
			,params TokenGroup[] groups
		) {

			var counts = ctx.GameState.Tokens[source];
			Token[] GetTokens() => counts.OfAnyType(groups);
			countToPush = System.Math.Min(countToPush,counts.SumAny(groups));

			var tokens = GetTokens();
			while(0<countToPush && 0<tokens.Length){
				var token = await ctx.Self.Action.Choose( new SelectTokenToPushDecision( source, countToPush, tokens, force ? Present.IfMoreThan1 : Present.Done ) );

				if(token==null) 
					break;

				var destination = await ctx.Self.Action.Choose( new PushTokenDecision(
					token,
					source,
					source.Adjacent.Where( x => x.Terrain != Terrain.Ocean ),
					Present.Always
				)); 
				await ctx.GameState.Move(token, source, destination );

				--countToPush;
				tokens = GetTokens();
			}
		}

		#endregion Push

		#region Place Presence

		static public Task PlacePresence( this IMakeGamestateDecisions engine, int range, Target filterEnum ) {
			Space[] destinationOptions = CalcDestinationOptions( engine, range, filterEnum );
			return engine.PlacePresence( destinationOptions );
		}

		static Space[] CalcDestinationOptions( IMakeGamestateDecisions engine, int range, Target filterEnum ) {
			// Calculate options
			var existing = engine.Self.Presence.Spaces.ToArray();

			var inRange = existing
				.SelectMany( s => s.Range( range ) )
				.Distinct()
				.ToArray();

			Space[] destinationOptions = inRange
				.Where( SpaceFilter.ForPlacingPresence.GetFilter( engine.Self, engine.GameState, filterEnum ) )
				.OrderBy( x => x.Label )
				.ToArray();
			return destinationOptions.Length == 0
				? throw new System.Exception( "dude you don't have anywhere to place your presence" )
				: destinationOptions;
		}

		static public async Task PlacePresence( this IMakeGamestateDecisions engine, params Space[] destinationOptions ) {
			var from = await engine.Self.SelectTrack();
			var to = await engine.Self.Action.Choose( new TargetSpaceDecision( "Where would you like to place your presence?", destinationOptions, Present.Always ));
			await engine.Self.Presence.PlaceFromBoard(from, to, engine.GameState );
		}

		#endregion Place Presence


		/// <summary>
		/// Used for Power-targetting, where range sympols appear.
		/// </summary>
		static public Task<Space> PowerTargetsSpace( this IMakeGamestateDecisions engine, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum )
			=> engine.Self.PowerApi.TargetsSpace( engine.Self, engine.GameState, sourceEnum, sourceTerrain, range, filterEnum );

		//// Not Changable!
		static public InvaderGroup InvadersOn( this IMakeGamestateDecisions engine, Space space )
			=> engine.Self.BuildInvaderGroupForPowers( engine.GameState, space );

		static public async Task DamageInvaders( this IMakeGamestateDecisions engine, Space space, int damage ) { // !!! let players choose the item to apply damage to
			if(damage == 0) return;
			await engine.InvadersOn( space ).ApplySmartDamageToGroup( damage );
		}

	}

}