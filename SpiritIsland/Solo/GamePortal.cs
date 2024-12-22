namespace SpiritIsland.SinglePlayer;

public class GamePortal(IUserPortalPlus inner) : IGamePortal {

	public IDecisionPortal DecisionPortal => _inner;

	public void RewindToRound( int targetRound) {
		_inner.IssueException(new RewindException(targetRound));
	}

	public void CancelGame() {
		_inner.IssueException(new GameOverException(new GameOverLogEntry(GameOverResult.Withdrawal, "User withdrew from game.")));
	}

	readonly IUserPortalPlus _inner = inner;
}
