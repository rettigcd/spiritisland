namespace SpiritIsland;

/// <summary>
/// The users API for receiving Decisions and making choices.
/// </summary>
public interface IUserPortal {

	public bool IsResolved { get; }
	bool WaitForNextDecision( int milliseconds );
	IDecision GetCurrent(bool block=true);

	void Choose( IOption option, bool block=true );

	void GoBackToBeginningOfRound( int targetRound );

	event Action<IDecision> NewWaitingDecision;

}

/// <summary>
/// The API that the engine uses to present decisions to the user.
/// </summary>
public interface IEnginePortal {
	Task<T> Decision<T>( Select.TypedDecision<T> originalDecision ) where T : class, IOption;
}

