using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Core {

	public static class SpecificEngineExtensions{

		static public async Task SelectActionsAndMakeFast(this ActionEngine engine, Spirit spirit, int countToMakeFast ) {
			var actionFactories = spirit.GetUnresolvedActionFactories( Speed.Slow ).ToArray();
			while(actionFactories.Length > 0 && countToMakeFast > 0) {
				var factory = await engine.SelectFactory(
					"Select action to make fast",
					actionFactories,
					true
				);

				spirit.Resolve( factory );
				spirit.AddActionFactory( new ChangeSpeed( factory, Speed.Fast ) );
			}
		}

		static public async Task GatherUpToNDahan( this ActionEngine eng, Space target, int dahanToGather ) {
  			int gathered = 0;
			var neighborsWithDahan = target.Neighbors.Where(eng.GameState.HasDahan).ToArray();
			while(gathered<dahanToGather && neighborsWithDahan.Length>0){
				var source = await eng.SelectSpace( $"Gather dahan {gathered+1} of {dahanToGather} from:", neighborsWithDahan, true);
				if(source == null) break;

				eng.GameState.AddDahan(source,-1);
				eng.GameState.AddDahan(target,1);

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

				eng.GameState.Adjust(source,invader,-1);
				eng.GameState.Adjust(target,invader,1);

				++gathered;
				neighborsWithItems = CalcSource();
			}

		}

		static public async Task PushUpToNDahan( this ActionEngine eng, Space source, int dahanToPush) {
			dahanToPush = System.Math.Min(dahanToPush,eng.GameState.GetDahanOnSpace(source));
			while(0<dahanToPush){
				Space destination = await eng.SelectSpace("Select destination for dahan"
					,source.Neighbors
					,true
				);
				if(destination == null) break;
				eng.GameState.AddDahan(source,-1);
				eng.GameState.AddDahan(destination,1);
				--dahanToPush;
			}
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
			eng.GameState.Adjust(source,invader,-1);
			eng.GameState.Adjust(destination,invader,1);
		}

		static public async Task Push1Dahan( this ActionEngine eng, Space source){
  			var destination = await eng.SelectSpace("Push dahan to"
				,source.Neighbors.Where(x=>x.IsLand)
			);
			eng.GameState.AddDahan(source,-1);
			eng.GameState.AddDahan(destination,1);
		}

	}

	/*
	// var promise = new Promise<MyResult>;
	var promise = new TaskCompletionSource<MyResult>();

	// handlerMyEventsWithHandler(msg => promise.Complete(msg););
	handlerMyEventsWithHandler(msg => promise.TrySetResult(msg));

	// var myResult = promise.Future.Await(2000);
	var completed = await Task.WhenAny(promise.Task, Task.Delay(2000));
	if (completed == promise.Task)
	  ; // Do something on timeout
	var myResult = await completed;

	Assert.Equals("my header", myResult.Header);
	*/


}