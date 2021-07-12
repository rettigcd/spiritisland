using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Core {

	public class SelectAsync<T> : IDecision where T:class,IOption {

		readonly TaskCompletionSource<T> promise;

		public SelectAsync(
			string prompt,
			T[] options,
			bool allowShortCircuit,
			TaskCompletionSource<T> promise
		){
			Prompt = prompt;
			this.promise = promise;

			var optionList = options.Cast<IOption>().ToList();
			if(optionList.Count == 0)
				promise.TrySetResult(null);
			else if(allowShortCircuit)
				optionList.Add(TextOption.Done);
			Options = optionList.ToArray();
		}

		public string Prompt {get;}

		public IOption[] Options {get;}

		public void Select( IOption option ) { promise.TrySetResult((T)option); }

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