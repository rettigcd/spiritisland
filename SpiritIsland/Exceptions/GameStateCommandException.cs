namespace SpiritIsland;

public class GameStateCommandException( IGameStateCommand cmd ) : Exception() {
	public IGameStateCommand Cmd { get; } = cmd;

}

public interface IGameStateCommand {}
	
public class Rewind( int toRound ) : IGameStateCommand {
	public int TargetRound { get; } = toRound;
}