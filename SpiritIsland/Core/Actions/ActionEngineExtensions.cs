﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

	// ! this could split up into 3 Extension files
	// * GatherExtensions
	// * PushExtesnsions
	// * PlacePresenceExtensions

	public static class SpecificEngineExtensions{

		#region Gather

		static public async Task GatherUpToNDahan( this ActionEngine eng, Space target, int dahanToGather ) {
  			int gathered = 0;
			var neighborsWithDahan = target.Adjacent.Where(eng.GameState.HasDahan).ToArray();
			while(gathered<dahanToGather && neighborsWithDahan.Length>0){
				var source = await eng.Self.SelectSpace( $"Gather dahan {gathered+1} of {dahanToGather} from:", neighborsWithDahan, true);
				if(source == null) break;

				await eng.GameState.MoveDahan(source,target);

				++gathered;
				neighborsWithDahan = target.Adjacent.Where(eng.GameState.HasDahan).ToArray();
			}

		}

		static public async Task GatherUpToNInvaders( this ActionEngine eng, Space target, int countToGather, params Invader[] ofType ) {
			InvaderSpecific[] spaceInvaders(Space space) => eng.GameState.InvadersOn(space).FilterBy(ofType);
			Space[] CalcSource() => target.Adjacent
				.Where(s=>spaceInvaders(s).Any())
				.ToArray();

			string label = ofType.Select(it=>it.Label).Join("/");

			Space[] neighborsWithItems = CalcSource();
  			int gathered = 0;
			while(gathered<countToGather && neighborsWithItems.Length>0){
				var source = await eng.Self.SelectSpace( $"Gather {label} {gathered+1} of {countToGather} from:", neighborsWithItems, true);
				if(source == null) break;

				var invader = await eng.Self.SelectInvader("Select invader to gather "+source.Label+" => "+target.Label,spaceInvaders(source));

				eng.GameState.Move(invader,source,target);

				++gathered;
				neighborsWithItems = CalcSource();
			}

		}

		#endregion Gather

		#region Push

		static public async Task<Space[]> PushUpToNDahan( this ActionEngine eng, Space source, int dahanToPush) {
			HashSet<Space> pushedToLands = new HashSet<Space>();
			dahanToPush = System.Math.Min(dahanToPush,eng.GameState.GetDahanOnSpace(source));
			while(0<dahanToPush){
				Space destination = await eng.Self.SelectSpace("Select destination for dahan"
					,source.Adjacent.Where(n=>n.IsLand)
					,true
				);
				if(destination == null) break;
				pushedToLands.Add(destination);
				await eng.GameState.MoveDahan(source,destination);
				--dahanToPush;
			}
			return pushedToLands.ToArray();
		}

		static public async Task PushUpToNInvaders( this ActionEngine eng, Space source, int countToPush
			,params Invader[] healthyInvaders
		) {

			InvaderSpecific[] CalcInvaderTypes() => eng.GameState.InvadersOn(source).FilterBy(healthyInvaders);
			var invaders = CalcInvaderTypes();
			while(0<countToPush && 0<invaders.Length){
				var invader = await eng.Self.SelectInvader("Select invader to push",invaders,true);
				if(invader==null) break;

				var destination = await eng.Self.SelectSpace( "Push " + invader.Summary + " to", source.Adjacent.Where( x => x.IsLand ) );
				eng.GameState.Move( invader, source, destination );

				--countToPush;
				invaders = CalcInvaderTypes();
			}
		}

		#endregion Push

		#region Place Presence

		static public Task PlacePresence( this ActionEngine engine, int range, Target filterEnum ) {
			Space[] destinationOptions = engine.Self.Presence.Spaces
				.SelectMany( s => s.SpacesWithin( range ) )
				.Distinct()
				.Where( TargetSpaceAttribute.ToLambda( engine.Self, engine.GameState, filterEnum ) )
				.OrderBy( x => x.Label )
				.ToArray();
			return engine.PlacePresence(destinationOptions);
		}

		static public async Task PlacePresence( this ActionEngine engine, params Space[] destinationOptions ) {

			var from = await engine.Self.SelectTrack();
			var to = await engine.Self.SelectSpace( "Where would you like to place your presence?", destinationOptions );
			engine.Self.Presence.PlaceFromBoard( from, to );

		}

		#endregion Place Presence

	}

}