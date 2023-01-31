using SpiritIsland.Log;

namespace SpiritIsland;

sealed public class UserGateway : IUserPortal, IEnginePortal {

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

	public IDecision Next => CacheNextDecision( true )?.Decision;
	IDecision IUserPortal.Current => CacheNextDecision( false )?.Decision;

	public bool IsResolved => activeDecisionMaker == null;

	public void Choose(IDecision _, IOption selection,bool block=true) {
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

	public SpaceToken Preloaded { get; set; }

	/// <summary>
	/// Caller presents a decision to the Gateway and waits for the gateway to return an choice.
	/// </summary>
	public Task<T> Decision<T>( Select.TypedDecision<T> originalDecision ) where T : class, IOption {
		if(originalDecision == null) throw new ArgumentNullException( nameof( originalDecision ) );

		if(activeDecisionMaker != null) 
			throw new InvalidOperationException( $"Pending decision was not properly awaited. Current:[{originalDecision.Prompt}], Previous:[{activeDecisionMaker.Decision.Prompt}] ");

		var promise = new TaskCompletionSource<T>();
		var decisionMaker = new ActionHelper<T>( originalDecision, promise );
		var decision = decisionMaker.Decision;

		if(decision.Options.Length == 0)
			// Auto-Select NULL
			promise.TrySetResult( null );
		else if(Preloaded != null) {
			if(!decision.Options.Contains(Preloaded))
				throw new InvalidOperationException( $"Preloaded option {Preloaded.Text} not an option for "+decision.Prompt );
			IOption preloaded = Preloaded; Preloaded = null;
			decisionMaker.Select( preloaded );
		} else if(decision.Options.Length == 1 && decision.AllowAutoSelect) {
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
		selections.Add( entry.Msg(LogLevel.Info) );
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

	/// <summary>
	/// Assigns one of the generic available options to the typed pending promise.
	/// </summary>
	class ActionHelper<T> : IDecisionMaker where T : class, IOption {

		public ActionHelper( IDecisionPlus decision, TaskCompletionSource<T> promise ) {
			Decision = decision;
			this._pendingPromise = promise;
		}

		public IDecisionPlus Decision { get; }

		public void Select( IOption selection ) {
			if( TextOption.Done.Matches( selection ) || selection is not T tt )
				_pendingPromise.TrySetResult( null );
			else if(Decision.Options.Contains( selection ))
				_pendingPromise.TrySetResult( tt );
			else
				_pendingPromise.TrySetException( new System.Exception( $"{selection.Text} not found in options" ) );
		}

		public void IssueCommand( IGameStateCommand cmd ) {
			_pendingPromise.TrySetException( new GameStateCommandException(cmd) );
		}

		readonly TaskCompletionSource<T> _pendingPromise;

	}

	#endregion

}