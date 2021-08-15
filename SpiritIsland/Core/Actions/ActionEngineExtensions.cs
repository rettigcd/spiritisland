using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland {

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
			var neighborsWithDahan = target.Adjacent.Where(eng.GameState.HasDahan).ToArray();
			while(gathered<dahanToGather && neighborsWithDahan.Length>0){
				var source = await eng.SelectSpace( $"Gather dahan {gathered+1} of {dahanToGather} from:", neighborsWithDahan, true);
				if(source == null) break;

				await eng.GameState.MoveDahan(source,target);

				++gathered;
				neighborsWithDahan = target.Adjacent.Where(eng.GameState.HasDahan).ToArray();
			}

		}

		static public async Task GatherUpToNInvaders( this ActionEngine eng, Space target, int countToGather, params Invader[] ofType ) {
			Invader[] spaceInvaders(Space space) => eng.GameState.InvadersOn(space).FilterBy(ofType);
			Space[] CalcSource() => target.Adjacent
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
				,source.Adjacent.Where(x=>x.IsLand)
			);
			eng.GameState.Move(invader,source,destination);
		}

		static public Task PlacePresence( this ActionEngine engine, int range, Target filterEnum ) {
			Space[] destinationOptions = engine.Self.Presence.Spaces
				.SelectMany( s => s.SpacesWithin( range ) )
				.Distinct()
				.Where( TargetSpaceAttribute.ToLambda( engine, filterEnum ) )
				.OrderBy( x => x.Label )
				.ToArray();
			return engine.PlacePresence(destinationOptions);
		}

		static public async Task PlacePresence( this ActionEngine engine, params Space[] destinationOptions ) {

			var from = await engine.SelectTrack();
			var to = await engine.SelectSpace( "Where would you like to place your presence?", destinationOptions );
			engine.Self.Presence.PlaceFromBoard( from, to );

		}

	}

}