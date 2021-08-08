﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Core {

	public static class SpecificEngineExtensions{

		static public async Task SelectActionsAndMakeFast(this ActionEngine engine, Spirit spirit, int maxCountToMakeFast ) {

			IActionFactory[] CalcSlowFacts() => spirit
				.GetUnresolvedActionFactories( Speed.Slow )
				.ToArray();
			var slowFactories = CalcSlowFacts();
			// clip count to available slow stuff
			maxCountToMakeFast = Math.Min(maxCountToMakeFast, slowFactories.Length); // !! unit test that we are limited by slow cards & by countToMakeFAst
			while( maxCountToMakeFast > 0 ) {
				var factory = await engine.SelectFactory(
					$"Select action to make fast. max:{maxCountToMakeFast}",
					slowFactories,
					true
				);

				spirit.RemoveUnresolvedFactory( factory ); // remove it as slow
				spirit.AddActionFactory( new ChangeSpeed( factory, Speed.Fast ) ); // add as fast

				slowFactories = CalcSlowFacts();
				--maxCountToMakeFast;
			}
		}

		static public async Task SelectSpaceCardToReplayForCost( this ActionEngine engine, Spirit spirit, int maxCost, List<SpaceTargetedArgs> played ) {
			maxCost = Math.Min(maxCost,spirit.Energy);
			var options = played.Select(p=>p.Card).ToArray();
			if(options.Length == 0) return;
			var factory = (TargetSpace_PowerCard)await engine.SelectFactory("Select card to replay",options);

            spirit.Energy -= factory.Cost;
			spirit.AddActionFactory( new ReplayOnSpace( factory, played.Single(p=>p.Card==factory ).Target ));
		}

		static public async Task GatherUpToNDahan( this ActionEngine eng, Space target, int dahanToGather ) {
  			int gathered = 0;
			var neighborsWithDahan = target.Neighbors.Where(eng.GameState.HasDahan).ToArray();
			while(gathered<dahanToGather && neighborsWithDahan.Length>0){
				var source = await eng.SelectSpace( $"Gather dahan {gathered+1} of {dahanToGather} from:", neighborsWithDahan, true);
				if(source == null) break;

				await eng.GameState.MoveDahan(source,target);

				++gathered;
				neighborsWithDahan = target.Neighbors.Where(eng.GameState.HasDahan).ToArray();
			}

		}

		static public async Task GatherUpToNInvaders( this ActionEngine eng, Space target, int countToGather, params Invader[] ofType ) {
			Invader[] spaceInvaders(Space space) => eng.GameState.InvadersOn(space).FilterBy(ofType);
			Space[] CalcSource() => target.Neighbors
				.Where(s=>spaceInvaders(s).Any())
				.ToArray();

			string label = ofType.Select(it=>it.Label).Join("/");

			Space[] neighborsWithItems = CalcSource();
  			int gathered = 0;
			while(gathered<countToGather && neighborsWithItems.Length>0){
				var source = await eng.SelectSpace( $"Gather {label} {gathered+1} of {countToGather} from:", neighborsWithItems, true);
				if(source == null) break;

				var invader = await eng.SelectInvader("Select invader to gather "+source.Label+" => "+target.Label,spaceInvaders(source));

				eng.GameState.Move(invader,source,target);

				++gathered;
				neighborsWithItems = CalcSource();
			}

		}

		static public async Task<Space[]> PushUpToNDahan( this ActionEngine eng, Space source, int dahanToPush) {
			HashSet<Space> pushedToLands = new HashSet<Space>();
			dahanToPush = System.Math.Min(dahanToPush,eng.GameState.GetDahanOnSpace(source));
			while(0<dahanToPush){
				Space destination = await eng.SelectSpace("Select destination for dahan"
					,source.Neighbors.Where(n=>n.IsLand)
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
			Invader[] CalcInvaderTypes() => eng.GameState.InvadersOn(source).FilterBy(healthyInvaders);
			var invaders = CalcInvaderTypes();
			while(0<countToPush && 0<invaders.Length){
				var invader = await eng.SelectInvader("Select invader to push",invaders,true);
				if(invader==null) break;
				await eng.PushInvader(source,invader);

				--countToPush;
				invaders = CalcInvaderTypes();
			}
		}

		static public async Task PushInvader( this ActionEngine eng, Space source, Invader invader){
  			var destination = await eng.SelectSpace("Push "+invader.Summary+" to"
				,source.Neighbors.Where(x=>x.IsLand)
			);
			eng.GameState.Move(invader,source,destination);
		}

		static Invader[] KillOrder => killOrder ??= "C@1 C@2 C@3 T@1 T@2 E@1".Split( ' ' ).Select( k => Invader.Lookup[k] ).ToArray();
		static Invader[] killOrder;

		static Invader[] LeftOverOrder => leftOverOrder ??= "C@2 T@2 C@3".Split( ' ' ).Select( k => Invader.Lookup[k] ).ToArray();
		static Invader[] leftOverOrder;

		// !!! Swap this out with user choosing.  Which user chooses when dahan are doing damage????
		static public Task ApplyDamage( this InvaderGroup grp, int startingDamage, List<string> log = null ) {
			int damageToInvaders = startingDamage;

			// While damage remains    &&    we have invaders
			while(damageToInvaders > 0 && grp.InvaderTypesPresent.Any()) {
				var invaderToDamage = KillOrder
					.FirstOrDefault( invader =>
						invader.Health <= damageToInvaders // prefer things we can kill
						&& grp[invader] > 0
					)
					?? LeftOverOrder.First( invader => grp[invader] > 0 ); // left-over damage

				damageToInvaders -= grp.ApplyDamage( invaderToDamage, damageToInvaders );

			}
			if(log != null) log.Add( $"{startingDamage} damage to invaders leaving {grp}." );

			return Task.CompletedTask;
		}


	}

}