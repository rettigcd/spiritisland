namespace SpiritIsland;

public class SyncEvent<T> {
	public void Invoke( GameState gameState, T t ) {
		foreach(var handler in ForRound)
			handler( gameState, t );
		foreach(var handler in ForGame)
			handler( gameState, t );
	}
	public void EndOfRound(GameState _) { ForRound.Clear(); }
	public List<Action<GameState, T>> ForRound = new List<Action<GameState, T>>();
	public List<Action<GameState, T>> ForGame = new List<Action<GameState, T>>();
}
