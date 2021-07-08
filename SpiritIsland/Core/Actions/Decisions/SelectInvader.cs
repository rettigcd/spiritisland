using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {
	public class SelectInvader : IDecision {
		readonly TaskCompletionSource<Invader> promise;
		public SelectInvader(
			string prompt,
			Invader[] invaders,
			bool allowShortCircuit,
			TaskCompletionSource<Invader> promise
		){
			Prompt = prompt;
			this.promise = promise;

			var options = invaders.Cast<IOption>().ToList();
			if(options.Count == 0)
				promise.TrySetResult(null);
			else if(allowShortCircuit)
				options.Add(TextOption.Done);
			Options = options.ToArray();
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