using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Core {

	// where is the best place for these methods to live?
	// Engine?   ActionBase?   Extension Methods?
	static class ActionEngineExtensions {

		static public Task<Spirit> SelectSpirit( this ActionEngine engine, Spirit[] spirits ) {
			var result = new TaskCompletionSource<Spirit>();

			engine.decisions.Push( new SelectSpirit( spirits
				, spirit => result.TrySetResult( spirit )
			) );

			return result.Task;
		}

		static public Task<Space> SelectSpace( this ActionEngine engine,
			string prompt,
			IEnumerable<Space> spaces,
			bool allowShortCircuit = false
		) {
			var result = new TaskCompletionSource<Space>();
			engine.decisions.Push( new SelectAsync<Space>( prompt, spaces.ToArray(), allowShortCircuit, result ) );
			return result.Task;
		}



		#region Target Helpers

		static public Task<Spirit> TargetSpirit(this ActionEngine engine)
			=> engine.SelectSpirit(engine.GameState.Spirits);

		static public async Task<Space> TargetSpace_Presence(this ActionEngine engine, int range){
			return await engine.SelectSpace("Select target.",engine.Self.Presence.Range(range));
		}
		static public async Task<Space> TargetSpace_Presence(this ActionEngine engine, int range, Func<Space,bool> filter){
			return await engine.SelectSpace("Select target.",engine.Self.Presence.Range(range).Where(filter));
		}

		static public async Task<Space> TargetSpace_SacredSite(this ActionEngine engine, int range){
			return await engine.SelectSpace("Select target.",engine.Self.SacredSites.Range(range));
		}
		static public async Task<Space> TargetSpace_SacredSite(this ActionEngine engine, int range, Func<Space,bool> filter){
			return await engine.SelectSpace("Select target.",engine.Self.SacredSites.Range(range).Where(filter));
		}
		#endregion

		static public Task<Invader> SelectInvader( this ActionEngine engine
			,string prompt
			,Invader[] invaders
			,bool allowShortCircuit=false
		) {
			var result = new TaskCompletionSource<Invader>();
			engine.decisions.Push( new SelectAsync<Invader>( 
				prompt, 
				invaders,
				allowShortCircuit,
				result 
			));
			return result.Task;
		}

		static public Task<IActionFactory> SelectFactory( this ActionEngine engine
			,string prompt
			,IActionFactory[] options
			,bool allowShortCircuit=false
		) {
			var result = new TaskCompletionSource<IActionFactory>();
			engine.decisions.Push( new SelectAsync<IActionFactory>( 
				prompt, 
				options,
				allowShortCircuit,
				result 
			));
			return result.Task;
		}


		static public Task<string> SelectText( this ActionEngine engine
			,string prompt
			,params string[] options
		) {
			var result = new TaskCompletionSource<string>();
			engine.decisions.Push( new SelectTextAsync(prompt, options,result));
			return result.Task;
		}

	}

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

		static public async Task GatherDahan( this ActionEngine eng, Space target, int dahanToGather ) {
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
			Invader[] CalcInvaderTypes() => eng.GameState.InvadersOn(source).FilterByHealthy(healthyInvaders);
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