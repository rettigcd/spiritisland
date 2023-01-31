using SpiritIsland.Log;

namespace SpiritIsland;

public class GameOverException : System.Exception {

	static public void Win(string cause) => throw new GameOverException( new GameOver( GameOverResult.Victory, cause ) );

	static public void Lost(string cause) => throw new GameOverException( new GameOver( GameOverResult.Defeat, cause ) );


	public GameOverException( GameOver status ):base( status.Msg( LogLevel.Info ) ) {
		Status = status;
	}
	public GameOver Status {get;}
}