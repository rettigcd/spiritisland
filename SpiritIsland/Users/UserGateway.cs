using SpiritIsland.Log;

namespace SpiritIsland;

sealed public class UserGateway : IUserPortal, IEnginePortal {

	#region IUserPortal

	public event Action<IDecision> NewWaitingDecision;

	IDecisionMaker CacheNextDecision( int? waitMs ) {
		if(_userAccessedDecision == null) {
			WaitForSignal( waitMs );
			_userAccessedDecision = _activeDecisionMaker;
		}
		return _userAccessedDecision;
	}

	void WaitForSignal(int? milliseconds) {
		if(milliseconds.HasValue)
			_signal.WaitOne( milliseconds.Value );
		else
			_signal.WaitOne(); // normal
	}

	public bool WaitForNextDecision( int milliseconds ) {
		if(_signal.WaitOne( milliseconds )) {
			_userAccessedDecision = _activeDecisionMaker;
			return true;
		}
		return false;
	}


	/// <summary> Generates an exception in the engine that resets it back to beginning. </summary>
	public void GoBackToBeginningOfRound( int targetRound ) {
		var poppedDecisionMaker = CacheNextDecision( null );
		_activeDecisionMaker = null;
		_userAccessedDecision = null;
		poppedDecisionMaker.IssueCommand( new Rewind( targetRound ) );
	}

	#region Blocking

	public IDecision Next => CacheNextDecision( null )?.Decision;
	IDecision IUserPortal.Current => CacheNextDecision( 0 )?.Decision;

	public bool WaitForNext(int ms) => CacheNextDecision( ms ) != null;

	public bool IsResolved => _activeDecisionMaker == null;

	public void Choose(IDecision _, IOption selection,bool block=true) {
		var currentDecisionMaker = CacheNextDecision( null );
		if(currentDecisionMaker == null) return;
		IDecisionPlus currentDecision = currentDecisionMaker.Decision;
		_activeDecisionMaker = null;
		_userAccessedDecision = null;

		if(!currentDecision.Options.Contains( selection ))
			throw new ArgumentException( selection.Text + " not found in options("+ currentDecision.Options.Select(x=>x.Text).Join(",") + ")" );

		Log( new DecisionLogEntry(selection,currentDecision,false)  );

		currentDecisionMaker.Select( selection ); // ####
	}

	#endregion

	#endregion

	/// <remarks>
	/// When there is no match, this gets set to null.
	/// This is fine because:
	/// Whatever criteria prevented them from having a match, will also prevent having any matches 
	/// and this will not be referenced/used.
	/// </remarks>
	public SpaceToken Preloaded { get; set; }


	readonly public static AsyncLocal<bool> UsePreselect = new AsyncLocal<bool>(); // !! combine with other preferences into a 'preference' object.


	/// <summary>
	/// Caller presents a decision to the Gateway and waits for the gateway to return an choice.
	/// </summary>
	public Task<T> Select<T>( A.TypedDecision<T> originalDecision ) where T : class, IOption {
		ArgumentNullException.ThrowIfNull( originalDecision );

		if(_activeDecisionMaker != null) 
			throw new InvalidOperationException( $"Pending decision was not properly awaited. Current:[{originalDecision.Prompt}], Previous:[{_activeDecisionMaker.Decision.Prompt}] ");

		IDecisionPlus decision = originalDecision as IDecisionPlus;

		// Scenario 1 - No options => Auto-Select NULL
		if(decision.Options.Length == 0) {
			return Preloaded is not null
				? throw new InvalidOperationException($"Preloaded {Preloaded} but that is now not an option.")
				: Task.FromResult<T>( null );
		}

		// Scenario 2 - Resolve Promise with preloaded value.
		if(Preloaded != null) {
			if( !decision.Options.Contains(Preloaded) )
				throw new InvalidOperationException( $"Preloaded option {Preloaded.Text} not an option for "+decision.Prompt );
			IOption preloaded = Preloaded;
			Preloaded = null;
			return Task.FromResult<T>( (T)preloaded );
		}

		// Scenario 3 - Auto-Select Single
		if(decision.Options.Length == 1 && decision.AllowAutoSelect) {
			Log( new DecisionLogEntry( decision.Options[0], decision, true ) );
			return Task.FromResult<T>( (T)decision.Options[0] );
		}

		// Scenario 4 - Returns unresolved promise.
		var promise = new TaskCompletionSource<T>();
		var decisionMaker = new ActionHelper<T>( originalDecision, promise );
		_activeDecisionMaker = decisionMaker;
		_signal.Set(); // Signal UI there is a Decision to be made.
		NewWaitingDecision?.Invoke(decision);
		return promise.Task;
	}

	#region selection log / private

	void Log( DecisionLogEntry entry ) {
		DecisionMade?.Invoke(entry);
		selections.Add( entry.Msg(LogLevel.Info) );
	}

	public event Action<DecisionLogEntry> DecisionMade;
	public readonly List<string> selections = [];

	readonly AutoResetEvent _signal = new AutoResetEvent( false );
	IDecisionMaker _activeDecisionMaker;
	IDecisionMaker _userAccessedDecision;

	#endregion

	#region internal DecisionMaker

	interface IDecisionMaker {
		public IDecisionPlus Decision { get; }
		public void Select(IOption option);
		public void IssueCommand( IGameStateCommand cmd );
	}

	/// <summary>
	/// Assigns one of the generic available options to the typed pending promise.
	/// </summary>
	class ActionHelper<T>( IDecisionPlus decision, TaskCompletionSource<T> promise ) : IDecisionMaker where T : class, IOption {
		public IDecisionPlus Decision { get; } = decision;

		public void Select( IOption selection ) {
			if( TextOption.Done.Matches( selection ) || selection is not T tt )
				promise.TrySetResult( null );
			else if(Decision.Options.Contains( selection ))
				promise.TrySetResult( tt );
			else
				promise.TrySetException( new System.Exception( $"{selection.Text} not found in options" ) );
		}

		public void IssueCommand( IGameStateCommand cmd ) {
			promise.TrySetException( new GameStateCommandException(cmd) );
		}
	}

	#endregion

}