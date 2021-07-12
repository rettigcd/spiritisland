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
			return await engine.SelectSpace("Select target."
				,engine.Self.Presence.Range(range));
		}

		static public async Task<Space> TargetSpace_SacredSite(this ActionEngine engine, int range){
			return await engine.SelectSpace("Select target."
				,engine.Self.SacredSites.Range(range));
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
			var neighborsWithDahan = target.Neighbors.Where( eng.GameState.HasDahan ).ToArray();
			while(dahanToGather > 0 && neighborsWithDahan.Length > 0) {
				Space source = await eng.SelectSpace(
					"Select source land to gather Dahan into " + target.Label,
					neighborsWithDahan, true
				);
				if(source == null) break;

				new MoveDahan( source, target ).Apply( eng.GameState );
				--dahanToGather;
				neighborsWithDahan = target.Neighbors.Where( eng.GameState.HasDahan ).ToArray();
			}
		}

		static public async Task PushInvader( this ActionEngine eng, Space source, Invader invader){
  			var destination = await eng.SelectSpace("Select space to push town",source.Neighbors);
			eng.GameState.Adjust(source,invader,-1);
			eng.GameState.Adjust(destination,invader,1);
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