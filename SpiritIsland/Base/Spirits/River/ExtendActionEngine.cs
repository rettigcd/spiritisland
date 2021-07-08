using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	// where is the best place for these methods to live?
	// Engine?   ActionBase?   Extension Methods?
	static class ExtendActionEngine {

		static public Task<Spirit> SelectSpirit(this ActionEngine engine, params Spirit[] spirits){
			var result = new TaskCompletionSource<Spirit>();

			engine.decisions.Push(new SelectSpirit(spirits
				,spirit => result.TrySetResult(spirit)
			));

			return result.Task;
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