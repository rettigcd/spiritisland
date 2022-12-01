namespace SpiritIsland;

static public class AsyncEventExtensions {
	static public async Task InvokeInSeries<Arg>( this Func<Arg, Task> func, Arg arg ) {
		// if(func == null) return; instead, use ?. notation to invoke:  myEvent?.InvokeInSeries(arg)
		var handlers = func.GetInvocationList().Cast<Func<Arg, Task>>();
		foreach(var handler in handlers)
			await handler( arg );
	}

	static public async Task InvokeInParallel<Arg>( this Func<Arg, Task> func, Arg arg ) {
		// if(func == null) return; instead, use ?. notation to invoke:  myEvent?.InvokeInSeries(arg)
		var handlers = func.GetInvocationList().Cast<Func<Arg, Task>>();
		await Task.WhenAll( handlers.Select( handler => handler( arg ) ).ToArray() );
	}
}
