namespace SpiritIsland;

public class AsyncEvent<T> {

	public Guid Add( Func<T, Task> action ) {
		Guid key = Guid.NewGuid();
		this.handlers.Add( key, action );
		return key;
	}

	public Guid Add( Action<T> action ) {
		return this.Add( ( args ) => { action( args ); return Task.CompletedTask; } );
	}

	public void Remove( Guid guid ) => handlers.Remove( guid );

	public async Task InvokeAsync( T t ) {
		foreach(var handler in handlers.Values)
			await TryHandle( handler, t );
	}

	public void Clear(GameState _) => Clear(); // convenience for adding to TimePasses_WholeGame

	public void Clear() => handlers.Clear();

	static async Task TryHandle( Func<T, Task> handler, T t ) {
		try {
			await handler( t );
		}
		catch(Exception) {
			// !! should do something with this...
		}
	}

	readonly Dictionary<Guid, Func<T, Task>> handlers = new Dictionary<Guid, Func<T, Task>>();
}