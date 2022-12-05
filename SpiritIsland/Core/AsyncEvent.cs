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
		// !!! is there a way we can catch this exception and log it?
//		try {
			await handler( t );
//		}
//		catch( Exception ex ) {
//		}
	}

	readonly Dictionary<Guid, Func<T, Task>> handlers = new Dictionary<Guid, Func<T, Task>>();

	#region Memento

	public virtual IMemento<AsyncEvent<T>> SaveToMemento() => new Memento( this );
	public virtual void LoadFrom( IMemento<AsyncEvent<T>> memento ) => ((Memento)memento).Restore( this );

	protected class Memento : IMemento<AsyncEvent<T>> {
		public Memento( AsyncEvent<T> src ) {
			handlers = new Dictionary<Guid, Func<T, Task>>(src.handlers);
		}
		public void Restore( AsyncEvent<T> src ) {
			src.handlers.Clear();
			foreach(var pair in handlers)
				src.handlers.Add(pair.Key,pair.Value);
		}
		readonly Dictionary<Guid, Func<T, Task>> handlers = new Dictionary<Guid, Func<T, Task>>();
	}

	#endregion

}