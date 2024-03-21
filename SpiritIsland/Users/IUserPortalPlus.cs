namespace SpiritIsland;

/// <summary>
/// The users API for receiving Decisions and making choices.
/// </summary>
public interface IDecisionPortal {

	#region Waiting for Next Decision

	/// <summary> Waits given # of ms for the next decision. Then returns.</summary>
	bool WaitForNext(int ms);
	bool WaitForNextDecision(int milliseconds);

	/// <summary> Waits for the Next Decision to arrive, then returns it. </summary>
	IDecision Next { get; }
	event Action<IDecision> NewWaitingDecision;

	#endregion Waiting for Next Decision

	/// <summary> Returns current Decision or null if there is none. </summary>
	void Choose(IDecision decision, IOption option, bool block = true);

}

/// <summary>
/// Used By Game Engine to Cancel Task
/// </summary>
public interface IUserPortalPlus : IDecisionPortal {
	void IssueException( Exception ex );
}

/// <summary>
/// The API that the engine uses to present decisions to the user.
/// </summary>
public interface IEnginePortal {
	Task<T> Select<T>( A.TypedDecision<T> originalDecision ) where T : class;
}

