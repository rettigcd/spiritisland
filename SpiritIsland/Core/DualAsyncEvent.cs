namespace SpiritIsland;

public class DualAsyncEvent<T> {

	public async Task InvokeAsync( T t ) {
		await ForRound.InvokeAsync(t);
		await ForGame.InvokeAsync(t);
	}

	public AsyncEvent<T> ForRound { get; set; } = new SpiritIsland.AsyncEvent<T>();

	public AsyncEvent<T> ForGame { get; set; } = new SpiritIsland.AsyncEvent<T>();

	#region Memento

	public virtual IMemento<DualAsyncEvent<T>> SaveToMemento() => new Memento( this );
	public virtual void LoadFrom( IMemento<DualAsyncEvent<T>> memento ) => ((Memento)memento).Restore( this );

	protected class Memento : IMemento<DualAsyncEvent<T>> {
		public Memento( DualAsyncEvent<T> src ) {
			forGame = src.ForGame.SaveToMemento();
			forRound = src.ForRound.SaveToMemento();
		}
		public void Restore( DualAsyncEvent<T> src ) {
			src.ForGame.LoadFrom( forGame );
			src.ForRound.LoadFrom( forRound );
		}
		readonly IMemento<AsyncEvent<T>> forGame;
		readonly IMemento<AsyncEvent<T>> forRound;
	}

	#endregion

}