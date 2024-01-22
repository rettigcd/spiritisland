namespace SpiritIsland;

/// <summary>
/// Allows adding 1 or more Sync or Async
/// </summary>
public sealed class AsyncEvent<T> : IHaveMemento {

	public Guid Add( Func<T, Task> action ) {
		Guid key = Guid.NewGuid();
		this._handlers.Add( key, action );
		return key;
	}

	public Guid Add( Action<T> action ) {
		return this.Add( ( args ) => { action( args ); return Task.CompletedTask; } );
	}

	public async Task InvokeAsync( T t ) {
		foreach(Func<T, Task> handler in _handlers.Values)
			await handler( t );
	}

	public void Remove( Guid guid ) => _handlers.Remove( guid );

	public void Clear() => _handlers.Clear();

	readonly Dictionary<Guid, Func<T, Task>> _handlers = [];

	#region Memento

	object IHaveMemento.Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	class MyMemento( AsyncEvent<T> _src ) {
		public void Restore( AsyncEvent<T> src ) {
			src._handlers.Clear();
			foreach(var pair in handlers)
				src._handlers.Add(pair.Key,pair.Value);
		}
		readonly Dictionary<Guid, Func<T, Task>> handlers = new Dictionary<Guid, Func<T, Task>>( _src._handlers );
	}

	#endregion

}