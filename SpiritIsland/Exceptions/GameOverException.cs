using SpiritIsland.Log;

namespace SpiritIsland;

public class GameOverException( GameOver status ) : System.Exception( status.Msg( LogLevel.Info ) ) {

	static public void Win(string cause) => throw new GameOverException( new GameOver( GameOverResult.Victory, cause ) );

	static public void Lost(string cause) => throw new GameOverException( new GameOver( GameOverResult.Defeat, cause ) );

	public GameOver Status { get; } = status;
}