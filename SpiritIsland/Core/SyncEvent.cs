namespace SpiritIsland;

public class SyncEvent<T> {
	public void Invoke( GameState gameState, T t ) {
		foreach(var handler in ForRound)
			handler( gameState, t );
		foreach(var handler in ForGame)
			handler( gameState, t );
	}
	public Task EndOfRound( GameState _ ) { ForRound.Clear(); return Task.CompletedTask; }
	public List<Action<GameState, T>> ForRound = [];
	public List<Action<GameState, T>> ForGame = [];
}
