using System.Linq;
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

		static public Task<Space> SelectSpace(this ActionEngine engine, Spirit spirit, int range, From @from, System.Func<Space,bool> filter=null){
			if(filter==null) filter = (s)=>true;

			var result = new TaskCompletionSource<Space>();

			var decision = @from == From.Presence
				? (IDecision)new SelectSpaceRangeFromPresence(spirit,range,filter,space=>result.TrySetResult(space))
				: (IDecision)new SelectSpaceRangeFromSacredSite(spirit,range,filter,space=>result.TrySetResult(space));
			engine.decisions.Push(decision);

			return result.Task;
		}

		static public Task<Invader> SelectInvader(this ActionEngine engine, InvaderGroup grp,string prompt){
			var result = new TaskCompletionSource<Invader>();
			engine.decisions.Push(new SelectInvader(grp,prompt,result));
			return result.Task;
		}

		
	}

	public class SelectInvader : IDecision {
		readonly TaskCompletionSource<Invader> promise;
		public SelectInvader(InvaderGroup invaderGroup,string prompt,TaskCompletionSource<Invader> promise){
			Options = invaderGroup.InvaderTypesPresent.ToArray();
			Prompt = prompt;
			this.promise = promise;
			if(Options.Length == 0)
				promise.TrySetResult(null);
		}

		public string Prompt {get;}

		public IOption[] Options {get;}

		public void Select( IOption option ) { promise.TrySetResult((Invader)option); }

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