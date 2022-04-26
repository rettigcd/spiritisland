namespace SpiritIsland;

sealed public class ActionGateway : IUserPortal, IEnginePortal {

	#region IUserPortal

	public event Action<IDecision> NewWaitingDecision;

	IDecisionMaker CacheNextDecision( bool block ) {
		if(userAccessedDecision == null) {
			WaitForSignal(block);
			userAccessedDecision = activeDecisionMaker;
		}
		return userAccessedDecision;
	}

	void WaitForSignal(bool block ) {
		if(block)
	 		signal.WaitOne(); // normal
		else
			signal.WaitOne(0);
	}

	public bool WaitForNextDecision( int milliseconds ) {
		if(signal.WaitOne( milliseconds )) {
			userAccessedDecision = activeDecisionMaker;
			return true;
		}
		return false;
	}


	/// <summary> Generates an exception in the engine that resets it back to beginning. </summary>
	public void GoBackToBeginningOfRound( int targetRound ) {
		var poppedDecisionMaker = CacheNextDecision(true);
		this.activeDecisionMaker = null;
		this.userAccessedDecision = null;
		poppedDecisionMaker.IssueCommand( new Rewind( targetRound ) );
	}

	#region Blocking

	/// <summary>
	/// Blocks and waits for there to be a decision. 
	/// Don't call unless you are willing to block.
	/// </summary>
	public IDecision GetCurrent(bool block=true) => CacheNextDecision(block)?.Decision;

	public bool IsResolved => activeDecisionMaker == null;

	public void Choose(IOption selection,bool block=true) {
		var currentDecisionMaker = CacheNextDecision(block);
		if(currentDecisionMaker == null) return;
		var currentDecision = currentDecisionMaker.Decision;
		this.activeDecisionMaker = null;
		this.userAccessedDecision = null;

		if(!currentDecision.Options.Contains( selection ))
			throw new ArgumentException( selection.Text + " not found in options("+ currentDecision.Options.Select(x=>x.Text).Join(",") + ")" );

		Log( new DecisionLogEntry(selection,currentDecision,false)  );

		currentDecisionMaker.Select( selection ); // ####
	}

	#endregion

	#endregion

	/// <summary>
	/// Caller presents a decision to the Gateway and waits for the gateway to return an choice.
	/// </summary>
	public Task<T> Decision<T>( Select.TypedDecision<T> originalDecision ) where T : class, IOption {
		if(originalDecision == null) throw new ArgumentNullException( nameof( originalDecision ) );

		if(activeDecisionMaker != null) 
			throw new InvalidOperationException( $"Pending decision was not properly awaited. Current:[{activeDecisionMaker.Decision.Prompt}], Previous:[{originalDecision.Prompt}] ");

		var promise = new TaskCompletionSource<T>();
		var decisionMaker = new ActionHelper<T>( originalDecision, promise );
		var decision = decisionMaker.Decision;

		if(decision.Options.Length == 0)
			// Auto-Select NULL
			promise.TrySetResult( null );

		else if(decision.Options.Length == 1 && decision.AllowAutoSelect) {
			// Auto-Select Single
			decisionMaker.Select( decision.Options[0] );
			Log( new DecisionLogEntry( decision.Options[0], decision, true ) );

		} else {
			activeDecisionMaker = decisionMaker;
			signal.Set();
			NewWaitingDecision?.Invoke(decision);
		}
		return promise.Task;
	}

	#region selection log / private

	void Log( DecisionLogEntry entry ) {
		DecisionMade?.Invoke(entry);
		selections.Add( entry.Msg() );
	}

	public event Action<DecisionLogEntry> DecisionMade;
	public readonly List<string> selections = new List<string>();

	readonly AutoResetEvent signal = new AutoResetEvent( false );
	IDecisionMaker activeDecisionMaker;
	IDecisionMaker userAccessedDecision;

	#endregion

	#region internal DecisionMaker

	interface IDecisionMaker {
		public IDecisionPlus Decision { get; }
		public void Select(IOption option);
		public void IssueCommand( IGameStateCommand cmd );
	}

	class ActionHelper<T> : IDecisionMaker where T : class, IOption {

		public IDecisionPlus Decision { get; }

		public ActionHelper( IDecisionPlus decision, TaskCompletionSource<T> promise ) {
			Decision = decision;
			this.promise = promise;
		}

		public void Select( IOption selection ) {
			if( TextOption.Done.Matches( selection ) || selection is not T tt )
				promise.TrySetResult( null );
			else if(Decision.Options.Contains( selection ))
				promise.TrySetResult( tt );
			else
				promise.TrySetException( new Exception( $"{selection.Text} not found in options" ) );
		}

		public void IssueCommand( IGameStateCommand cmd ) {
			promise.TrySetException( new GameStateCommandException(cmd) );
		}

		readonly TaskCompletionSource<T> promise;

	}

	#endregion

}

public class DecisionLogEntry : ILogEntry {

	readonly IOption selection;
	readonly IDecision decision;
	readonly bool auto;

	public DecisionLogEntry(IOption selection, IDecision decision, bool auto ) {
		this.selection = selection;
		this.decision = decision;
		this.auto = auto;
	}

	public string Msg( LogLevel level ) {
		// Fatal/Error/Warn/Info
		if( level <= LogLevel.Info)
			return decision.Prompt + ": " + selection.Text;

		// Debug/All
		string msg = decision.Prompt + "(" + decision.Options.Select( o => o.Text ).Join( "," ) + "):" + selection.Text;
		if(auto) msg += " AUTO";
		return msg;
	}

	public LogLevel Level => LogLevel.Info;
}
