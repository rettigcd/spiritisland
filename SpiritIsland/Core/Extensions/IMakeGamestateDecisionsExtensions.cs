﻿using System.Collections.Generic;
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

		static public async Task GatherUpToNDahan( this IMakeGamestateDecisions eng, Space target, int dahanToGather ) {
  			int gathered = 0;
			var neighborsWithDahan = target.Adjacent.Where(eng.GameState.Dahan.Has).ToArray();
			while(gathered<dahanToGather && neighborsWithDahan.Length>0){
				var source = await eng.Self.Action.Choose( new GatherDahanFromDecision( dahanToGather-gathered, target, neighborsWithDahan, Present.Done));
				if(source == null) break;

				await eng.GameState.Dahan.Move(source,target);

				++gathered;
				neighborsWithDahan = target.Adjacent.Where(eng.GameState.Dahan.Has).ToArray();
			}

		}

		static public async Task GatherUpToNInvaders( this IMakeGamestateDecisions ctx, Space target, int countToGather, params Invader[] invaderTypes ) {
			InvaderSpecific[] spaceInvaders(Space space) => ctx.GameState.Invaders.Counts[space].FilterBy(invaderTypes);
			Space[] CalcSource() => target.Adjacent
				.Where(s=>spaceInvaders(s).Any())
				.ToArray();

			string label = invaderTypes.Select(it=>it.Label).Join("/");

			Space[] neighborsWithItems = CalcSource();
  			int gathered = 0;
			while(gathered<countToGather && neighborsWithItems.Length>0){
				var source = await ctx.Self.Action.Choose( new GatherInvaderFromDecision( countToGather-gathered, invaderTypes, target, neighborsWithItems, Present.Done ));
				if(source == null) break;

				var invader = await ctx.Self.Action.Choose( new SelectInvaderToGatherDecision( source, target, spaceInvaders(source), Present.IfMoreThan1 ) );

				await ctx.GameState.Invaders.Move(invader, source, target);

				++gathered;
				neighborsWithItems = CalcSource();
			}

		}

		#endregion Gather

		#region Push

		static public async Task<Space[]> FearPushUpToNDahan( this IMakeGamestateDecisions eng, Space source, int dahanToPush) {
			HashSet<Space> pushedToLands = new HashSet<Space>();
			dahanToPush = System.Math.Min(dahanToPush,eng.GameState.Dahan.Count(source));
			while(0<dahanToPush){
				Space destination = await eng.Self.Action.Choose(new PushDahanDecision(
					source
					,source.Adjacent.Where(n=>n.Terrain != Terrain.Ocean ) // This is non-power push, so directly checking ocean is ok
					,Present.Done
				));
				if(destination == null) break;
				pushedToLands.Add(destination);
				await eng.GameState.Dahan.Move(source,destination);
				--dahanToPush;
			}
			return pushedToLands.ToArray();
		}

		// non-power push (for fear)
		static public async Task FearPushUpToNInvaders( this IMakeGamestateDecisions ctx, Space source, int countToPush
			,params Invader[] healthyInvaders
		) {

			InvaderSpecific[] CalcInvaderTypes() => ctx.GameState.Invaders.Counts[source].FilterBy(healthyInvaders);

			var invaders = CalcInvaderTypes();
			while(0<countToPush && 0<invaders.Length){
				var invader = await ctx.Self.Action.Choose( new SelectInvaderToPushDecision( source, countToPush, invaders, Present.Done ) );

				if(invader==null) 
					break;

				var destination = await ctx.Self.Action.Choose( new PushInvaderDecision(
					invader,
					source,
					source.Adjacent.Where( x => x.Terrain != Terrain.Ocean ),
					Present.Always
				)); 
				await ctx.GameState.Invaders.Move(invader, source, destination );

				--countToPush;
				invaders = CalcInvaderTypes();
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