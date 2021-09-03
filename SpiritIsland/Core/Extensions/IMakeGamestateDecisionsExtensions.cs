using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	// High-Level game actions
	public static class IMakeGamestateDecisionsExtensions {

		static public IMakeGamestateDecisions MakeDecisionsFor(this Spirit spirit, GameState gameState ) 
//			=> new GsDecisionMaker(spirit,gameState);
			=> new PowerCtx( spirit, gameState );

		#region IMakeGamestateDecision class

		class GsDecisionMaker : IMakeGamestateDecisions {
			public GsDecisionMaker(Spirit spirit,GameState gs ) { this.Self = spirit; GameState = gs; }
			public Spirit Self { get; }

			public GameState GameState { get; }
		}

		#endregion

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

		static public Task<Space[]> PushNTokens( this IMakeGamestateDecisions ctx, Space source, int countToPush , params TokenGroup[] groups ) 
			=> ctx.PushTokens_Inner(source,countToPush,groups, Present.IfMoreThan1 );

		static public Task<Space[]> PushUpToNTokens( this IMakeGamestateDecisions ctx, Space source, int countToPush , params TokenGroup[] groups ) 
			=> ctx.PushTokens_Inner( source, countToPush, groups, Present.Done );

		static async Task<Space[]> PushTokens_Inner( this IMakeGamestateDecisions ctx, Space source, int countToPush, TokenGroup[] groups, Present present )  {

			// !!! This next line needs to Use Power Adjacents for Powers
			var destinationOptions = source.Adjacent.Where( x => x.Terrain != Terrain.Ocean );

			var counts = ctx.GameState.Tokens[source];
			Token[] GetTokens() => counts.OfAnyType(groups);
			countToPush = System.Math.Min(countToPush,counts.SumAny(groups));

			var pushedToSpaces = new List<Space>();

			var tokens = GetTokens();
			while(0<countToPush && 0<tokens.Length){
				var decision = new SelectTokenToPushDecision( source, countToPush, tokens, present );
				var token = await ctx.Self.Action.Choose( decision );

				if(token==null) 
					break;

				var destination = await ctx.Self.Action.Choose( new PushTokenDecision(token,source,destinationOptions,Present.Always)); 
				await ctx.GameState.Move(token, source, destination );

				pushedToSpaces.Add( destination );
				--countToPush;
				tokens = GetTokens();
			}
			return pushedToSpaces.ToArray();
		}

		#endregion Push

		#region Place Presence

		static public Task PlacePresence( this IMakeGamestateDecisions engine, int range, Target filterEnum ) {
			Space[] destinationOptions = CalcDestinationOptions( engine, range, filterEnum );
			return engine.PlacePresence( destinationOptions );
		}

		static public async Task PlacePresence( this IMakeGamestateDecisions engine, params Space[] destinationOptions ) {
			var from = await engine.Self.SelectTrack();
			var to = await engine.Self.Action.Choose( new TargetSpaceDecision( "Where would you like to place your presence?", destinationOptions, Present.Always ));
			await engine.Self.Presence.PlaceFromBoard(from, to, engine.GameState );
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

		#endregion Place Presence

		#region Damage Invaders

		static public InvaderGroup InvadersOn( this IMakeGamestateDecisions engine, Space space )
			=> engine.Self.BuildInvaderGroupForPowers( engine.GameState, space );

		static public async Task DamageInvaders( this IMakeGamestateDecisions engine, Space space, int damage ) { // !!! let players choose the item to apply damage to
			if(damage == 0) return;
			await engine.InvadersOn( space ).ApplySmartDamageToGroup( damage );
		}

		#endregion

		/// <summary>
		/// Used for Power-targetting, where range sympols appear.
		/// </summary>
		static public Task<Space> PowerTargetsSpace( this IMakeGamestateDecisions engine, From sourceEnum, Terrain? sourceTerrain, int range, Target filterEnum )
			=> engine.Self.PowerApi.TargetsSpace( engine.Self, engine.GameState, sourceEnum, sourceTerrain, range, filterEnum );

	}

}