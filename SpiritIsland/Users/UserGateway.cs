#nullable enable
using SpiritIsland.Log;

namespace SpiritIsland;

sealed public class UserGateway : IUserPortalPlus, IEnginePortal {

	#region IUserPortal - wait for next decision

	public IDecision? Next => CacheNextDecision( null )?.Decision;
	public IDecision? Current => CacheNextDecision( 0 )?.Decision;
	public event Action<IDecision>? NewWaitingDecision;
	public bool WaitForNext( int ms ) => CacheNextDecision( ms ) != null;
	public bool WaitForNextDecision( int milliseconds ) { // !!!
		if(_signal.WaitOne( milliseconds )) {
			_userAccessedDecision = _activeDecisionMaker;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Will wait a certain period of time for non-null, then returns null.
	/// </summary>
	IDecisionMaker? CacheNextDecision( int? waitMs ) {
		if(_userAccessedDecision is null) {
			WaitForSignal( waitMs );
			_userAccessedDecision = _activeDecisionMaker;
		}
		return _userAccessedDecision;
	}
	void WaitForSignal( int? milliseconds ) {
		if(milliseconds.HasValue)
			_signal.WaitOne( milliseconds.Value );
		else
			_signal.WaitOne(); // normal
	}

	#endregion IUserPortal - wait for next decision

	#region IUserPortal - Choose

	public void Choose( IDecision _, IOption selection, bool block = true ) {
		// ** UI-Thread **

		var currentDecisionMaker = CacheNextDecision( null );
		if(currentDecisionMaker == null) 
			return;

		IDecisionPlus currentDecision = currentDecisionMaker.Decision;
		_activeDecisionMaker = null;
		_userAccessedDecision = null;

		if(!currentDecision.Options.Contains( selection ))
			throw new ArgumentException( selection.Text + " not found in options(" + currentDecision.Options.Select( x => x.Text ).Join( "," ) + ")" );

		currentDecisionMaker.Select( selection ); // ####
	}

	#endregion IUserPortal - Choose

	/// <summary> Generates an exception in the engine that resets it back to beginning. </summary>
	public void IssueException( Exception exception ) {
		IDecisionMaker? poppedDecisionMaker = CacheNextDecision( null );
		_activeDecisionMaker = null;
		_userAccessedDecision = null;
		poppedDecisionMaker?.IssueException( exception ); // !!! Might not Issue Exception if no decision ever shows up
	}

	/// <remarks>
	/// When there is no match, this gets set to null.
	/// This is fine because:
	/// Whatever criteria prevented them from having a match, will also prevent having any matches 
	/// and this will not be referenced/used.
	/// </remarks>
	public SpaceToken? PreloadedSpaceToken { get; set; }

	readonly public static AsyncLocal<bool> UsePreselect = new AsyncLocal<bool>(); // !! combine with other preferences into a 'preference' object.

	/// <summary>
	/// Caller presents a decision to the Gateway and waits for the gateway to return an choice.
	/// </summary>
	public async Task<T?> Select<T>( A.TypedDecision<T> decision ) where T : class {
		ArgumentNullException.ThrowIfNull( decision );

		if(_activeDecisionMaker != null) 
			throw new InvalidOperationException( $"Pending decision was not properly awaited. Current:[{decision.Prompt}], Previous:[{_activeDecisionMaker.Decision.Prompt}] ");

		// Scenario 1 - No options => Auto-Select NULL
		if(decision.Options.Length == 0)
			return PreloadedSpaceToken is null ? null
				: throw new InvalidOperationException($"Preloaded {PreloadedSpaceToken} but that is now not an option.");

		// Scenario 2 - Resolve Promise with preloaded value.
		if(PreloadedSpaceToken is not null) {
			if( !decision.Options.Contains(PreloadedSpaceToken) )
				throw new InvalidOperationException( $"Preloaded option {PreloadedSpaceToken} not an option for "+ decision.Prompt );
			IOption preloaded = PreloadedSpaceToken;
			PreloadedSpaceToken = null;

			Log(new DecisionLogEntry( preloaded, decision, false));
			return (T)preloaded;
		}

		// Scenario 3 - Auto-Select Single
		if(decision.Options.Length == 1 && decision.AllowAutoSelect) {
			Log( new DecisionLogEntry(decision.Options[0], decision, true ) );
			return decision.ConvertOptionToResult(decision.Options[0]);
		}

		// Scenario 4 - Returns unresolved promise.
		var promise = new TaskCompletionSource<T?>();
		var decisionMaker = new ActionHelper<T>( decision, promise );
		_activeDecisionMaker = decisionMaker;

		// We are signalling this twice

		// 1st Signal - autosetsignal - AutoResetEvent
		// non-blocking - GOOD
		_signal.Set(); // Signal UI there is a Decision to be made.

		// 2nd Signal - callback
		// was BLOCKING - BAD, caused problems updating the UI thread.

		NewWaitingDecision?.Invoke(decision);

		T? result = await promise.Task;
		if(result is not null)
			Log(new DecisionLogEntry((IOption)result, decision, false));
		return result;
	}

	#region selection log / private

	void Log( DecisionLogEntry entry ) {
		if(ActionScope.Current == null) throw new InvalidOperationException("Must have action scope");

		DecisionMade?.Invoke(entry);
		selections.Add( entry.Msg(LogLevel.Info) );
	}

	public event Action<DecisionLogEntry>? DecisionMade;
	public readonly List<string> selections = [];

	readonly AutoResetEvent _signal = new AutoResetEvent( false );
	#pragma warning disable CA1859
	// I can't set this to the <T> it described because it is generic
	IDecisionMaker? _activeDecisionMaker;
	#pragma warning restore CA1859
	IDecisionMaker? _userAccessedDecision;

	#endregion

	#region internal DecisionMaker

	interface IDecisionMaker {
		public IDecisionPlus Decision { get; }
		public void Select(IOption option);
		public void IssueException( Exception exception );
	}

	/// <summary>
	/// Assigns one of the generic available options to the typed pending promise.
	/// </summary>
	class ActionHelper<T>(A.TypedDecision<T> decision, TaskCompletionSource<T?> promise ) : IDecisionMaker where T : class {
		public IDecisionPlus Decision { get; } = decision;

		public void Select( IOption option ) {
			if( decision.TryGetResultFromOption(option,out T? result) )
				promise.TrySetResult( result );
			else
				promise.TrySetException( new System.Exception( $"{option.Text} not found in options" ) );
		}

		public void IssueException( Exception exception ) {
			promise.TrySetException( exception );
		}
	}

	#endregion

}