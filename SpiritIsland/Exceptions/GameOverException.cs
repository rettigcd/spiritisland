namespace SpiritIsland;

public class GameOverException : Exception {

	static public void Win(string cause) => throw new GameOverException( new GameOver( GameOverResult.Victory, cause ) );

	static public void Lost(string cause) => throw new GameOverException( new GameOver( GameOverResult.Defeat, cause ) );


	public GameOverException( GameOver status ):base( status.Msg ) {
		Status = status;
	}
	public GameOver Status {get;}
}