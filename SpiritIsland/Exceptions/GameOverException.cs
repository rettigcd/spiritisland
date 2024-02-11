using SpiritIsland.Log;

namespace SpiritIsland;

public class GameOverException( GameOverLogEntry status )
	: Exception( status.ToString() )
{
	static public void Win(string cause) => throw new GameOverException( new GameOverLogEntry( GameOverResult.Victory, cause ) );

	static public void Lost(string cause) => throw new GameOverException( new GameOverLogEntry( GameOverResult.Defeat, cause ) );

	public GameOverLogEntry Status { get; } = status;
}