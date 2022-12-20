namespace SpiritIsland;

/// <summary>
/// The users API for receiving Decisions and making choices.
/// </summary>
public interface IUserPortal {

	public bool IsResolved { get; }
	bool WaitForNextDecision( int milliseconds );


	/// <summary> Waits for the Next Decision to arrive, then returns it. </summary>
	IDecision Next { get; }
	/// <summary> Returns current Decision or null if there is none. </summary>
	IDecision Current { get; }

	void Choose( IDecision decision, IOption option, bool block=true );

	void GoBackToBeginningOfRound( int targetRound );

	event Action<IDecision> NewWaitingDecision;

}

/// <summary>
/// The API that the engine uses to present decisions to the user.
/// </summary>
public interface IEnginePortal {
	Task<T> Decision<T>( Select.TypedDecision<T> originalDecision ) where T : class, IOption;
}

